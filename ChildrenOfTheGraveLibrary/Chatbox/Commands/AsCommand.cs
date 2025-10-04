namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class AsCommand : ChatCommandBase
    {
        public override string Command => "as";
        public override string Syntax => $"{Command} bonusAs (percent)";

        public AsCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
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
            else if (float.TryParse(split[1], out var attackSpeed))
            {
                Game.PlayerManager.GetPeerInfo(userId).Champion.Stats.AttackSpeedMultiplier
                    .IncPercentBasePerm(attackSpeed);
            }
        }
    }
}
