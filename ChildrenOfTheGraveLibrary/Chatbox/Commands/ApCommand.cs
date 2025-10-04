namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class ApCommand : ChatCommandBase
    {
        public override string Command => "ap";
        public override string Syntax => $"{Command} bonusAp";

        public ApCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
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
            else if (float.TryParse(split[1], out var ap))
            {
                Game.PlayerManager.GetPeerInfo(userId).Champion.Stats.AbilityPower.IncFlatBonusPerm(ap);
            }
        }
    }
}
