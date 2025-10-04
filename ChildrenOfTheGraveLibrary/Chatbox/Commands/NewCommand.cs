namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class NewCommand : ChatCommandBase
    {
        public override string Command => "newcommand";
        public override string Syntax => $"{Command}";

        public NewCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var msg = $"The new command added by {ChatCommandManager.CommandStarterCharacter}help has been executed";
            ChatManager.Info(msg);
            ChatCommandManager.RemoveCommand(Command);
        }
    }
}
