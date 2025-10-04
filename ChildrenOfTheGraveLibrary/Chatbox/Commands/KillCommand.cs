using ChildrenOfTheGraveEnumNetwork.Enums;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits.AI;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class KillCommand : ChatCommandBase
    {
        public override string Command => "kill";
        public override string Syntax => $"{Command} minions";

        public KillCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');

            if (split.Length < 2)
            {
                ChatManager.SyntaxError();
                ShowSyntax();
            }
            else if (split[1] == "minions")
            {
                var objects = Game.ObjectManager.GetObjects();
                foreach (var o in objects)
                {
                    if (o.Value is Minion minion)
                    {
                        minion.Die(new DeathData
                        {
                            BecomeZombie = false,
                            DieType = 0,
                            Unit = minion,
                            Killer = minion,
                            DamageType = (byte)DamageType.DAMAGE_TYPE_PHYSICAL,
                            DamageSource = (byte)DamageSource.DAMAGE_SOURCE_RAW,
                            DeathDuration = 0
                        }); // :(
                    }
                }
            }
            else
            {
                ChatManager.SyntaxError();
                ShowSyntax();
            }
        }
    }
}
