namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class ModelCommand : ChatCommandBase
    {
        public override string Command => "model";
        public override string Syntax => $"{Command} modelName";

        public ModelCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.Split(' ');
            if (split.Length >= 2)
            {
                Game.PlayerManager.GetPeerInfo(userId).Champion.ChangeModel(split[1]);
            }
            else
            {
                ChatManager.SyntaxError();
                ShowSyntax();
            }
        }
    }
}
