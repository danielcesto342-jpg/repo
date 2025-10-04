
using System;
using System.Collections.Generic;
using ChildrenOfTheGraveEnumNetwork.Enums;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Content;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects;
using MoonSharp.Interpreter;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer;

public class MapData
{
    public int Id { get; private set; }
    public Dictionary<string, float> MapConstants { get; private set; } = new();
    /// <summary>
    /// Collection of MapObjects present within a map's room file, with the key being the name present in the room file. Refer to <see cref="MapObject"/>.
    /// </summary>
    public List<MapObject> MapObjects { get; internal set; } = new();
    /// <summary>
    /// Amount of time death should last depending on level.
    /// </summary>
    public List<float> DeathTimes { get; private set; } = new();
    /// <summary>
    /// Potential progression of stats per-level of jungle monsters.
    /// </summary>
    /// TODO: Figure out what this is and how to implement it.
    [Obsolete("This doesn't exist in 126.0.0.1. Remove Later")]
    public List<float> StatsProgression { get; private set; } = new();

    public List<int> UnpurchasableItemList = new();
    public List<int> ItemInclusionList = new();

    public Dictionary<string, MapObject> LocationList = new();
    public Dictionary<string, EncounterDefinition> EncounterList = new();
    public Dictionary<string, MutatorDefinition> MutatorList = new();
    public Dictionary<TeamId, float> MapScoring = new();
    public MapData(int mapId)
    {
        Id = mapId;

    }




    public EncounterManager encounterManager = new();
    public MutatorManager mutatorManager = new();
    public AISquadListManager aisquadlistmanager = new();

    public DynValue DefaultNeutralMinionValues = default;
}