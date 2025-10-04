using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Performance;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class AllocReportCommand : ChatCommandBase
    {
        public override string Command => "allocreport";
        public override string Syntax => "!allocreport";

        public AllocReportCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            AllocationTracker.ForceReport();
            ChatManager.System(userId, "Forced allocation report. Check server console.");
        }
    }
}