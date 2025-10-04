namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class MrCommand : ChatCommandBase
    {
        public override string Command => "mr";
        public override string Syntax => $"{Command} bonusMr";

        public MrCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
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
            else if (float.TryParse(split[1], out var mr))
            {
                Game.PlayerManager.GetPeerInfo(userId).Champion.Stats.MagicResist.IncFlatBonusPerm(mr);
            }
        }
    }
}
