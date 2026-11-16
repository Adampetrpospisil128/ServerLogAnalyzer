namespace ServerLogAnalyzer;

/// <summary>
/// Creates a realistic Purpur server sample log file on disk so users and recruiters
/// can test the analyzer immediately without needing an existing server directory.
/// </summary>
public static class SampleLogProvider
{
    /// <summary>
    /// Writes sample-purpur.log next to the executable and returns its full path.
    /// </summary>
    public static string EnsureSampleLogExists()
    {
        string samplePath = Path.Combine(AppContext.BaseDirectory, "sample-purpur.log");

        const string sampleContent = """
            [16:00:01] [ServerMain/INFO]: Environment: Environment[sessionHost=https://sessionserver.mojang.com]
            [16:00:03] [Server thread/INFO]: Starting minecraft server version 1.21.4
            [16:00:03] [Server thread/INFO]: This server is running Purpur version 1.21.4-2380-HEAD@a1b2c3d (2025-02-10T18:12:00Z)
            [16:00:04] [Server thread/INFO]: [LuckPerms] Loading server plugin LuckPerms v5.4.150
            [16:00:05] [Server thread/INFO]: [Essentials] Loading server plugin Essentials v2.21.0
            [16:00:05] [Server thread/INFO]: [Citizens] Loading server plugin Citizens v2.0.37-SNAPSHOT
            [16:00:08] [Server thread/INFO]: Done (5.142s)! For help, type "help"
            [16:05:12] [Server thread/INFO]: Alex[/192.168.1.24:51234] logged in with entity id 128 at ([world]120.5, 65.0, -44.2)
            [16:07:45] [Async Chat Thread - #0/INFO]: <Alex> Ahoj, funguje spawn?
            [16:12:30] [Server thread/INFO]: Steve[/192.168.1.38:60112] logged in with entity id 245 at ([world]118.0, 65.0, -40.0)
            [16:15:02] [Server thread/WARN]: [Essentials] File motd.txt does not exist. Creating one for you.
            [16:18:44] [Server thread/WARN]: Can't keep up! Is the server overloaded? Running 3420ms or 68 ticks behind
            [16:22:10] [Server thread/ERROR]: [Citizens] Unhandled exception executing task for Citizens v2.0.37
            java.lang.NullPointerException: Cannot invoke "org.bukkit.Location.getWorld()" because "loc" is null
            	at net.citizensnpcs.npc.CitizensNPC.spawn(CitizensNPC.java:142) ~[Citizens.jar:?]
            	at net.citizensnpcs.Citizens.onEnable(Citizens.java:89) ~[Citizens.jar:?]
            [16:25:19] [Server thread/WARN]: [Citizens] Could not load NPC ID 14 in unloaded world 'event_world'.
            [16:30:01] [Server thread/INFO]: Alex lost connection: Disconnected
            [16:32:10] [Server thread/INFO]: Alex[/192.168.1.24:51890] logged in with entity id 412 at ([world]120.5, 65.0, -44.2)
            [16:40:00] [Server thread/WARN]: Can't keep up! Is the server overloaded? Running 5120ms or 102 ticks behind
            """;

        File.WriteAllText(samplePath, sampleContent);
        return samplePath;
    }
}