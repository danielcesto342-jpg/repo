namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class XpCommand : ChatCommandBase
    {

        public override string Command => "xp";
        public override string Syntax => $"{Command} xp";

        public XpCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
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

            if (float.TryParse(split[1], out float xp))
            {
                if (xp <= 0)
                {
                    return;
                }

                Game.PlayerManager.GetPeerInfo(userId).Champion.Experience.AddEXP(xp);
            }
        }
    }
}
