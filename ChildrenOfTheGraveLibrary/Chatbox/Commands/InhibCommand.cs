using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits.AI;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class InhibCommand : ChatCommandBase
    {
        public override string Command => "inhib";
        public override string Syntax => $"{Command}";

        public InhibCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var sender = Game.PlayerManager.GetPeerInfo(userId);
            var min = new Minion(
                null,
                sender.Champion.Position,
                "Worm",
                "Worm",
                AIScript: "BasicJungleMonsterAI"
                );
            Game.ObjectManager.AddObject(min);
        }
    }
}
