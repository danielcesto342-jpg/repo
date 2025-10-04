#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Content.FileSystem;
using ChildrenOfTheGraveEnumNetwork.Enums;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Content.Navigation;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Content.Navigation.aimesh;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits.AI;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Handlers;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Inventory;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Logging;
using log4net;
using MoonSharp.Interpreter;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Content;

/// <summary>
/// Class that handles loading and handling information from the server-side parsed lol_game_client
/// </summary>
internal class ContentManager
{
    private static ILog _logger = LoggerProvider.GetLogger();

    /// <summary>
    /// Path to the server side directory containing the parsed lol_game_client and script packages. 
    /// </summary>
    internal static string ContentPath { get; set; }

    /// <summary>
    /// Path to the DATA directory within the parsed lol_game_client
    /// </summary>
    internal static string DataPath { get; private set; }

    /// <summary>
    /// Path to the DATA Location.dat
    /// </summary>
    internal static string LocationPath { get; private set; }

    /// <summary>
    /// Path to the primary script package directory that contains server side game logic
    /// </summary>
    internal static string ScriptsPath { get; private set; }

    /// <summary>
    /// Path to the secondary script package used if thee primary ones are not present.
    /// Hardcoded to AvLua-Converted at the moment.
    /// </summary>
    internal static string ScriptsPathSecondary { get; private set; }

    /// <summary>
    /// Path to the Talents directory within the DATA directory.
    /// </summary>
    internal static string TalentsPath { get; private set; }

    /// <summary>
    /// Path to the Spells directory within the DATA directory.
    /// </summary>
    internal static string SpellsPath { get; private set; }

    /// <summary>
    /// Path to the Characters directory within the DATA directory.
    /// </summary>
    internal static string CharactersPath { get; private set; }

    /// <summary>
    /// Path to the Items directory within the DATA directory.
    /// </summary>
    internal static string ItemsPath { get; private set; }

    /// <summary>
    /// Path to the directory of the map being used by the sever instance within the
    /// parsed lol_game_client/LEVELS directory.
    /// </summary>
    internal static string MapPath { get; private set; }

    /// <summary>
    /// Path to the scene directory within the map directory.
    /// </summary>
    internal static string ScenePath { get; private set; }

    /// <summary>
    /// Path to the Particles directory within the DATA directory.
    /// </summary>
    internal static string ParticlesPath { get; private set; }

    /// <summary>
    /// Path to the Encounter directory .
    /// </summary>
    internal static string EncounterPath { get; private set; }

    /// <summary>
    /// Path to the Encounter directory .
    /// </summary>
    internal static string MutatorPath { get; private set; }

