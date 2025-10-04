using System;
using System.Numerics;
using ChildrenOfTheGraveEnumNetwork.Enums;
using ChildrenOfTheGraveEnumNetwork.NetInfo;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.API;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Content;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits.AI;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.StatsNS;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Inventory;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class SpawnCommand : ChatCommandBase
    {
        public override string Command => "spawn";
        public override string Syntax => $"{Command} champorder [champion], champchaos [champion], minionsorder, minionschaos, regionorder [size, time], regionchaos [size, time]";

        public SpawnCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
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
            else if (split[1].StartsWith("minions"))
            {
                split[1] = split[1].Replace("minions", "team_").ToUpper();
                if (!Enum.TryParse(split[1], out TeamId team) || team == TeamId.TEAM_NEUTRAL)
                {
                    ChatManager.SyntaxError();
                    ShowSyntax();
                    return;
                }

                SpawnMinionsForTeam(team, userId);
            }
            else if (split[1].StartsWith("champ"))
            {
                split[1] = split[1].Replace("champ", "team_").ToUpper();
                if (!Enum.TryParse(split[1], out TeamId team) || team == TeamId.TEAM_NEUTRAL)
                {
                    ChatManager.SyntaxError();
                    ShowSyntax();
                    return;
                }

                if (split.Length > 2)
                {
                    string championModel = arguments.Split(' ')[2];

                    if (ContentManager.GetCharData(championModel) is null)
                    {
                        ChatManager.SyntaxError("Character Name: " + championModel + " invalid.");
                        ShowSyntax();
                        return;
                    }

                    SpawnChampForTeam(team, userId, championModel);
                    return;
                }

                SpawnChampForTeam(team, userId, "Katarina");
            }
            else if (split[1].StartsWith("region"))
            {
                float size = 250.0f;
                float time = -1f;

                split[1] = split[1].Replace("region", "team_").ToUpper();
                if (!Enum.TryParse(split[1], out TeamId team) || team == TeamId.TEAM_NEUTRAL)
                {
                    ChatManager.SyntaxError();
                    ShowSyntax();
                    return;
                }

                if (split.Length > 2)
                {
                    size = float.Parse(arguments.Split(' ')[2]);

                    if (split.Length > 3)
                    {
                        time = float.Parse(arguments.Split(' ')[2]);
                    }
                }
                else if (split.Length > 4)
                {
                    ChatManager.SyntaxError();
                    ShowSyntax();
                    return;
                }

                SpawnRegionForTeam(team, userId, size, time);
            }
        }

        public void SpawnMinionsForTeam(TeamId team, int userId)
        {
            var championPos = Game.PlayerManager.GetPeerInfo(userId).Champion.Position;
            var random = new Random();

            string teamName = team is TeamId.TEAM_ORDER ? "Blue" : "Red";

            Minion[] minions = new Minion[]
            {
                new(null, championPos, $"{teamName}_Minion_Basic", "MELEE", team, AIScript: "idle.lua"),
                new(null, championPos, $"{teamName}_Minion_MechCannon", "CANNON", team, AIScript: "idle.lua"),
                new(null, championPos, $"{teamName}_Minion_Wizard", $"CASTER", team, AIScript: "idle.lua"),
                new(null, championPos, $"{teamName}_Minion_MechMelee", $"SUPER", team, AIScript: "idle.lua")
            };

            const int MaxDistance = 400;
            foreach (var minion in minions)
            {
                minion.SetPosition(championPos + new Vector2(random.Next(-MaxDistance, MaxDistance), random.Next(-MaxDistance, MaxDistance)), false);
                minion.PauseAI(true);
                minion.StopMovement();
                minion.UpdateMoveOrder(OrderType.Hold);
                Game.ObjectManager.AddObject(minion);
            }
        }

        public void SpawnChampForTeam(TeamId team, int userId, string model)
        {
            var championPos = Game.PlayerManager.GetPeerInfo(userId).Champion.Position;

            var runesTemp = new RuneInventory();
            var talents = new TalentInventory();
            var clientInfoTemp = new ClientInfo("", team, 0, 0, 0, $"{model} Bot", new string[] { "SummonerHeal", "SummonerFlash" }, -1);

            Game.PlayerManager.AddPlayer(clientInfoTemp);

            var c = new Champion(
                model,
                clientInfoTemp,
                team: team
            );

            clientInfoTemp.Champion = c;

            c.SetPosition(championPos, false);
            c.StopMovement();
            c.UpdateMoveOrder(OrderType.Stop);
            c.Experience.LevelUp();

            StatsModifier statsModifier = new();
            statsModifier.HealthPoints.FlatBonus = 10000 - c.Stats.HealthPoints.Total;
            c.AddStatModifier(statsModifier);

            Game.ObjectManager.AddObject(c);

            ChatManager.Info($"Spawned Bot {c.Name} as {c.Model} with NetID: {c.NetId}.");
        }

        public void SpawnRegionForTeam(TeamId team, int userId, float radius = 250.0f, float lifetime = -1.0f)
        {
            var championPos = Game.PlayerManager.GetPeerInfo(userId).Champion.Position;
            ApiFunctionManager.AddPosPerceptionBubble(championPos, radius, lifetime, team, true);
        }
    }
}