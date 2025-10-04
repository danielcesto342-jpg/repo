namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class ArmorCommand : ChatCommandBase
    {
        public override string Command => "armor";
        public override string Syntax => $"{Command} bonusArmor";

        public ArmorCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
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
            else if (float.TryParse(split[1], out var armor))
            {
                Game.PlayerManager.GetPeerInfo(userId).Champion.Stats.Armor.IncFlatBonusPerm(armor);
            }
        }
    }
}
