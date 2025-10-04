using ChildrenOfTheGraveEnumNetwork;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class ChangeTeamCommand : ChatCommandBase
    {
        public override string Command => "changeteam";
        public override string Syntax => $"{Command} teamNumber";

        public ChangeTeamCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2)
            {
                ChatManager.SyntaxError();
                ShowSyntax();
                return;
            }

            if (!int.TryParse(split[1], out var t))
            {
                return;
            }

            var team = t.ToTeamId();
            Game.PlayerManager.GetPeerInfo(userId).Champion.SetTeam(team);
        }
    }
}