    /// <summary>
    /// Map .ini contents that contains configurations for the map in use.
    /// </summary>
    internal static RFile MapConfig { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Model"></param>
    /// <param name="SkinId"></param>
    /// <param name="LifeTime"></param>
    public record class ParticleData(string Model, int SkinId, float LifeTime, int bindtoemitter);

    /// <summary>
    /// Offset for the initial spawn of champion in a match.
    /// </summary>
    internal static Dictionary<TeamId, List<List<SpawnOffsetInfo>>> HeroSpawnOffset { get; } = new()
    {
        [TeamId.TEAM_ORDER] = [],
        [TeamId.TEAM_CHAOS] = []
    };

    /// <summary>
    /// Game feature flags that are currently enabled for the match. 
    /// </summary>
    internal static GameFeatures GameFeatures { get; private set; } = 0 | GameFeatures.FoundryOptions
                                                                        | GameFeatures.EarlyWarningForFOWMissiles
                                                                        | GameFeatures.ItemUndo
                                                                        | GameFeatures.TurretRangeIndicators
                                                                        | GameFeatures.NewMinionSpawnOrder
                                                                        | GameFeatures.AlternateBountySystem
                                                                        | GameFeatures.ParticleSkinNameTech
                                                                        | GameFeatures.JungleTimers
                                                                        | GameFeatures.TimerSyncForReplay
                                                                        | GameFeatures.NonRefCountedCharacterStates
                                                                        | GameFeatures.ActiveItemUI;

    // Variables used only within ContentManager

    /// <summary>
    /// Character data that the server has currently loaded.
    /// </summary>
    private static Dictionary<string, CharData> CharactersData { get; } = new();

    /// <summary>
    /// Spell data that server has currently loaded.
    /// </summary>
    private static Dictionary<string, SpellData> SpellsData { get; } = new();

    /// <summary>
    /// Item data that server has currently loaded.
    /// </summary>
    private static Dictionary<int, ItemData> ItemsData { get; } = new();

    /// <summary>
    /// Talents data that server has currently loaded.
    /// </summary>
    private static Dictionary<string, TalentData> TalentsData { get; } = new();

    /// <summary>
    /// Particle data that server has currently loaded.
    /// </summary>
    private static Dictionary<string, List<ParticleData>> ParticlesData { get; } = new();

    /// <summary>
    /// Particle data that server has currently loaded.
    /// </summary>
    private static Dictionary<string, MapObject> LocationData { get; } = new();

    /// <summary>
    /// encounter that server has currently loaded.
    /// </summary>
    private static Dictionary<string, EncounterDefinition> EncounterData { get; } = new();

    /// <summary>
    /// Mutator that server has currently loaded.
    /// </summary>
    private static Dictionary<string, MutatorDefinition> MutatorData { get; } = new();

    // ============================================================================
    // DICTIONNAIRES POUR LES FICHIERS .DAT DE CONFIGURATION AI
    // ============================================================================

    /// <summary>
    /// AI Manager configuration data loaded from AiManagerDataFile.dat
    /// </summary>
    public static Dictionary<string, string> AiManagerData { get; } = new();

    /// <summary>
    /// AI Mission configuration data loaded from AiMissionDataFile.dat
    /// </summary>
    public static Dictionary<string, string> AiMissionData { get; } = new();

    /// <summary>
    /// AI Task configuration data loaded from AiTaskDataFile.dat
    /// </summary>
    public static Dictionary<string, string> AiTaskData { get; } = new();

    /// <summary>
    /// AI Entity configuration data loaded from AiEntityDataFile.dat
    /// </summary>
    public static Dictionary<string, string> AiEntityData { get; } = new();

    /// <summary>
    /// AI Fuzzy configuration data loaded from AiFuzzyDataFile.dat
    /// </summary>
    public static Dictionary<string, string> AiFuzzyData { get; } = new();

    /// <summary>
    /// Level Script configuration data loaded from LevelScriptDataFile.dat
    /// </summary>
    public static Dictionary<string, string> LevelScriptData { get; } = new();

    /// <summary>
    /// Team Order AI Mission data loaded from TeamOrderAiMission.dat
    /// </summary>
    public static Dictionary<string, string> TeamOrderAiMissionData { get; } = new();

    /// <summary>
    /// Team Chaos AI Mission data loaded from TeamChaosAiMission.dat
    /// </summary>
    public static Dictionary<string, string> TeamChaosAiMissionData { get; } = new();

    /// <summary>
    /// Team Neutral AI Mission data loaded from TeamNeutralAiMission.dat
    /// </summary>
    public static Dictionary<string, string> TeamNeutralAiMissionData { get; } = new();

    /// <summary>
    /// Team Order AI Task data loaded from TeamOrderAiTask.dat
    /// </summary>
    public static Dictionary<string, string> TeamOrderAiTaskData { get; } = new();

    /// <summary>
    /// Team Chaos AI Task data loaded from TeamChaosAiTask.dat
    /// </summary>
    public static Dictionary<string, string> TeamChaosAiTaskData { get; } = new();

    /// <summary>
    /// Team Neutral AI Task data loaded from TeamNeutralAiTask.dat
    /// </summary>
    public static Dictionary<string, string> TeamNeutralAiTaskData { get; } = new();

    /// <summary>
    /// Level Script Data File Server data loaded from LevelScriptDataFileServer.dat
    /// </summary>
    public static Dictionary<string, string> LevelScriptDataFileServerData { get; } = new();

    /// <summary>
    /// Level Script Data File Client data loaded from LevelScriptDataFileClient.dat
    /// </summary>
    public static Dictionary<string, string> LevelScriptDataFileClientData { get; } = new();


    public Table DefaultNeutralMinionValues { get; set; } = default;

    static ContentManager()
    {
        ContentPath = string.Empty;
        ScriptsPath = string.Empty;


        ScriptsPathSecondary = Game.Config.VersionOfClient switch
        {
            "1.0.0.106" or "0.9.22.14" => "AvLua-Converted106",
            _ => "AvLua-Converted"
        };


        DataPath = Game.Config.VersionOfClient switch
        {
            "1.0.0.106" or "0.9.22.14" => "GameClient106",
            _ => "GameClient"
        };


        TalentsPath = "DATA/Talents";
        SpellsPath = "DATA/Spells";
        CharactersPath = "DATA/Characters";
        ItemsPath = "DATA/Items";
        ParticlesPath = "DATA/Particles";
        MapPath = "LEVELS/Map";
        ScenePath = "Scene";
        EncounterPath = "Encounters";
        MutatorPath = "Mutators";
        MapConfig = default!;
    }

    /// <summary>
    /// Initializes the ContentManger for the server and loads in preliminary content before the match begins.
    /// </summary>
    /// <param name="config">Config that contains server setup information</param>
    internal ContentManager(Config config)
    {
        var contentPath = Path.Join(config.ContentPath);
        ContentPath = contentPath;
        ScriptsPath = Path.Join(contentPath, config.GameConfig.DataPackage);
        ScriptsPathSecondary = Path.Join(contentPath, ScriptsPathSecondary);
        DataPath = Path.Join(contentPath, DataPath);
        TalentsPath = Path.Join(DataPath, TalentsPath);
        SpellsPath = Path.Join(DataPath, SpellsPath);
        CharactersPath = Path.Join(DataPath, CharactersPath);
        ItemsPath = Path.Join(DataPath, ItemsPath);
        ParticlesPath = Path.Join(DataPath, ParticlesPath);
        MapPath = Path.Join(DataPath, MapPath + config.GameConfig.Map);
        ScenePath = Path.Join(MapPath, ScenePath);
        EncounterPath = Path.Join(MapPath, EncounterPath);
        MutatorPath = Path.Join(EncounterPath, MutatorPath);
        MapConfig = Cache.GetFile($"{ScenePath}/CFG/ObjectCFG.cfg");

        LocationPath = MapPath;

        foreach (var contentDirectory in new[]
                 {
                     ContentPath,
                     DataPath,
                     MapPath,
                     ScriptsPath,
                     ScriptsPathSecondary,
                     TalentsPath,
                     ItemsPath,
                     SpellsPath,
                     CharactersPath,
                     ParticlesPath,
                     ScenePath
                 })
        {
            if (!Directory.Exists(contentDirectory))
            {
                _logger.Warn($"This Content directory doesnt exist: {contentDirectory}");
            }
        }

        LoadContent();
    }

    //Hack
    KeyValuePair<Vector3, float> GetHeroSpawnLocation(TeamId team, byte spawnPos, int size /*hack*/)
    {
        if (spawnPos < 6)
        {
            string section = "";
            if (team is TeamId.TEAM_ORDER)
            {
                section = $"Order{size}";
            }
            else if (team == TeamId.TEAM_CHAOS)
            {
                section = $"Chaos{size}";
            }

            Cache.GetValue(out Vector3 offset, CharactersPath + "/HeroSpawnOffsets.ini", section, $"Pos{spawnPos}");
            Cache.GetValue(out float facing, CharactersPath + "/HeroSpawnOffsets.ini", section, $"Facing{spawnPos}");
            return new(offset, facing);
        }

        return new(Vector3.Zero, 0);
    }

    /// <summary>
    /// Begins loading content data from the server-side parsed lol_game_client that will be used by the match.
    /// TODO: It pulls all possible data from the game-client till selective loading is setup.
    /// </summary>
    private void LoadContent()
    {
        _logger.Info("Loading Spawn Offsets...");
        RFile spawnOffset = Cache.GetFile($"{CharactersPath}/HeroSpawnOffsets.ini");

        //Hack
        for (int teamPlayerCount = 1; teamPlayerCount <= 5; teamPlayerCount++)
        {
            HeroSpawnOffset[TeamId.TEAM_ORDER].Add([]);
            HeroSpawnOffset[TeamId.TEAM_CHAOS].Add([]);
            //int teamPlayerCount = Game.PlayerManager.GetTeamSize(TeamId.TEAM_ORDER);
            for (byte i = 1; i <= teamPlayerCount; i++)
            {
                KeyValuePair<Vector3, float> offsets = GetHeroSpawnLocation(TeamId.TEAM_ORDER, i, teamPlayerCount);
                HeroSpawnOffset[TeamId.TEAM_ORDER][teamPlayerCount - 1].Add(new()
                {
                    PositionOffset = offsets.Key,
                    FacingDirection = offsets.Value
                });

                offsets = GetHeroSpawnLocation(TeamId.TEAM_ORDER, i, teamPlayerCount);
                HeroSpawnOffset[TeamId.TEAM_CHAOS][teamPlayerCount - 1].Add(new()
                {
                    PositionOffset = offsets.Key,
                    FacingDirection = offsets.Value
                });
            }
        }

        _logger.Info("Loading Character Data...");
        _logger.Info(CharactersPath);
        Parallel.ForEach(Directory.EnumerateDirectories(CharactersPath), LoadCharacter);

        _logger.Info("Loading Spell Data...");
        //First loads spells in Data/Spells and Data/Shared/Spells
        // Pseudocode:
        // - Use Directory.EnumerateFiles with SearchOption.AllDirectories for SpellsPath
        // - Filter files to those ending with ".ini" or ".inibin" (case-insensitive)
        // - Use Parallel.ForEach to process each file

        Parallel.ForEach(Directory.EnumerateFiles(SpellsPath, "*", SearchOption.AllDirectories),
            file =>
            {
                if (file.EndsWith(".ini", StringComparison.OrdinalIgnoreCase) ||
                    file.EndsWith(".inibin", StringComparison.OrdinalIgnoreCase))
                {
                    ProcessSpellFile(file.Replace(".inibin", ".ini"));
                }
            });


        _logger.Info("Loading Item Data...");
        Parallel.ForEach(Directory.EnumerateFiles(ItemsPath, "*"), (file) =>
        {
            if (file.EndsWith(".ini", StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".inibin", StringComparison.OrdinalIgnoreCase))
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (int.TryParse(fileName, out int parsedId))
                {
                    ItemData data = new(parsedId, file.Replace(".inibin", ".ini"));
                    lock (ItemsData)
                    {
                        ItemsData.TryAdd(data.Id, data);
                    }
                }
            }
        });

        _logger.Info("Loading Talent Data...");
        Parallel.ForEach(Directory.EnumerateFiles(TalentsPath, "*"), (file) =>
        {
            if (file.EndsWith(".ini", StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".inibin", StringComparison.OrdinalIgnoreCase))
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (int.TryParse(fileName, out int parsedId))
                {
                    TalentData data = new(fileName, file.Replace(".inibin", ".ini"));
                    lock (TalentsData)
                    {
                        TalentsData.TryAdd(data.Id, data);
                    }
                }
            }
        });

