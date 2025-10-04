namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class AdCommand : ChatCommandBase
    {
        public override string Command => "ad";
        public override string Syntax => $"{Command} bonusAd";

        public AdCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
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
            else if (float.TryParse(split[1], out var ad))
            {
                Game.PlayerManager.GetPeerInfo(userId).Champion.Stats.AttackDamage.FlatBonus += ad;
            }
        }
    }
}
