using System;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Logging;
using log4net;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class JunglespawnCommand : ChatCommandBase
    {
        private static ILog _logger = LoggerProvider.GetLogger();

        public override string Command => "junglespawn";
        public override string Syntax => $"{Command}";

        public JunglespawnCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            try
            {
                //Game.Map.MapScript.SpawnAllCamps();
            }
            catch (Exception e)
            {
                _logger.Error(null, e);
            }
            _logger.Info($"{ChatCommandManager.CommandStarterCharacter}{Command} Jungle Spawned!");
            ChatManager.System("Jungle Spawned!");
        }
    }
}
