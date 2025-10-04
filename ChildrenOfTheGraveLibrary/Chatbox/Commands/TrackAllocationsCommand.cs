using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Performance;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class TrackAllocationsCommand : ChatCommandBase
    {
        public override string Command => "trackalloc";
        public override string Syntax => "!trackalloc [0/1]";

        public TrackAllocationsCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var champion = Game.PlayerManager.GetPeerInfo(userId).Champion;

            if (!hasReceivedArguments)
            {
                ChatManager.System(userId, "Usage: !trackalloc [0/1]");
                return;
            }

            bool enable = arguments.Trim() == "1";
            AllocationTracker.Enable(enable);

            ChatManager.System(userId, $"Allocation tracking {(enable ? "enabled" : "disabled")}. Check server console for reports every 5 seconds.");
        }
    }
}