using System.Numerics;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class TpCommand : ChatCommandBase
    {
        public override string Command => "tp";
        public override string Syntax => $"{Command} [target NetID (0 or none for self)] X Y";

        public TpCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');

            if (split.Length < 3 || split.Length > 4)
            {
                ChatManager.SyntaxError();
                ShowSyntax();
                return;
            }

            if (split.Length > 3 && uint.TryParse(split[1], out uint targetNetID) && float.TryParse(split[2], out float x) && float.TryParse(split[3], out float y))
            {
                var obj = Game.ObjectManager.GetObjectById(targetNetID);
                if (obj is AttackableUnit unit)
                {
                    unit.TeleportTo(new Vector2(x, y));
                }
                else
                {
                    ChatManager.SyntaxError("An object with the netID: " + targetNetID + " was not found.");
                    ShowSyntax();
                }
            }
            else if (float.TryParse(split[1], out x) && float.TryParse(split[2], out y))
            {
                Game.PlayerManager.GetPeerInfo(userId).Champion.TeleportTo(new Vector2(x, y), true);
            }
        }
    }
}