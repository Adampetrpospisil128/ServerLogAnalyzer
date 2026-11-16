# Minecraft Server Log Analyzer (C#)

A fast, memory-efficient CLI tool built in C# (.NET 10) for parsing and analyzing `latest.log` files from **Purpur**, **Paper**, **Spigot**, and **Vanilla** Minecraft servers.
<img width="2598" height="1386" alt="image" src="https://github.com/user-attachments/assets/b9a77508-b8cb-4db2-b85e-e559e38b152e" />

## Features
- **Streaming File Processing:** Uses `StreamReader` to parse multi-megabyte log files line-by-line with minimal RAM overhead.
- **Compiled Regex Parsing:** Extracts timestamps, server threads (including async chat/IO threads), severity levels, and `[PluginName]` prefixes using source-generated regular expressions (`[GeneratedRegex]`).
- **Java Stack Trace Grouping:** Automatically attaches unprefixed multi-line Java exception stack traces to their parent `ERROR` or `WARN` log entry.
- **Performance & Player Analytics:** Tracks unique players, login counts, TPS overload warnings (`Can't keep up!`), and ranks plugins by error/warning frequency using `LINQ`.
- **JSON Report Export:** Outputs a structured `analysis-report.json` summary alongside a color-coded terminal report.
- **Built-in Sample Log:** Generates a realistic sample Purpur server log when run without arguments for instant testing.

## Project Structure
- `Program.cs` – CLI entry point, file path handling, and color-coded console output.
- `LogAnalyzerService.cs` – Streaming file reader, stack-trace aggregation, `LINQ` analytics, and JSON export.
- `LogParser.cs` – Source-generated regular expressions for Log4j2 lines, player joins, and lag spikes.
- `LogEntry.cs` – Data model for parsed log entries and severity levels.
- `AnalysisReport.cs` – Immutable `record` types representing the final report and JSON schema.
- `SampleLogProvider.cs` – Generates `sample-purpur.log` for out-of-the-box testing.

## How to Run
1. Clone the repository:
   ```bash
   git clone [https://github.com/Adampetrpospisil128/ServerLogAnalyzer.git](https://github.com/Adampetrpospisil128/ServerLogAnalyzer.git)
