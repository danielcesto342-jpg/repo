namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class GoldCommand : ChatCommandBase
    {
        public override string Command => "gold";
        public override string Syntax => $"{Command} goldAmount";

        public GoldCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
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
            else if (float.TryParse(split[1], out var gold))
            {
                var ch = Game.PlayerManager.GetPeerInfo(userId).Champion;
                ch.GoldOwner.AddGold(gold);
            }
        }
    }
}
