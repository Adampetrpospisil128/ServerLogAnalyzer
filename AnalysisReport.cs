namespace ServerLogAnalyzer;

/// <summary>
/// Aggregated error and warning counts grouped by a specific server plugin.
/// </summary>
public record PluginIssueStat(string PluginName, int ErrorCount, int WarningCount);

/// <summary>
/// Captures a single "Can't keep up!" tick-overload warning with lag duration in milliseconds and ticks.
/// </summary>
public record LagSpikeEvent(string Timestamp, int BehindMs, int BehindTicks);

/// <summary>
/// Condensed error entry stored in the report and exported to JSON.
/// </summary>
public record ErrorSummary(
    string Timestamp,
    string Thread,
    string? Plugin,
    string Message,
    string? StackTracePreview);

/// <summary>
/// Complete structured analysis output generated from a Minecraft server log file.
/// </summary>
public record AnalysisReport(
    string SourceFile,
    DateTime AnalyzedAt,
    int TotalLinesProcessed,
    int ParsedEntriesCount,
    string? FirstTimestamp,
    string? LastTimestamp,
    int InfoCount,
    int WarnCount,
    int ErrorCount,
    int TotalLogins,
    List<string> UniquePlayers,
    List<LagSpikeEvent> LagSpikes,
    List<PluginIssueStat> PluginIssues,
    List<ErrorSummary> RecentErrors);