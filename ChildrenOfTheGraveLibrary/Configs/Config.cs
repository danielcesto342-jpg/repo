using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Logging;
using ChildrenOfTheGraveEnumNetwork.Enums;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer
{
    /// <summary>
    /// Class that contains basic game information which is used to decide how the game will function after starting, such as players, their spawns,
    /// the packages which control the functionality of their champions/abilities, and lastly whether basic game mechanics such as 
    /// cooldowns/mana costs/minion spawns should be enabled/disabled.
    /// </summary>
    public class Config
    {
        internal const string VERSION_STRING = "Version 1.0.0.126 [PUBLIC]";

        private static ILog _logger = LoggerProvider.GetLogger();
        internal static readonly Version VERSION = new(1, 0, 0, 126);

        public List<PlayerConfig> Players { get; private set; }
        public string HttpPostAddress { get; private set; } = string.Empty;
        public bool SupressScriptNotFound { get; private set; }
        public bool ABClient { get; set; } = false;
        public bool EnableLogAndConsole { get; set; } = false;
        public bool EnableLogBehaviourTree { get; set; } = false;
        public bool EnableLogPKT { get; set; } = false;
        public bool EnableReplay { get; set; } = false;

        public bool EnableAllocationTracker { get; set; } = false;
        internal GameConfig GameConfig { get; private set; } = new(null);
        internal FeatureFlags GameFeatures { get; private set; }
        internal float ForcedStart { get; private set; }
        internal int TickRate { get; private set; }
        internal long GameId { get; private set; }
        internal bool ChatCheatsEnabled { get; private set; }
        internal bool IsDamageTextGlobal { get; private set; }
        internal string ContentPath { get; private set; } = string.Empty;
        internal string? DeployFolder { get; private set; }
        internal bool KeepAliveWhenEmpty { get; private set; }
        internal string? VersionOfClient { get; private set; }
        internal string? nicknameforreplay { get; private set; }
        internal string? pswdforreplay { get; private set; }

        //use kdrive for host replay 
        internal string? apikeydropbox { get; private set; }
        internal string[] AssemblyPaths { get; private set; }


        /// <summary>
        /// Loads config from json text or a json file.
        /// </summary>
        /// <param name="json">Config Text or path to Config File</param>
        /// <exception cref="ArgumentNullException">Invalid/Null/Empty Config provided</exception>
        public Config(string json)
        {
            Players = new List<PlayerConfig>();

            //Check Json File
            if (File.Exists(json))
            {
                var file = File.ReadAllText(json);
                if (string.IsNullOrEmpty(file))
                {
                    throw new ArgumentNullException(json, "The provided json file is empty, please provide a valid config.");
                }
                LoadConfig(file);
                return;
            }

            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException(json, "The provided json text is null or empty, please provide a valid config.");
            }

            // Load raw json text
            LoadConfig(json);
        }

        private void LoadConfig(string json)
        {
            _logger.Info("PARSING CONFIG");
            JObject data = JObject.Parse(json);
            GameId = data.Value<long>("gameId");

            // DATA
            GameConfig = new GameConfig(data?.Value<JToken?>("game"));

            JArray playerConfigurations = data?.Value<JArray>("players")!;
            foreach (var player in playerConfigurations)
            {
                Players.Add(new PlayerConfig(player));
            }

            // GAME-INFO
            JToken? gameInfo = data?.Value<JToken?>("gameInfo");
            TickRate = gameInfo?.Value<int?>("TICK_RATE") ?? 30;
            SetGameFeatures(FeatureFlags.EnableManaCosts, gameInfo?.Value<bool?>("MANACOSTS_ENABLED") ?? true);
            SetGameFeatures(FeatureFlags.EnableCooldowns, gameInfo?.Value<bool?>("COOLDOWNS_ENABLED") ?? true);
            SetGameFeatures(FeatureFlags.EnableLaneMinions, gameInfo?.Value<bool?>("MINION_SPAWNS_ENABLED") ?? true);
            ChatCheatsEnabled = gameInfo?.Value<bool?>("CHEATS_ENABLED") ?? false;
            //Time to the game get forced to start, even if not all players are connected
            ForcedStart = gameInfo?.Value<float?>("FORCE_START_TIMER") * 1000 ?? 60_000.0f;
            SupressScriptNotFound = gameInfo?.Value<bool?>("SUPRESS_SCRIPT_NOT_FOUND_LOGS") ?? false;
            ABClient = gameInfo?.Value<bool?>("AB_CLIENT") ?? false;
            EnableLogAndConsole = gameInfo?.Value<bool?>("ENABLE_LOG_AND_CONSOLEWRITELINE") ?? false;
            EnableLogBehaviourTree = gameInfo?.Value<bool?>("ENABLE_LOG_BehaviourTree") ?? false;
            EnableLogPKT = gameInfo?.Value<bool?>("ENABLE_LOG_PKT") ?? false;
            EnableReplay = gameInfo?.Value<bool?>("ENABLE_REPLAY") ?? false;
            EnableAllocationTracker = gameInfo?.Value<bool?>("ENABLE_ALLOCATION_TRACKER") ?? false;
            IsDamageTextGlobal = gameInfo?.Value<bool?>("IS_DAMAGE_TEXT_GLOBAL") ?? false;
            ContentPath = gameInfo?.Value<string?>("CONTENT_PATH") ?? string.Empty;
            DeployFolder = gameInfo?.Value<string?>("DEPLOY_FOLDER") ?? string.Empty;
            KeepAliveWhenEmpty = gameInfo?.Value<bool?>("KEEP_ALIVE_WHEN_EMPTY") ?? false;
            VersionOfClient = gameInfo?.Value<string?>("CLIENT_VERSION") ?? string.Empty;
            //launcherside
            nicknameforreplay = gameInfo?.Value<string?>("USERNAMEOFREPLAYMAN") ?? string.Empty;
            pswdforreplay = gameInfo?.Value<string?>("PASSWORDOFREPLAYMAN") ?? string.Empty;

            AssemblyPaths = gameInfo?.SelectToken("SCRIPT_ASSEMBLIES")?.Values<string>()?.ToArray() ?? [];
            //Address used to make an end-game API call to deliver all game stats. (Used for end-game screen on laucnher-clients mostly)
            HttpPostAddress = gameInfo?.Value<string?>("ENDGAME_HTTP_POST_ADDRESS") ?? string.Empty;

            //account used for kdrive 
            apikeydropbox = gameInfo?.Value<string?>("APIKEYDROPBOX") ?? string.Empty;

            //Evaluate if content path is correct, if not try to path traversal to find it
            if (!Directory.Exists(ContentPath))
            {
                ContentPath = GetContentPath() ?? string.Empty;
            }
            if (DeployFolder is null && ContentPath is not null)
            {
                DeployFolder = Path.Combine(ContentPath, "deploy");
            }
        }

        private static string? GetContentPath()
        {
            string? result = null;

            var executionDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = new DirectoryInfo(executionDirectory ?? Directory.GetCurrentDirectory());

            while (result == null)
            {
                if (path == null)
                {
                    break;
                }

                var directory = path.GetDirectories().Where(c => c.Name.Equals("Content")).ToArray();

                if (directory.Length == 1)
                {
                    result = directory[0].FullName;
                }
                else
                {
                    path = path.Parent;
                }
            }

            return result;
        }

        public void SetGameFeatures(FeatureFlags flag, bool enabled)
        {
            // Toggle the flag on.
            if (enabled)
            {
                GameFeatures |= flag;
            }
            // Toggle off.
            else
            {
                GameFeatures &= ~flag;
            }
        }
    }
}