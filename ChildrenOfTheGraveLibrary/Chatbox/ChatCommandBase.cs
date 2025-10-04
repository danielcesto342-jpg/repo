namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox
{
    public abstract class ChatCommandBase
    {
        protected readonly ChatCommandManager ChatCommandManager;

        public abstract string Command { get; }
        public abstract string Syntax { get; }
        public bool IsHidden { get; set; }
        public bool IsDisabled { get; set; }

        protected ChatCommandBase(ChatCommandManager chatCommandManager)
        {
            ChatCommandManager = chatCommandManager;
        }

        public abstract void Execute(int userId, bool hasReceivedArguments, string arguments = "");

        public void ShowSyntax()
        {
            var msg = $"{ChatCommandManager.CommandStarterCharacter}{Syntax}";
            ChatManager.Syntax(msg);
        }

        internal virtual void Update()
        {
        }
    }
}
