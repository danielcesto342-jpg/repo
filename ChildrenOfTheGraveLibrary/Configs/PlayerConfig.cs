using ChildrenOfTheGraveEnumNetwork;
using ChildrenOfTheGraveEnumNetwork.Enums;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Logging;
using log4net;
using Newtonsoft.Json.Linq;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer;

public class PlayerConfig
{
    public long PlayerID { get; private set; }
    public string Rank { get; private set; }
    public string Name { get; private set; }
    public string Champion { get; private set; }
    public TeamId Team { get; private set; }
    public short Skin { get; private set; }
    public string Summoner1 { get; private set; }
    public string Summoner2 { get; private set; }
    public short Ribbon { get; private set; }
    public int Icon { get; private set; }
    public string BlowfishKey { get; private set; }
    public JToken? Runes { get; private set; }
    public JToken? Talents { get; private set; }
    public bool UseDoomSpells { get; private set; }
    private JToken _playerData;

    public EntityDiffcultyType AIDifficulty { get; private set; }

    private static ILog _logger = LoggerProvider.GetLogger();

    public PlayerConfig(JToken playerData)
    {
        _playerData = playerData;
        PlayerID = playerData.Value<long>("playerId");
        Rank = playerData.Value<string>("rank") ?? "DIAMOND";
        Name = playerData.Value<string>("name") ?? "Test";
        Champion = playerData.Value<string>("champion") ?? "";

        Team = playerData.Value<string>("team").GetTeamFromString();

        Skin = playerData.Value<short>("skin");
        Summoner1 = playerData.Value<string>("summoner1") ?? "SummonerFlash";
        Summoner2 = playerData.Value<string>("summoner2") ?? "SummonerHeal";
        Ribbon = playerData.Value<short>("ribbon");
        Icon = playerData.Value<int>("icon");
        BlowfishKey = playerData.Value<string>("blowfishKey") ?? "";

        Runes = _playerData.SelectToken("runes");
        Talents = _playerData.SelectToken("talents");

        AIDifficulty = (EntityDiffcultyType)playerData.Value<int>("AIDifficulty");
        UseDoomSpells = playerData.Value<bool>("useDoomSpells");
    }
}