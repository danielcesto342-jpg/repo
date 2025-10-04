using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class SuicideCommand : ChatCommandBase
    {
        public override string Command => "suicide";
        public override string Syntax => $"{Command}";

        public SuicideCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {

            if (!Game.Config.ChatCheatsEnabled)
            {


                var damage = new DamageData()
                {
                    Attacker = Game.PlayerManager.GetPeerInfo(userId).Champion,
                    Damage = 25000.0f,
                    DamageResultType = ChildrenOfTheGraveEnumNetwork.Enums.DamageResultType.RESULT_NORMAL,
                    DamageSource = ChildrenOfTheGraveEnumNetwork.Enums.DamageSource.DAMAGE_SOURCE_SPELL,
                    DamageType = ChildrenOfTheGraveEnumNetwork.Enums.DamageType.DAMAGE_TYPE_TRUE,
                    Target = Game.PlayerManager.GetPeerInfo(userId).Champion,
                    IgnoreDamageCrit = true,
                    IgnoreDamageIncreaseMods = true,
                };

                Game.PlayerManager.GetPeerInfo(userId).Champion.TakeDamage(damage);

            }
        }
    }
}
