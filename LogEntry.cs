namespace ServerLogAnalyzer;

/// <summary>
/// Standard Log4j2 severity levels emitted by Purpur, Paper, Spigot, and Vanilla servers.
/// </summary>
public enum ServerLogLevel
{
    Info,
    Warn,
    Error,
    Fatal,
    Debug
}

/// <summary>
/// Represents a single parsed timestamped line from latest.log,
/// along with any multi-line Java exception stack trace lines that follow it.
/// </summary>
public class LogEntry
{
    /// <summary>Time when the log event occurred (HH:mm:ss).</summary>
    public string Timestamp { get; init; } = string.Empty;

    /// <summary>Server thread name (e.g., "Server thread" or "Async Chat Thread - #0").</summary>
    public string Thread { get; init; } = string.Empty;

    /// <summary>Severity classification of the entry.</summary>
    public ServerLogLevel Level { get; init; }

    /// <summary>Plugin prefix extracted from [PluginName], or null for core server messages.</summary>
    public string? Plugin { get; init; }

    /// <summary>Main log message payload.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Unprefixed continuation lines belonging to a Java stack trace.</summary>
    public List<string> StackTraceLines { get; } = [];
}