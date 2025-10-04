namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class ManaCommand : ChatCommandBase
    {
        public override string Command => "mana";
        public override string Syntax => $"{Command} maxMana";

        public ManaCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
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
            else if (float.TryParse(split[1], out var mp))
            {
                Game.PlayerManager.GetPeerInfo(userId).Champion.Stats.ManaPoints.FlatBonus += mp;
                Game.PlayerManager.GetPeerInfo(userId).Champion.Stats.CurrentMana += mp;
            }
        }
    }
}