        // DATA/Particles
        // DATA/Shared/Particles
        // DATA/Characters/{Character}
        // DATA/Characters/{Character}/skins/{Skin}
        // DATA/Characters/{Character}/Skins/{Skin}
        // DATA/Characters/{Character}/Skins/{Skin}/Particles

        _logger.Info("Loading Particles...");
        Parallel.ForEach(Directory.EnumerateFiles(ParticlesPath, "*"), (file) =>
        {
            if (file.EndsWith(".troy", StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".troybin", StringComparison.OrdinalIgnoreCase))
            {
                ProcessParticleFile(file.Replace(".troybin", ".troy"), "", -1);
            }
        });

        // ============================================================================
        // CHARGEMENT DES FICHIERS .DAT DE CONFIGURATION AI
        // ============================================================================
        _logger.Info("Loading AI Configuration Data Files...");
        LoadAiConfigurationData();

    }

    //TODO: Refactor or add another method that only loads assets for a specific character skin
    /// <summary>
    /// Iterates over the parsed server-side lol_game_client Characters directory loading related assets:
    /// - Character ini file
    /// - Spell ini files from from the Spells directory if its present
    /// - Particle troy files from ALL skin directories in the Skins directory if they are present.
    /// </summary>
    private void LoadCharacter(string characterPath)
    {
        // Loads Character.ini -> Spells folder -> Skins\Particles folder
        // Load Character ini
        var dirName = Path.GetFileName(characterPath);

        // Rechercher les fichiers dans characterPath, et uniquement ceux qui se terminent par ".ini", sans tenir compte de la casse
        Parallel.ForEach(Directory.EnumerateFiles(characterPath), (file) =>
        {
            string fileName = Path.GetFileNameWithoutExtension(file);

            // Comparer les noms de fichiers sans tenir compte de la casse et s'assurer que l'extension est .ini
            if (
                string.Equals(fileName, dirName, StringComparison.OrdinalIgnoreCase) && (
                    file.EndsWith(".ini", StringComparison.OrdinalIgnoreCase) ||
                    file.EndsWith(".inibin", StringComparison.OrdinalIgnoreCase
                ))
            ){
                var data = new CharData(dirName, file.Replace(".inibin", ".ini"));
                lock (CharactersData)
                {
                    fileName = fileName.ToLowerInvariant();
                    CharactersData.TryAdd(fileName, data);
                }
            }
        });

        // Load Character spells
        var spellsPath = $"{characterPath}/Spells";
        if (Directory.Exists(spellsPath))
        {
            Parallel.ForEach(Directory.EnumerateFiles(spellsPath, "*.ini", SearchOption.AllDirectories), x =>
            {
                ProcessSpellFile(Path.GetFileNameWithoutExtension(x));
            });
        }

        // Loading all skins until selective loading is setup
        // Load Character particles
        var skinsPath = "";
        if (Directory.Exists($"{characterPath}/Skins"))
        {
            skinsPath = $"{characterPath}/Skins";
        }
        else if (Directory.Exists($"{characterPath}/skins"))
        {
            skinsPath = $"{characterPath}/skins";
        }

        if (string.IsNullOrEmpty(skinsPath))
        {
            return;
        }

        Parallel.ForEach(Directory.EnumerateDirectories(skinsPath), (skinDir) =>
        {
            string skinName = Path.GetFileName(skinDir).ToLowerInvariant();
            int skinId;

            if (skinName is "base")
            {
                skinId = 0;
            }
            else if (Regex.IsMatch(skinName, @"^skin(\d+)$"))
            {
                // 4 for skipping the initial "skin" in the name
                skinId = int.Parse(skinName[4..]);
            }
            else
            {
                return;
            }

            foreach (var file in Directory.EnumerateFiles(skinDir, "*.troy"))
            {
                ProcessParticleFile(file, dirName, skinId);
            }

            string partDirPath = Path.Join(skinDir, "Particles");
            if (!Directory.Exists(partDirPath))
            {
                return;
            }

            foreach (string file in Directory.EnumerateFiles(partDirPath, "*.troy"))
            {
                ProcessParticleFile(file, dirName, skinId);
            }
        });
    }


    // Process file(s)

    /// <summary>
    /// Parses the Spell .ini file for spell info,
    /// </summary>
    /// <param name="file"></param>
    private static void ProcessSpellFile(string filePath)
    {
        string name = Path.GetFileNameWithoutExtension(filePath);

        SpellData data = new(name, filePath);
        //SpellFlagsMarker.SwitchFlagsIfNeeded(data); // 126Fix

        name = name.ToLowerInvariant();
        if (!SpellsData.ContainsKey(name))
        {
            lock (SpellsData)
            {
                SpellsData[name] = data;
            }
        }
    }

    /// <summary>
    /// Parses the Particle .troy file for particle info.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="model"></param>
    /// <param name="skin"></param>
    private static void ProcessParticleFile(string file, string model, int skin)
    {
        RFile? contentFile = Cache.GetFile(file);
        if (contentFile is null)
        {
            return;
        }

        float maxEffectLifetime = 0;

        for (int i = 1; ; i++)
        {
            string? sectionName = contentFile.GetValue("System", $"GroupPart{i}", null!);

            if (sectionName is null)
            {
                break;
            }

            // "e-life" - emitter, "p-life" - particle, "f-life" - fluid
            float eLife = contentFile.GetValue(sectionName, "e-life", -1.0f);
            float pLife = contentFile.GetValue(sectionName, "p-life", -1.0f);
            float fLife = contentFile.GetValue(sectionName, "f-life", -1.0f);
            string bindValueRaw = contentFile.GetValue(sectionName, "p-bindtoemitter", null);

            if (eLife is -1.0f || pLife is -1.0f || fLife is -1.0f)
            {
                maxEffectLifetime = float.PositiveInfinity;
            }

            //TODO: e-timeoffset? build-up-time?
            maxEffectLifetime = Math.Max(maxEffectLifetime, eLife + Math.Max(pLife, fLife));

            int pbindtoemitter = 0;
            if (!string.IsNullOrEmpty(bindValueRaw))
            {
                string[] parts = bindValueRaw.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (int.TryParse(parts[0], out int parsed))
                {
                    pbindtoemitter = parsed;
                }
            }

            string name = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
            lock (ParticlesData)
            {

                var particleDataList = ParticlesData.GetValueOrDefault(name) ?? (ParticlesData[name] = []);
                particleDataList.Add(new ParticleData(model, skin, maxEffectLifetime, pbindtoemitter));
            }
        }
    }

    internal static void LoadMapObjects(List<MapObject> objects, Dictionary<string, MapObject?>? locations)
    {
        string[] filesToLoad = Directory.GetFiles(ScenePath, "*.sco");
        foreach (var file in filesToLoad)
        {
            var name = Path.GetFileNameWithoutExtension(file);


            if (objects.Any(x => x.Name == name))
            {
                continue;
            }

            //Loads a Map Object from the Map's Scene folder
            var lines = File.ReadAllLines($"{ScenePath}/{name}.sco").ToList();
            var positionStr = lines.Find(x => x.StartsWith("CentralPoint"))!.Split('=')[1];
            var coords = positionStr.Split(' ');
            Vector3 pos = new()
            {
                X = float.Parse(coords[1]),
                Y = float.Parse(coords[2]),
                Z = float.Parse(coords[3])
            };
            var mapObject = new MapObject(name, pos);

            objects.Add(mapObject);
        }

        if (locations is null)
        {
            return;
        }

        string fileName = $"{MapPath}/Locations.dat";
        if (File.Exists(fileName))
        {
            foreach (var line in File.ReadAllLines(fileName))
            {
                if (line.StartsWith(';') || string.IsNullOrEmpty(line))
                {
                    continue;
                }

                string[] parts = line.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length is not 2)
                {
                    continue;
                }

                locations[parts[0]] = objects.Find(x => x.Name == parts[1]);
            }
        }
    }

    // Static Methods

    /// <summary>
    /// This is where the magic happens, this is where all Map information is loaded
    /// </summary>
    /// <param name="gameModeExpCurveOverride">Overrides the default exp curve of a map's default game-mode.</param>
    /// <param name="mapId">Map to read.</param>
    /// <returns>MapData containing all objects listed in the room file.</returns>
    internal static MapData GetMapData(int mapId)
    {
        MapData toReturnMapData = new(mapId);

        if (!Directory.Exists(ScenePath))
        {
            return toReturnMapData;
        }

        if (File.Exists($"{ScenePath}/MapObjects.mob"))
        {
            MobFile mobFile = new($"{ScenePath}/MapObjects.mob");
            toReturnMapData.MapObjects = new(mobFile.MapObjects);
        }

        string[] filesToLoad = Directory.GetFiles(ScenePath, "*.sco");
        foreach (var file in filesToLoad)
        {
            var name = Path.GetFileNameWithoutExtension(file);

            if (toReturnMapData.MapObjects.Any(x => x.Name == name))
            {
                continue;
            }

            //Loads a Map Object from the Map's Scene folder
            var lines = File.ReadAllLines($"{ScenePath}/{name}.sco").ToList();
            var positionStr = lines.Find(x => x.StartsWith("CentralPoint"))!.Split('=')[1];
            var coords = positionStr.Split(' ');
            Vector3 pos = new()
            {
                X = float.Parse(coords[1]),
                Y = float.Parse(coords[2]),
                Z = float.Parse(coords[3])
            };

            var mapObject = new MapObject(name, pos);

            toReturnMapData.MapObjects.Add(mapObject);
        }

        toReturnMapData.MapObjects.RemoveAll(x => x.Name.Contains("AIPath"));

        RFile contentFile = Cache.GetFile($"{MapPath}/DeathTimes.ini");
        float defaultVal = 0;
        for (int i = 1; i <= 18; i++)
        {
            float val = contentFile.GetValue("TimeDeadPerLevel", $"Level{i:D2}", defaultVal);
            toReturnMapData.DeathTimes.Add(val);
            defaultVal = val;
        }

        contentFile = Cache.GetFile($"{MapPath}/Items.ini");
        for (int i = 0; ; i++)
        {
            int val = contentFile.GetValue("ItemInclusionList", $"Item{i}", -1);
            if (val is -1)
            {
                break;
            }

            toReturnMapData.ItemInclusionList.Add(val);
        }

        for (int i = 0; ; i++)
        {
            int val = contentFile.GetValue("UnpurchasableItemList", $"Item{i}", -1);
            if (val is -1)
            {
                break;
            }

            toReturnMapData.UnpurchasableItemList.Add(val);
        }

        //TODO: ConstData class
        if (File.Exists($"{MapPath}/Constants.var"))
        {
            foreach (var line in File.ReadAllLines($"{MapPath}/Constants.var"))
            {
                if (line.StartsWith(';') || string.IsNullOrEmpty(line))
                {
                    continue;
                }

                string[] split = line.Trim().Split('=');
                if (float.TryParse(split[1].Split(';')[0], out float value))
                {
                    toReturnMapData.MapConstants.Add(split[0].Trim(), value);
                }
            }
        }

        if (File.Exists($"{MapPath}/Locations.dat"))
        {
            foreach (var line in File.ReadAllLines($"{MapPath}/Locations.dat"))
            {
                if (line.StartsWith(';') || string.IsNullOrEmpty(line))
                {
                    continue;
                }

                string[] parts = line.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var foundObject = toReturnMapData.MapObjects.Find(x => x.Name == parts[1]);
                    if (foundObject != null)
                    {
                        LocationData[parts[0]] = foundObject;
                    }
                }
            }

            toReturnMapData.LocationList = LocationData;
        }

        if (Directory.Exists(EncounterPath))
        {
            string[] filesToLoad2 = Directory.GetFiles(EncounterPath, "*.json");
            foreach (var file in filesToLoad2)
            {
                string key = Path.GetFileNameWithoutExtension(file);
                string json = File.ReadAllText(file);


                EncounterWrapper encounterWrapper =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<EncounterWrapper>(json);

                if (encounterWrapper != null && encounterWrapper.EncounterDefinition != null)
                {
                    EncounterDefinition encounter = encounterWrapper.EncounterDefinition;
                    EncounterData[key] = encounter;
                }
            }

            toReturnMapData.EncounterList = EncounterData;
        }

        if (Directory.Exists(MutatorPath))
        {
            string[] filesToLoad3 = Directory.GetFiles(MutatorPath, "*.json");
            foreach (var file in filesToLoad3)
            {
                string key = Path.GetFileNameWithoutExtension(file);
                string json = File.ReadAllText(file);


                MutatorWrapper MutatorWrapper = Newtonsoft.Json.JsonConvert.DeserializeObject<MutatorWrapper>(json);

                if (MutatorWrapper != null && MutatorWrapper.MutatorDefinition != null)
                {
                    MutatorDefinition mutator = MutatorWrapper.MutatorDefinition;


                    MutatorData[key] = mutator;
                }
            }

            toReturnMapData.MutatorList = MutatorData;
        }


        return toReturnMapData;
    }

    //Yes, I know there's a typo, but that's the actual function name in the game
    void LoadExcusionList()
    {
    }

    // ============================================================================
    // AI CONFIGURATION .DAT FILE LOADING METHODS
    // ============================================================================

    /// <summary>
    /// Loads all AI configuration .DAT files
    /// </summary>
    private void LoadAiConfigurationData()
    {
        string scriptsPath = $"{MapPath}/Scripts";

        if (!Directory.Exists(scriptsPath))
        {
            _logger.Warn($"AI Scripts directory doesn't exist: {scriptsPath}");
            return;
        }

        // Load AiManagerDataFile.dat
        LoadAiManagerDataFile(scriptsPath);

        // Load AiMissionDataFile.dat
        LoadAiMissionDataFile(scriptsPath);

        // Load AiTaskDataFile.dat
        LoadAiTaskDataFile(scriptsPath);

        // Load AiEntityDataFile.dat
        LoadAiEntityDataFile(scriptsPath);

        // Load AiFuzzyDataFile.dat
        LoadAiFuzzyDataFile(scriptsPath);

        // Load LevelScriptDataFile.dat
        LoadLevelScriptDataFile(scriptsPath);

        // Load LevelScriptDataFileServer.dat
        LoadLevelScriptDataFileServer(scriptsPath);

        // Load LevelScriptDataFileClient.dat
        LoadLevelScriptDataFileClient(scriptsPath);

        // Load team mission files
        LoadTeamMissionFiles(scriptsPath);

        // Load team task files
        LoadTeamTaskFiles(scriptsPath);

        _logger.Info($"AI Configuration loaded: {AiManagerData.Count} managers, {AiMissionData.Count} missions, {AiTaskData.Count} tasks");
    }

    /// <summary>
    /// Loads AiManagerDataFile.dat
    /// </summary>
    private void LoadAiManagerDataFile(string scriptsPath)
    {
        string filePath = $"{scriptsPath}/AiManagerDataFile.dat";
        if (File.Exists(filePath))
        {
            string currentGameMode = "";
            string currentGameType = "";

            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.StartsWith(';') || line.StartsWith('#') || string.IsNullOrEmpty(line))
                    continue;

                // Detect game modes
                if (line.Contains("<CLASSIC>") || line.Contains("<TUTORIAL>") || line.Contains("<ODIN>"))
                {
                    currentGameMode = line.Split('<')[1].Split('>')[0];
                    continue;
                }

                if (line.Contains("{") && line.Contains("}"))
                {
                    currentGameType = line.Split('{')[1].Split('}')[0];
                    continue;
                }

                // Parse manager mappings
                string[] parts = line.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    string team = parts[0];
                    string managerProject = parts[1];
                    string managerTag = parts[2];

                    string key = $"{currentGameMode}_{currentGameType}_{team}";
                    AiManagerData[key] = managerTag;
                }
            }
        }
    }

    /// <summary>
    /// Loads AiMissionDataFile.dat
    /// </summary>
    private void LoadAiMissionDataFile(string scriptsPath)
    {
        string filePath = $"{scriptsPath}/AiMissionDataFile.dat";
        if (File.Exists(filePath))
        {
            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.StartsWith(';') || line.StartsWith('#') || string.IsNullOrEmpty(line))
                    continue;

                string[] parts = line.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    string team = parts[0];
                    string missionFile = parts[1];

                    AiMissionData[team] = missionFile;
                }
            }
        }
    }

    /// <summary>
    /// Loads AiTaskDataFile.dat
    /// </summary>
    private void LoadAiTaskDataFile(string scriptsPath)
    {
        string filePath = $"{scriptsPath}/AiTaskDataFile.dat";
        if (File.Exists(filePath))
        {
            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.StartsWith(';') || line.StartsWith('#') || string.IsNullOrEmpty(line))
                    continue;

                string[] parts = line.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    string team = parts[0];
                    string taskFile = parts[1];

                    AiTaskData[team] = taskFile;
                }
            }
        }
    }

    /// <summary>
    /// Loads AiEntityDataFile.dat
    /// </summary>
    private void LoadAiEntityDataFile(string scriptsPath)
    {
        string filePath = $"{scriptsPath}/AiEntityDataFile.dat";
        if (File.Exists(filePath))
        {
            string currentSection = "";
            string currentGameMode = "";
            string currentGameType = "";

            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.StartsWith(';') || line.StartsWith('#') || string.IsNullOrEmpty(line))
                    continue;

                // Detect sections
                if (line.Contains("[Champions]"))
                {
                    currentSection = "Champions";
                    continue;
                }
                else if (line.Contains("[Minions]"))
                {
                    currentSection = "Minions";
                    continue;
                }
                else if (line.Contains("[Creatures]"))
                {
                    currentSection = "Creatures";
                    continue;
                }

                // Detect game modes
                if (line.Contains("<CLASSIC>") || line.Contains("<TUTORIAL>"))
                {
                    currentGameMode = line.Split('<')[1].Split('>')[0];
                    continue;
                }

                if (line.Contains("{") && line.Contains("}"))
                {
                    currentGameType = line.Split('{')[1].Split('}')[0];
                    continue;
                }

                // Parse entities
                string[] parts = line.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4)
                {
                    string uniqueName = parts[0];
                    string skinName = parts[1];
                    string projectName = parts[2];
                    string tag = parts[3];

                    string key = $"{currentGameMode}_{currentGameType}_{uniqueName}";
                    AiEntityData[key] = tag;
                }
            }
        }
    }

    /// <summary>
    /// Loads AiFuzzyDataFile.dat
    /// </summary>
    private void LoadAiFuzzyDataFile(string scriptsPath)
    {
        string filePath = $"{scriptsPath}/AiFuzzyDataFile.dat";
        if (File.Exists(filePath))
        {
            string currentGameMode = "";
            string currentGameType = "";

            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.StartsWith(';') || line.StartsWith('#') || string.IsNullOrEmpty(line))
                    continue;

                // Detect game modes
                if (line.Contains("<CLASSIC") || line.Contains("<TUTORIAL"))
                {
                    currentGameMode = line.Split('<')[1].Split(',')[0];
                    continue;
                }

                if (line.Contains("{") && line.Contains("}"))
                {
                    currentGameType = line.Split('{')[1].Split('}')[0];
                    continue;
                }

                // Parse fuzzy tables
                string[] parts = line.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    string fuzzyTag = parts[0];
                    string fuzzyTableFile = parts[1];

                    string key = $"{currentGameMode}_{currentGameType}_{fuzzyTag}";
                    AiFuzzyData[key] = fuzzyTableFile;
                }
            }
        }
    }

    /// <summary>
    /// Loads LevelScriptDataFile.dat
    /// </summary>
    private void LoadLevelScriptDataFile(string scriptsPath)
    {
        string filePath = $"{scriptsPath}/LevelScriptDataFile.dat";
        if (File.Exists(filePath))
        {
            string currentGameMode = "";

            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.StartsWith(';') || line.StartsWith('#') || string.IsNullOrEmpty(line))
                    continue;

                // Detect game modes
                if (line.Contains("<TUTORIAL>"))
                {
                    currentGameMode = "TUTORIAL";
                    continue;
                }

                // Parse level scripts
                string[] parts = line.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    string scriptProject = parts[0];
                    string scriptTag = parts[1];

                    string key = $"{currentGameMode}_{scriptProject}";
                    LevelScriptData[key] = scriptTag;
                }
            }
        }
    }

    /// <summary>
    /// Loads LevelScriptDataFileServer.dat
    /// </summary>
    private void LoadLevelScriptDataFileServer(string scriptsPath)
    {
        string filePath = $"{scriptsPath}/LevelScriptDataFileServer.dat";
        if (File.Exists(filePath))
        {
            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.StartsWith(';') || line.StartsWith('#') || string.IsNullOrEmpty(line))
                    continue;

                string[] parts = line.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    string key = parts[0];
                    string value = parts[1];
                    LevelScriptDataFileServerData[key] = value;
                }
            }
        }
    }

    /// <summary>
    /// Loads LevelScriptDataFileClient.dat
    /// </summary>
    private void LoadLevelScriptDataFileClient(string scriptsPath)
    {
        string filePath = $"{scriptsPath}/LevelScriptDataFileClient.dat";
        if (File.Exists(filePath))
        {
            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.StartsWith(';') || line.StartsWith('#') || string.IsNullOrEmpty(line))
                    continue;

                string[] parts = line.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    string key = parts[0];
                    string value = parts[1];
                    LevelScriptDataFileClientData[key] = value;
                }
            }
        }
    }

    /// <summary>
    /// Loads team mission files
    /// </summary>
    private void LoadTeamMissionFiles(string scriptsPath)
    {
        // Load TeamOrderAiMission.dat
        LoadTeamMissionFile(scriptsPath, "TeamOrderAiMission.dat", TeamOrderAiMissionData);

        // Load TeamChaosAiMission.dat
        LoadTeamMissionFile(scriptsPath, "TeamChaosAiMission.dat", TeamChaosAiMissionData);

        // Load TeamNeutralAiMission.dat
        LoadTeamMissionFile(scriptsPath, "TeamNeutralAiMission.dat", TeamNeutralAiMissionData);
    }

    /// <summary>
    /// Loads a specific team mission file
    /// </summary>
    private void LoadTeamMissionFile(string scriptsPath, string fileName, Dictionary<string, string> targetDict)
    {
        string filePath = $"{scriptsPath}/{fileName}";
        if (File.Exists(filePath))
        {
            string currentGameMode = "";
            string currentGameType = "";

            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.StartsWith(';') || line.StartsWith('#') || string.IsNullOrEmpty(line))
                    continue;

                // Detect game modes
                if (line.Contains("<CLASSIC>") || line.Contains("<TUTORIAL>"))
                {
                    currentGameMode = line.Split('<')[1].Split('>')[0];
                    continue;
                }

                if (line.Contains("{") && line.Contains("}"))
                {
                    currentGameType = line.Split('{')[1].Split('}')[0];
                    continue;
                }

                // Parse missions
                string[] parts = line.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4)
                {
                    string missionTopic = parts[0];
                    string missionProject = parts[1];
                    string missionTag = parts[2];
                    string priority = parts[3];

                    string key = $"{currentGameMode}_{currentGameType}_{missionTopic}";
                    targetDict[key] = missionTag;
                }
            }
        }
    }

    /// <summary>
    /// Loads team task files
    /// </summary>
    private void LoadTeamTaskFiles(string scriptsPath)
    {
        // Load TeamOrderAiTask.dat
        LoadTeamTaskFile(scriptsPath, "TeamOrderAiTask.dat", TeamOrderAiTaskData);

        // Load TeamChaosAiTask.dat
        LoadTeamTaskFile(scriptsPath, "TeamChaosAiTask.dat", TeamChaosAiTaskData);

        // Load TeamNeutralAiTask.dat
        LoadTeamTaskFile(scriptsPath, "TeamNeutralAiTask.dat", TeamNeutralAiTaskData);
    }

    /// <summary>
    /// Loads a specific team task file
    /// </summary>
    private void LoadTeamTaskFile(string scriptsPath, string fileName, Dictionary<string, string> targetDict)
    {
        string filePath = $"{scriptsPath}/{fileName}";
        if (File.Exists(filePath))
        {
            string currentGameMode = "";

            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.StartsWith(';') || line.StartsWith('#') || string.IsNullOrEmpty(line))
                    continue;

                // Detect game modes
                if (line.Contains("<CLASSIC") || line.Contains("<TUTORIAL"))
                {
                    currentGameMode = line.Split('<')[1].Split(',')[0];
                    continue;
                }

                // Parse tasks
                string[] parts = line.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 5)
                {
                    string taskTopic = parts[0];
                    string taskXmlFile = parts[1];
                    string taskLogicFile = parts[2];
                    string priority = parts[3];
                    string actions = parts[4];

                    string key = $"{currentGameMode}_{taskTopic}";
                    targetDict[key] = taskXmlFile;
                }
            }
        }
    }

    /// <summary>
    /// Returns the CharData for a given Character
    /// </summary>
    /// <param name="characterName"></param>
    /// <returns></returns>
    internal static CharData? GetCharData(string characterName)
    {
        if (CharactersData.TryGetValue(characterName.ToLowerInvariant(), out var charData))
        {
            return charData;
        }

        if (!string.IsNullOrEmpty(characterName))
        {
            _logger.Error($"Could not find CharData for Character \"{characterName}\"!");
        }

        return null;
    }

    /// <summary>
    /// Returns SpellData respective to a given Spell
    /// </summary>
    /// <param name="spellName"></param>
    /// <returns></returns>
    internal static SpellData? GetSpellData(string spellName)
    {
        if (string.IsNullOrEmpty(spellName))
        {
            //TODO: This gets triggered a lot for some reason.
            //_logger.Error($"Provided spell name is null or empty.");
            return null;
        }

        if (SpellsData.TryGetValue(spellName.ToLowerInvariant(), out var spellData))
        {
            return spellData;
        }

        _logger.Error($"Could not find SpellData for Spell \"{spellName}\"!");
        return null;
    }

    /// <summary>
    /// Returns ParticleData respective to a given Particle
    /// </summary>
    /// <param name="name">name of the particle</param>
    /// <param name="characters">Characters in order of priority, in whose files to look for a particle.</param>
    /// <returns></returns>
    internal static ParticleData? GetParticleData(string name, params GameObject[] characters)
    {
        if (!ParticlesData.TryGetValue(name.ToLowerInvariant(), out var list) || list.Count <= 0)
        {
            return null;
        }

        ParticleData? particleData;
        foreach (var character in characters)
        {
            if (character is not AttackableUnit u)
            {
                continue;
            }

            string model = u.Model.ToLowerInvariant();

            if (character is ObjAIBase ai)
            {
                particleData = list.Find(data => data.Model == model && data.SkinId == ai.SkinID);
                if (particleData is not null)
                {
                    return particleData;
                }
            }

            particleData = list.Find(data => data.Model == model && data.SkinId == -1);
            if (particleData is not null)
            {
                return particleData;
            }
        }

        particleData = list.Find(data => data.Model == "" && data.SkinId == -1);
        if (particleData is not null)
        {
            return list.First();
        }

        return null;
    }

    /// <summary>
    /// Returns TalentData for a given Talent
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    internal static TalentData? GetTalentData(string id)
    {
        if (TalentsData.TryGetValue(id.ToLowerInvariant(), out var data))
        {
            return data;
        }

        _logger.Error($"Could not find TalentData for Talent \"{id}\"!");
        return null;
    }

    /// <summary>
    /// Returns the ItemData for a given ItemID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    internal static ItemData? GetItemData(int id)
    {
        if (ItemsData.TryGetValue(id, out var data))
        {
            return data;
        }

        _logger.Error($"Could not find ItemData for Item \"{id}\"!");
        return null;
    }

    /// <summary>
    /// Loads and parses the map's NavGrid
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    internal static NavigationGrid GetNavigationGrid(MapHandler map)
    {
        var navGridName = "AIPath";
        if (!string.IsNullOrEmpty(map.GameMode.MapScriptMetadata.NavGridOverride))
        {
            navGridName = map.GameMode.MapScriptMetadata.NavGridOverride;
        }

        return new NavigationGrid($"{MapPath}/{navGridName}.aimesh_ngrid");
    }

    /// <summary>
    /// Loads and parses the map's NavGrid
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    internal static NaviMeshTest GetAiMesh(MapHandler map)
    {
        var navGridName = "AIPath";
        if (!string.IsNullOrEmpty(map.GameMode.MapScriptMetadata.NavGridOverride))
        {
            navGridName = map.GameMode.MapScriptMetadata.NavGridOverride;
        }

        return new NaviMeshTest($"{MapPath}/{navGridName}.aimesh");
    }

}