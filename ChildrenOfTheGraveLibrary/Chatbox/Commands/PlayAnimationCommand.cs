
namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class PlayAnimationCommand : ChatCommandBase
    {
        public override string Command => "playanim";
        public override string Syntax => $"{Command} animationName";

        public PlayAnimationCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
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
                Game.PlayerManager.GetPeerInfo(userId).Champion.PlayAnimation(split[1]);
            }
        }
    }
}
