using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.StatsNS;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class SpeedCommand : ChatCommandBase
    {
        public override string Command => "speed";
        public override string Syntax => $"{Command} [flat speed] [percent speed]";

        public SpeedCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2 || split.Length > 3)
            {
                ChatManager.SyntaxError();
                ShowSyntax();
            }

            try
            {
                StatsModifier stat = new();

                if (split.Length == 3)
                {
                    stat.MoveSpeed.PercentBonus = float.Parse(split[2]) / 100;
                }
                stat.MoveSpeed.FlatBonus = float.Parse(split[1]);

                Game.PlayerManager.GetPeerInfo(userId).Champion.AddStatModifier(stat);
            }
            catch
            {
                ChatManager.SyntaxError();
                ShowSyntax();
            }
        }
    }
}
