using static PacketVersioning.PktVersioning;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class SkillpointsCommand : ChatCommandBase
    {
        public override string Command => "skillpoints";
        public override string Syntax => $"{Command}";

        public SkillpointsCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var champion = Game.PlayerManager.GetPeerInfo(userId).Champion;
            champion.Experience.SpellTrainingPoints.AddTrainingPoints(17);

            NPC_UpgradeSpellAnsNotify(userId, champion.NetId, 0, 0, 17);
        }
    }
}
