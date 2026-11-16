using System.Text;
using ServerLogAnalyzer;

// Configure console encoding and window title for clean Unicode output
Console.OutputEncoding = Encoding.UTF8;
Console.Title = "Minecraft Server Log Analyzer (Purpur / Paper / Spigot)";

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("====================================================");
Console.WriteLine("   MINECRAFT SERVER LOG ANALYZER (Purpur / Paper)");
Console.WriteLine("====================================================");
Console.ResetColor();

Console.WriteLine("Enter path to latest.log (or drag & drop the file here).");
Console.Write("Press [Enter] to test with a built-in sample Purpur log: ");

// Strip surrounding quotes added automatically when dragging a file into Windows Terminal
string? input = Console.ReadLine()?.Trim().Trim('"');
string logPath;

if (string.IsNullOrWhiteSpace(input))
{
    logPath = SampleLogProvider.EnsureSampleLogExists();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"\nUsing generated sample log: {logPath}");
    Console.ResetColor();
}
else
{
    logPath = input;
}

if (!File.Exists(logPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\nError: File not found at '{logPath}'.");
    Console.ResetColor();
    return;
}

// Run log analysis and export structured JSON summary next to the executable
var analyzer = new LogAnalyzerService();
AnalysisReport report = analyzer.AnalyzeFile(logPath);

string jsonOutputPath = Path.Combine(AppContext.BaseDirectory, "analysis-report.json");
analyzer.ExportToJson(report, jsonOutputPath);

PrintReport(report, jsonOutputPath);

/// <summary>
/// Renders a human-readable, color-coded operational summary to the console.
/// </summary>
static void PrintReport(AnalysisReport report, string jsonPath)
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("--- SUMMARY ----------------------------------------");
    Console.ResetColor();
    Console.WriteLine($"Total lines processed : {report.TotalLinesProcessed:N0}");
    Console.WriteLine($"Parsed log entries    : {report.ParsedEntriesCount:N0}");
    Console.WriteLine($"Time span             : {report.FirstTimestamp ?? "N/A"} -> {report.LastTimestamp ?? "N/A"}");

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"INFO messages         : {report.InfoCount:N0}");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"WARN messages         : {report.WarnCount:N0}");
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"ERROR / FATAL         : {report.ErrorCount:N0}");
    Console.ResetColor();

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("--- PLAYER ACTIVITY --------------------------------");
    Console.ResetColor();
    Console.WriteLine($"Total logins          : {report.TotalLogins}");
    Console.WriteLine($"Unique players ({report.UniquePlayers.Count})    : {(report.UniquePlayers.Count > 0 ? string.Join(", ", report.UniquePlayers) : "None")}");

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("--- PERFORMANCE & LAG SPIKES -----------------------");
    Console.ResetColor();
    Console.WriteLine($"Overload warnings     : {report.LagSpikes.Count}");
    if (report.LagSpikes.Count > 0)
    {
        var worst = report.LagSpikes.MaxBy(s => s.BehindMs)!;
        Console.WriteLine($"Worst lag spike       : {worst.BehindMs} ms ({worst.BehindTicks} ticks) at {worst.Timestamp}");
    }

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("--- TOP PLUGINS WITH WARNINGS / ERRORS -------------");
    Console.ResetColor();
    if (report.PluginIssues.Count == 0)
    {
        Console.WriteLine("No plugin-specific warnings or errors detected.");
    }
    else
    {
        foreach (var plugin in report.PluginIssues)
        {
            Console.WriteLine($"- {plugin.PluginName,-20} | Errors: {plugin.ErrorCount,-4} | Warnings: {plugin.WarningCount}");
        }
    }

    if (report.RecentErrors.Count > 0)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("--- LATEST ERRORS ----------------------------------");
        Console.ResetColor();
        foreach (var err in report.RecentErrors)
        {
            Console.WriteLine($"[{err.Timestamp}] [{err.Plugin ?? err.Thread}] {err.Message}");
            if (!string.IsNullOrWhiteSpace(err.StackTracePreview))
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"   -> {err.StackTracePreview}");
                Console.ResetColor();
            }
        }
    }

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"Full JSON report exported to: {jsonPath}");
    Console.ResetColor();
}