namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class SpawnStateCommand : ChatCommandBase
    {
        public override string Command => "spawnstate";
        public override string Syntax => $"{Command} 0 (disable) / 1 (enable)";

        public SpawnStateCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {

        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');

            if (split.Length < 2 || !byte.TryParse(split[1], out var input) || input > 1)
            {
                ChatManager.SyntaxError();
                ShowSyntax();
            }
            else
            {
                Game.Map.GameMode.MapScriptMetadata.MinionSpawnEnabled = input != 0;
                Game.Config.SetGameFeatures(ChildrenOfTheGraveEnumNetwork.Enums.FeatureFlags.EnableLaneMinions, input != 0);
            }
        }
    }
}
