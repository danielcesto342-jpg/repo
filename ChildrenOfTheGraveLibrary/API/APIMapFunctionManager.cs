using System.Numerics;
using ChildrenOfTheGraveEnumNetwork.Enums;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits.AI;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Handlers;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Logging;
using log4net;
using MapScripts;
//using SiphoningStrike.Game.Common;
using static PacketVersioning.PktVersioning;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.API
{
    public static class ApiMapFunctionManager
    {
        // Required variables.
        private static ILog _logger = LoggerProvider.GetLogger();









        /// <summary>
        /// Checks if minion spawn is enabled
        /// </summary>
        /// <returns></returns>
        public static bool IsMinionSpawnEnabled()
        {
            return Game.Config.GameFeatures.HasFlag(FeatureFlags.EnableLaneMinions);
        }



        /// <summary>
        /// Adds a prop to the map
        /// </summary>
        /// <param name="name"></param>
        /// <param name="model"></param>
        /// <param name="position"></param>
        /// <param name="height"></param>
        /// <param name="direction"></param>
        /// <param name="posOffset"></param>
        /// <param name="scale"></param>
        /// <param name="skinId"></param>
        /// <param name="skillLevel"></param>
        /// <param name="rank"></param>
        /// <param name="type"></param>
        /// <param name="netId"></param>
        /// <param name="netNodeId"></param>
        /// <returns></returns>
        public static LevelProp AddLevelProp(string name, string model, Vector3 position, Vector3 direction, Vector3 posOffset, Vector3 scale, int skinId = 0, byte skillLevel = 0, byte rank = 0, byte type = 2, uint netId = 0, byte netNodeId = 64)
        {
            return new LevelProp(netNodeId, name, model, position, direction, posOffset, scale, skinId, skillLevel, rank, type, netId);
        }

        /// <summary>
        /// Notifies prop animations (Such as the stairs at the beginning on Dominion)
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="animation"></param>
        /// <param name="animationFlag"></param>
        /// <param name="duration"></param>
        /// <param name="destroyPropAfterAnimation"></param>
        public static void NotifyPropAnimation(LevelProp prop, string animation, AnimationFlags animationFlag, float duration, bool destroyPropAfterAnimation)
        {
            CreateAnimationdata(prop, animation, animationFlag, duration, destroyPropAfterAnimation);

        }

        /// <summary>
        /// Sets up the surrender functionality
        /// </summary>
        /// <param name="time"></param>
        /// <param name="restTime"></param>
        /// <param name="length"></param>
        public static void AddSurrender(float time, float restTime, float length)
        {
            Game.Map.Surrenders.Add(TeamId.TEAM_ORDER, new SurrenderHandler(TeamId.TEAM_ORDER, time, restTime, length));
            Game.Map.Surrenders.Add(TeamId.TEAM_CHAOS, new SurrenderHandler(TeamId.TEAM_CHAOS, time, restTime, length));
        }

        public static void HandleSurrender(int userId, Champion who, bool vote)
        {
            if (Game.Map.Surrenders.TryGetValue(who.Team, out SurrenderHandler value))
            {
                value.HandleSurrender(userId, who, vote);
            }
        }




        /// <summary>
        /// I couldn't tell the functionality for this besides Notifying the scoreboard at the start of the match
        /// </summary>
        /// <param name="capturePointIndex"></param>
        /// <param name="otherNetId"></param>
        /// <param name="PARType"></param>
        /// <param name="attackTeam"></param>
        /// <param name="capturePointUpdateCommand"></param>
        public static void NotifyHandleCapturePointUpdate(int capturePointIndex, uint otherNetId, byte PARType, byte attackTeam, CapturePointUpdateCommand capturePointUpdateCommand)
        {
            HandleCapturePointUpdateNotify(capturePointIndex, otherNetId, PARType, attackTeam, capturePointUpdateCommand);
        }




        public static ILevelScript GetLevelScript()
        {
            return Game.Map.LevelScript;
        }

        public static float GetTeamScore(TeamId team)
        {
            if (Game.Map.MapData.MapScoring.ContainsKey(team))
            {
                return Game.Map.MapData.MapScoring[team];
            }
            return 500.0f;
        }
    }
}
