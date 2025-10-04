using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits.AI;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class ChCommand : ChatCommandBase
    {
        public override string Command => "ch";
        public override string Syntax => $"{Command} championName";

        public ChCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.Split(' ');
            if (split.Length < 2)
            {
                ChatManager.SyntaxError();
                ShowSyntax();
                return;
            }
            var currentChampion = Game.PlayerManager.GetPeerInfo(userId).Champion;

            var c = new Champion(
                split[1],
                Game.PlayerManager.GetClientInfoByChampion(currentChampion),
                currentChampion.NetId,
                Game.PlayerManager.GetPeerInfo(userId).Champion.Team
            );
            c.SetPosition(
                Game.PlayerManager.GetPeerInfo(userId).Champion.Position
            );

            c.ChangeModel(split[1]); // trigger the "modelUpdate" proc
            c.SetTeam(Game.PlayerManager.GetPeerInfo(userId).Champion.Team);
            Game.ObjectManager.RemoveObject(Game.PlayerManager.GetPeerInfo(userId).Champion);
            Game.ObjectManager.AddObject(c);
            Game.PlayerManager.GetPeerInfo(userId).Champion = c;
        }
    }
}
