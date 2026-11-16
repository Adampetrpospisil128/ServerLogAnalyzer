using System.Text.Json;

namespace ServerLogAnalyzer;

/// <summary>
/// Streams server log files line-by-line to keep memory usage low on large logs
/// and aggregates operational metrics using LINQ.
/// </summary>
public class LogAnalyzerService
{
    private readonly LogParser _parser = new();
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Reads and analyzes the specified log file, attaching multi-line Java stack traces
    /// to their parent log entry.
    /// </summary>
    public AnalysisReport AnalyzeFile(string filePath)
    {
        int totalLines = 0;
        var entries = new List<LogEntry>();
        LogEntry? lastEntry = null;

        // StreamReader avoids loading multi-megabyte log files entirely into RAM
        using var reader = new StreamReader(filePath);
        string? line;

        while ((line = reader.ReadLine()) is not null)
        {
            totalLines++;
            LogEntry? parsed = _parser.ParseLine(line);

            if (parsed is not null)
            {
                entries.Add(parsed);
                lastEntry = parsed;
            }
            else if (lastEntry is not null && !string.IsNullOrWhiteSpace(line))
            {
                // Lines without a [HH:mm:ss] header belong to the preceding exception stack trace
                lastEntry.StackTraceLines.Add(line.Trim());
            }
        }

        return BuildReport(filePath, totalLines, entries);
    }

    /// <summary>
    /// Serializes the structured analysis report to a formatted JSON file.
    /// </summary>
    public void ExportToJson(AnalysisReport report, string outputPath)
    {
        string json = JsonSerializer.Serialize(report, JsonOptions);
        File.WriteAllText(outputPath, json);
    }

    private AnalysisReport BuildReport(string filePath, int totalLines, List<LogEntry> entries)
    {
        int infoCount = entries.Count(e => e.Level == ServerLogLevel.Info);
        int warnCount = entries.Count(e => e.Level == ServerLogLevel.Warn);
        int errorCount = entries.Count(e => e.Level is ServerLogLevel.Error or ServerLogLevel.Fatal);

        var uniquePlayers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int totalLogins = 0;
        var lagSpikes = new List<LagSpikeEvent>();

        foreach (var entry in entries)
        {
            if (_parser.TryExtractPlayerLogin(entry.Message, out string player))
            {
                totalLogins++;
                uniquePlayers.Add(player);
            }

            if (_parser.TryExtractLagSpike(entry.Timestamp, entry.Message, out LagSpikeEvent? spike) && spike is not null)
            {
                lagSpikes.Add(spike);
            }
        }

        // Group warnings and errors by plugin name and rank the most problematic plugins first
        var pluginIssues = entries
            .Where(e => e.Plugin is not null && e.Level is ServerLogLevel.Warn or ServerLogLevel.Error or ServerLogLevel.Fatal)
            .GroupBy(e => e.Plugin!)
            .Select(g => new PluginIssueStat(
                PluginName: g.Key,
                ErrorCount: g.Count(e => e.Level is ServerLogLevel.Error or ServerLogLevel.Fatal),
                WarningCount: g.Count(e => e.Level == ServerLogLevel.Warn)))
            .OrderByDescending(p => p.ErrorCount)
            .ThenByDescending(p => p.WarningCount)
            .ToList();

        // Capture the 5 most recent errors with the first line of their Java exception for quick triage
        var recentErrors = entries
            .Where(e => e.Level is ServerLogLevel.Error or ServerLogLevel.Fatal)
            .TakeLast(5)
            .Select(e => new ErrorSummary(
                Timestamp: e.Timestamp,
                Thread: e.Thread,
                Plugin: e.Plugin,
                Message: e.Message,
                StackTracePreview: e.StackTraceLines.FirstOrDefault()))
            .ToList();

        return new AnalysisReport(
            SourceFile: Path.GetFileName(filePath),
            AnalyzedAt: DateTime.Now,
            TotalLinesProcessed: totalLines,
            ParsedEntriesCount: entries.Count,
            FirstTimestamp: entries.FirstOrDefault()?.Timestamp,
            LastTimestamp: entries.LastOrDefault()?.Timestamp,
            InfoCount: infoCount,
            WarnCount: warnCount,
            ErrorCount: errorCount,
            TotalLogins: totalLogins,
            UniquePlayers: uniquePlayers.OrderBy(p => p).ToList(),
            LagSpikes: lagSpikes,
            PluginIssues: pluginIssues,
            RecentErrors: recentErrors);
    }
}