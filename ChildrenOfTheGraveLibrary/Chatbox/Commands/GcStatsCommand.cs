using System;
using System.Runtime;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    /// <summary>
    /// Chat command to display garbage collection statistics and memory usage.
    /// </summary>
    public class GcStatsCommand : ChatCommandBase
    {
        public override string Command => "gcstats";
        public override string Syntax => $"{Command}";

        public GcStatsCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            // Get current memory usage before collection
            var beforeGC = GC.GetTotalMemory(false);

            // Get generation counts
            var gen0Count = GC.CollectionCount(0);
            var gen1Count = GC.CollectionCount(1);
            var gen2Count = GC.CollectionCount(2);

            // Get total memory
            var totalMemory = GC.GetTotalMemory(false);

            // Get GC mode information
            var gcMode = GCSettings.IsServerGC ? "Server GC" : "Workstation GC";
            var latencyMode = GCSettings.LatencyMode.ToString();

            // Format memory sizes to human-readable format
            var totalMemoryMB = totalMemory / (1024.0 * 1024.0);

            // Send statistics to chat
            ChatManager.System(userId, "[GC Stats] Memory and Garbage Collection Information:");
            ChatManager.System(userId, $"[GC Stats] Total Memory: {totalMemoryMB:F2} MB");
            ChatManager.System(userId, $"[GC Stats] Generation 0 Collections: {gen0Count}");
            ChatManager.System(userId, $"[GC Stats] Generation 1 Collections: {gen1Count}");
            ChatManager.System(userId, $"[GC Stats] Generation 2 Collections: {gen2Count}");
            ChatManager.System(userId, $"[GC Stats] GC Mode: {gcMode}");
            ChatManager.System(userId, $"[GC Stats] Latency Mode: {latencyMode}");

            // Additional memory info
            var workingSet = Environment.WorkingSet / (1024.0 * 1024.0);
            ChatManager.System(userId, $"[GC Stats] Working Set: {workingSet:F2} MB");

            // Show if LOH (Large Object Heap) compaction is enabled
            var lohCompactionMode = GCSettings.LargeObjectHeapCompactionMode.ToString();
            ChatManager.System(userId, $"[GC Stats] LOH Compaction Mode: {lohCompactionMode}");
        }
    }
}