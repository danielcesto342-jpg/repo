using System;
using System.Globalization;
using System.Numerics;
using ChildrenOfTheGraveEnumNetwork.Enums;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits.AI;
using ChildrenOfTheGraveEnumNetwork.NetInfo;
using static ChildrenOfTheGrave.ChildrenOfTheGraveServer.API.GameAnnouncementManager;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class CcCommand : ChatCommandBase
    {
        public override string Command => "cc";
        public override string Syntax => $"{Command} ccType duration";

        public CcCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');

            if (split.Length < 3)
            {
                ChatManager.SyntaxError();
                ShowSyntax();
                ChatManager.Info("Available CC types: stun, silence, blind, slow, fear, taunt, root, suppression");
                ChatManager.Info("CC will always be applied from an enemy source to test passive effects.");
                return;
            }

            var ccType = split[1];  // CC type is the second argument

            if (!float.TryParse(split[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var duration))
            {
                ChatManager.Error($"Invalid duration '{split[2]}'. Please enter a valid number.");
                return;
            }

            if (duration <= 0)
            {
                ChatManager.Error("Duration must be greater than 0.");
                return;
            }
            var champion = Game.PlayerManager.GetPeerInfo(userId).Champion;

            // Determine enemy team
            var enemyTeam = champion.Team == TeamId.TEAM_ORDER ? TeamId.TEAM_CHAOS : TeamId.TEAM_ORDER;

            // Spawn temporary enemy bot
            var tempBot = SpawnTempEnemyBot(champion, enemyTeam, "Garen");

            if (tempBot == null)
            {
                ChatManager.Error("Failed to spawn temporary enemy bot.");
                return;
            }

            ChatManager.Info($"Spawned temporary enemy bot '{tempBot.Name}' to apply {ccType} for {duration} seconds");

            // Apply CC from the enemy bot
            switch (ccType)
            {
                case "stun":
                    champion.Buffs.Add("Stun", duration, 1, null, champion, tempBot);
                    ChatManager.Info($"Enemy bot stunned you for {duration} seconds!");
                    break;
                case "silence":
                    champion.Buffs.Add("Silence", duration, 1, null, champion, tempBot);
                    ChatManager.Info($"Enemy bot silenced you for {duration} seconds!");
                    break;
                case "blind":
                    champion.Buffs.Add("BlindingDart", duration, 1, null, champion, tempBot);
                    ChatManager.Info($"Enemy bot blinded you for {duration} seconds!");
                    break;
                case "slow":
                    champion.Buffs.Add("Internal_30Slow", duration, 1, null, champion, tempBot);
                    champion.Buffs.Add("ItemSlow", duration, 1, null, champion, tempBot);
                    ChatManager.Info($"Enemy bot slowed you for {duration} seconds!");
                    break;
                case "fear":
                    champion.Buffs.Add("Fear", duration, 1, null, champion, tempBot);
                    ChatManager.Info($"Enemy bot feared you for {duration} seconds!");
                    break;
                case "taunt":
                    champion.Buffs.Add("Taunt", duration, 1, null, champion, tempBot);
                    ChatManager.Info($"Enemy bot taunted you for {duration} seconds!");
                    break;
                case "root":
                case "snare":
                    champion.Buffs.Add("LeonaZenithBladeRoot", duration, 1, null, champion, tempBot);
                    ChatManager.Info($"Enemy bot rooted you for {duration} seconds!");
                    break;
                case "suppression":
                case "suppress":
                    champion.Buffs.Add("Suppression", duration, 1, null, champion, tempBot);
                    ChatManager.Info($"Enemy bot suppressed you for {duration} seconds!");
                    break;
                default:
                    ChatManager.Error($"Unknown CC type '{ccType}'. Available types: stun, silence, blind, slow, fear, taunt, root, suppression");
                    RemoveTempBot(tempBot);
                    return;
            }

            // Schedule bot removal after a short delay (longer than CC duration to avoid issues)
            var removalDelay = Math.Max(duration + 1.0f, 2.0f);
            var removalTime = (Game.Time.GameTime / 1000f) + removalDelay;
            CreateTimedEvent(() => RemoveTempBot(tempBot), removalTime);
        }

        private Champion SpawnTempEnemyBot(Champion playerChampion, TeamId enemyTeam, string model)
        {
            try
            {
                // Position the bot very far away from the player to avoid any interactions
                var botPosition = playerChampion.Position + new Vector2(5000, 5000);

                // Create temporary client info for the bot
                var clientInfoTemp = new ClientInfo("TempBotCC", enemyTeam, 0, 0, 0, $"TempBot_{model}",
                    new string[] { "SummonerHeal", "SummonerFlash" }, -1);

                Game.PlayerManager.AddPlayer(clientInfoTemp);

                var bot = new Champion(model, clientInfoTemp, team: enemyTeam);
                clientInfoTemp.Champion = bot;

                bot.SetPosition(botPosition, false);
                bot.StopMovement();
                bot.UpdateMoveOrder(OrderType.Stop);

                // Make it level 1 and give some health so it doesn't immediately die
                bot.Experience.LevelUp();

                // Make it completely passive and untargetable
                bot.SetStatus(StatusFlags.Invulnerable, true);
                bot.SetStatus(StatusFlags.Targetable, false);
                bot.SetStatus(StatusFlags.Ghosted, true);
                bot.SetStatus(StatusFlags.CanAttack, false);
                bot.PauseAI(true);  // Disable AI completely

                Game.ObjectManager.AddObject(bot);

                return bot;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        private void RemoveTempBot(Champion bot)
        {
            try
            {
                if (bot != null && Game.ObjectManager.GetObjectById(bot.NetId) != null)
                {
                    Game.ObjectManager.RemoveObject(bot);

                    ChatManager.Info($"Removed temporary enemy bot '{bot.Name}'");
                }
            }
            catch (System.Exception ex)
            {
            }
        }
    }
}