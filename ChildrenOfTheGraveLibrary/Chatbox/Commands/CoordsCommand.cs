using System;
using ChildrenOfTheGraveEnumNetwork;
using System.Numerics;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Logging;
using log4net;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class CoordsCommand : ChatCommandBase
    {
        private static ILog _logger = LoggerProvider.GetLogger();

        public override string Command => "coords";
        public override string Syntax => $"{Command}";

        public CoordsCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var champion = Game.PlayerManager.GetPeerInfo(userId).Champion;
            _logger.Debug($"At {champion.Position.X}; {champion.Position.Y}");
            var dirMsg = "Not moving anywhere";
            if (!champion.IsPathEnded())
            {
                Vector2 dir = champion.Direction.ToVector2();
                // Angle measured between [1, 0] and player direction vectors (to X axis)
                double ang = Math.Acos(dir.X / dir.Length()) * (180 / Math.PI);
                dirMsg = $"dirX: {dir.X} dirY: {dir.Y} dirAngle (to X axis): {ang}";
            }
            var coords3D = champion.GetPosition3D();
            var msg = $"At Coords - X: {coords3D.X} Y: {coords3D.Z} Height: {coords3D.Y} " + dirMsg;
            ChatManager.System(msg);
        }
    }
}
