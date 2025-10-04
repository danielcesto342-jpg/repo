using System;
using ChildrenOfTheGraveEnumNetwork.Enums;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits.AI;
using System.Collections.Generic;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Content;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Inventory;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Logging;
using log4net;
using Newtonsoft.Json.Linq;

namespace ChildrenOfTheGraveEnumNetwork.NetInfo
{
    public class ClientInfo
    {
        private static ILog _logger = LoggerProvider.GetLogger();

        public static readonly Dictionary<TeamId, byte> CurrentTeamSpawnIndex = new();
        public long PlayerId { get; private set; }
        public int ClientId { get; set; }
        public bool IsMatchingVersion { get; set; }
        /// <summary>
        /// False if the client sent a StartGame request.
        /// </summary>
        public bool IsDisconnected { get; set; }
        /// <summary>
        /// True if the client sent a Handshake request.
        /// </summary>
        public bool IsStartedClient { get; set; }
        public int SkinNo { get; private set; }
        public string[] SummonerSkills { get; private set; }
        public string Name { get; private set; }
        public string Rank { get; private set; }
        public short Ribbon { get; private set; }
        public int Icon { get; private set; }
        public TeamId Team { get; private set; }
        public byte InitialSpawnIndex { get; private set; }


        public bool IsReady = false;


        private Champion _champion;
        public Champion Champion
        {
            get => _champion;
            set
            {
                _champion = value;
                _champion.UpdateSkin(SkinNo);
            }
        }

        public RuneInventory Runes { get; private set; }
        public TalentInventory Talents { get; private set; }

        public ClientInfo
        (
            string rank,
            TeamId team,
            short ribbon,
            int icon,
            int skinNo,
            string name,
            string[] summonerSkills,
            long playerId,
            JToken? runes = null,
            JToken? talents = null
        )
        {
            Rank = rank;
            Team = team;
            Ribbon = ribbon;
            Icon = icon;
            SkinNo = skinNo;
            IsMatchingVersion = true;
            Name = name;
            SummonerSkills = summonerSkills;
            PlayerId = playerId;
            IsDisconnected = true;
            IsStartedClient = false;
            Runes = CreateRuneInventory(runes);
            Talents = CreateTalentInventory(talents);

            CurrentTeamSpawnIndex.TryAdd(team, 0);
            InitialSpawnIndex = CurrentTeamSpawnIndex[team]++;
        }

        private RuneInventory CreateRuneInventory(JToken? runesConfig)
        {
            var runes = new RuneInventory();

            if (runesConfig == null)
            {
                _logger.Warn($"No runes found for player {Name}!");
                return runes;
            }

            foreach (var jToken in runesConfig)
            {
                var runeCategory = (JProperty)jToken;
                runes.Add(Convert.ToInt32(runeCategory.Name), Convert.ToInt32(runeCategory.Value));
            }

            return runes;
        }

        private TalentInventory CreateTalentInventory(JToken? talentsConfig)
        {
            var talents = new TalentInventory();

            if (talentsConfig == null)
            {
                _logger.Warn($"No talents found for player {Name}!");
                return talents;
            }

            foreach (var jToken in talentsConfig)
            {
                var talent = (JProperty)jToken;
                byte level = 1;

                try
                {
                    level = talent.Value.Value<byte>();
                }
                catch
                {
                    _logger.Warn(
                        $"Invalid Talent Rank for Talent {talent.Name}! " +
                        $"Please use ranks between 1 and {byte.MaxValue}! " +
                        "Defaulting to Rank 1...");
                }

                try
                {
                    var data = ContentManager.GetTalentData(talent.Name) ?? new(talent.Name);
                    talents.Add(data, level);
                }
                catch (FormatException)
                {
                    _logger.Warn($"Invalid Talent Id: {talent.Name}");
                }
            }

            return talents;
        }
    }
}
