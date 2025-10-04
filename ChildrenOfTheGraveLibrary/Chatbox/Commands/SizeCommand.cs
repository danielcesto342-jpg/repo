namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class SizeCommand : ChatCommandBase
    {
        public override string Command => "size";
        public override string Syntax => $"{Command} size";

        public SizeCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
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
            else if (float.TryParse(split[1], out float size))
            {
                Game.PlayerManager.GetPeerInfo(userId).Champion.Stats.Size.IncPercentBonusPerm(size);
            }
        }
    }
}
