namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class ClearSlowCommand : ChatCommandBase
    {
        public override string Command => "clearslows";
        public override string Syntax => $"{Command}";

        public ClearSlowCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            Game.PlayerManager.GetPeerInfo(userId).Champion.Stats.MoveSpeed.ClearSlows();
            ChatManager.Info("Your slows have been cleared!");
        }
    }
}
