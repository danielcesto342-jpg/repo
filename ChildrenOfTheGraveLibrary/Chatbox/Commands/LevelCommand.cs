namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class LevelCommand : ChatCommandBase
    {
        const int MAX_LEVEL = 18;
        public override string Command => "level";
        public override string Syntax => $"{Command} level";

        public LevelCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            var champ = Game.PlayerManager.GetPeerInfo(userId).Champion;

            if (split.Length < 2)
            {
                ChatManager.SyntaxError();
                ShowSyntax();
            }
            else if (byte.TryParse(split[1], out var lvl))
            {
                if (lvl <= champ.Experience.Level || lvl > MAX_LEVEL)
                {
                    ChatManager.Error($"The level must be higher than current and smaller or equal to {MAX_LEVEL}!");
                    return;
                }

                champ.Experience.AddEXP(champ.Experience.ExpNeededPerLevel[lvl - 2] - champ.Experience.Exp);
            }
        }
    }
}
