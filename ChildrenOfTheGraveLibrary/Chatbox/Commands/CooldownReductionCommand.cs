using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.StatsNS;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class CooldownReductionCommand : ChatCommandBase
    {
        public override string Command => "cdr";
        public override string Syntax => $"{Command} bonusCdr";

        public CooldownReductionCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
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
            else if (float.TryParse(split[1], out var cdr))
            {
                StatsModifier modifier = new();
                modifier.CooldownReduction.IncPercentBonusPerm(cdr / 100f);
                Game.PlayerManager.GetPeerInfo(userId).Champion.AddStatModifier(modifier);
            }
        }
    }
}
