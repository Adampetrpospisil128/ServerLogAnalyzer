using System.Text.RegularExpressions;

namespace ServerLogAnalyzer;

/// <summary>
/// Parses raw Log4j2 lines from Purpur, Paper, Spigot, and Vanilla logs
/// using source-generated compiled regular expressions for high throughput.
/// </summary>
public partial class LogParser
{
    // Matches Log4j2 lines: [HH:mm:ss] [Thread/LEVEL]: [OptionalPlugin] Message
    // Also supports optional YYYY-MM-DD date prefixes added by some hosting panels.
    [GeneratedRegex(
        @"^\[(?:\d{4}-\d{2}-\d{2}\s+)?(?<time>\d{2}:\d{2}:\d{2})\]\s+\[(?<thread>[^/\]]+)/(?<level>INFO|WARN|ERROR|FATAL|DEBUG)\]:\s+(?:\[(?<plugin>[A-Za-z0-9_\-\.]+)\]\s+)?(?<message>.*)$",
        RegexOptions.Compiled)]
    private static partial Regex LogLineRegex();

    // Matches standard Bukkit/Paper/Purpur player join lines: PlayerName[/IP:Port] logged in with entity id ...
    [GeneratedRegex(
        @"^(?<player>[A-Za-z0-9_]{3,16})\[/[^\]]+\]\s+logged in with entity id",
        RegexOptions.Compiled)]
    private static partial Regex PlayerLoginRegex();

    // Matches main-thread tick overload warnings emitted when server TPS drops
    [GeneratedRegex(
        @"Can't keep up! Is the server overloaded\? Running (?<ms>\d+)ms or (?<ticks>\d+) ticks behind",
        RegexOptions.Compiled)]
    private static partial Regex LagSpikeRegex();

    /// <summary>
    /// Parses a single timestamped line into a structured <see cref="LogEntry"/>.
    /// Returns null for unprefixed continuation lines (such as Java exception stack traces).
    /// </summary>
    public LogEntry? ParseLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;

        Match match = LogLineRegex().Match(line);
        if (!match.Success)
            return null;

        ServerLogLevel level = match.Groups["level"].Value switch
        {
            "INFO" => ServerLogLevel.Info,
            "WARN" => ServerLogLevel.Warn,
            "ERROR" => ServerLogLevel.Error,
            "FATAL" => ServerLogLevel.Fatal,
            "DEBUG" => ServerLogLevel.Debug,
            _ => ServerLogLevel.Info
        };

        string? plugin = match.Groups["plugin"].Success
            ? match.Groups["plugin"].Value
            : null;

        return new LogEntry
        {
            Timestamp = match.Groups["time"].Value,
            Thread = match.Groups["thread"].Value,
            Level = level,
            Plugin = plugin,
            Message = match.Groups["message"].Value
        };
    }

    /// <summary>
    /// Extracts the player username if the message represents a successful login event.
    /// </summary>
    public bool TryExtractPlayerLogin(string message, out string playerName)
    {
        Match match = PlayerLoginRegex().Match(message);
        if (match.Success)
        {
            playerName = match.Groups["player"].Value;
            return true;
        }

        playerName = string.Empty;
        return false;
    }

    /// <summary>
    /// Extracts millisecond and tick lag metrics if the message is a server overload warning.
    /// </summary>
    public bool TryExtractLagSpike(string timestamp, string message, out LagSpikeEvent? lagEvent)
    {
        Match match = LagSpikeRegex().Match(message);
        if (match.Success)
        {
            int ms = int.Parse(match.Groups["ms"].Value);
            int ticks = int.Parse(match.Groups["ticks"].Value);
            lagEvent = new LagSpikeEvent(timestamp, ms, ticks);
            return true;
        }

        lagEvent = null;
        return false;
    }
}