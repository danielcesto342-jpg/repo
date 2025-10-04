using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.StatsNS;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class HealthCommand : ChatCommandBase
    {
        public override string Command => "health";
        public override string Syntax => $"{Command} maxHealth";

        public HealthCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
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
            else if (float.TryParse(split[1], out var hp))
            {
                StatsModifier modifier = new();
                modifier.HealthPoints.FlatBonus = hp;
                Game.PlayerManager.GetPeerInfo(userId).Champion.AddStatModifier(modifier);
            }
        }
    }
}
