using ChildrenOfTheGraveEnumNetwork.Enums;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class TargettableCommand : ChatCommandBase
    {
        public override string Command => "targetable";
        public override string Syntax => $"{Command} false (untargetable) / true (targetable)";

        public TargettableCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');

            if (split.Length != 2 || !bool.TryParse(split[1], out var userInput))
            {
                ChatManager.SyntaxError();
                ShowSyntax();
            }
            else
            {
                Game.PlayerManager.GetPeerInfo(userId).Champion.SetStatus(StatusFlags.Targetable, userInput);
            }
        }
    }
}