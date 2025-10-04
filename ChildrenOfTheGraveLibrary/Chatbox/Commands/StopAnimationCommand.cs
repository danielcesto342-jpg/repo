
namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class StopAnimationCommand : ChatCommandBase
    {
        public override string Command => "stopanim";
        public override string Syntax => $"{Command} animationName";

        public StopAnimationCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2)
            {
                ChatManager.SyntaxError();
                ShowSyntax();
            }
            else
            {
                Game.PlayerManager.GetPeerInfo(userId).Champion.StopAnimation(split[1]);
            }
        }
    }
}
