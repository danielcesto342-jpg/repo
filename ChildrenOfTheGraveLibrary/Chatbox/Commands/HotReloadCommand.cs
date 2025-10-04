namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class HotReloadCommand : ChatCommandBase
    {
        public override string Command => "hotreload";
        public override string Syntax => $"{Command} 0 (disable) / 1 (enable)";

        public HotReloadCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');

            if (split.Length < 2 || !byte.TryParse(split[1], out byte input) || input > 1)
            {
                ChatManager.SyntaxError();
                ShowSyntax();
            }
            else
            {
                if (input == 1)
                {
                    //   Game.EnableHotReload(true);
                    ChatManager.Info("Scripts hot reload enabled.");
                }
                else
                {
                    //   Game.EnableHotReload(false);
                    ChatManager.Info("Scripts hot reload disabled.");
                }

            }
        }
    }
}
