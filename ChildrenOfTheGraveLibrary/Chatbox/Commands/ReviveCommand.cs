namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class ReviveCommand : ChatCommandBase
    {
        public override string Command => "revive";
        public override string Syntax => $"{Command}";

        public ReviveCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var champ = Game.PlayerManager.GetPeerInfo(userId).Champion;
            if (!champ.Stats.IsDead)
            {
                ChatManager.Info("Your champion is already alive.");
                return;
            }

            ChatManager.Info("Your champion has revived!");
            champ.Respawn();
        }
    }
}
