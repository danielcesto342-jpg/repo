namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class HelpCommand : ChatCommandBase
    {
        private const string COMMAND_PREFIX = "<font color=\"#E175FF\"><b>";
        private const string COMMAND_SUFFIX = "</b></font>, ";
        private readonly int MESSAGE_MAX_SIZE = 512;

        public override string Command => "help";
        public override string Syntax => $"{Command}";

        public HelpCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {

        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            if (!Game.Config.ChatCheatsEnabled)
            {
                var msg = "[ChildrenOfTheGrave] Chat commands are disabled in this game.";
                ChatManager.System(msg);
                return;
            }

            var commands = ChatCommandManager.GetCommandsStrings();
            var commandsString = "";
            var lastCommandString = "";
            var isNewMessage = false;

            ChatManager.System("List of available commands: ");

            foreach (var command in commands)
            {
                if (isNewMessage)
                {
                    commandsString = new string(lastCommandString);
                    isNewMessage = false;
                }

                lastCommandString = $"{COMMAND_PREFIX}" +
                $"{ChatCommandManager.CommandStarterCharacter}{command}" +
                $"{COMMAND_SUFFIX}";

                if (commandsString.Length + lastCommandString.Length >= MESSAGE_MAX_SIZE)
                {
                    ChatManager.System(commandsString);
                    commandsString = "";
                    isNewMessage = true;
                }
                else
                {
                    commandsString = $"{commandsString}{lastCommandString}";
                }
            }

            if (commandsString.Length != 0)
            {
                ChatManager.System(commandsString);
            }

            ChatManager.System("There are " + commands.Count + " commands");
        }
    }
}
