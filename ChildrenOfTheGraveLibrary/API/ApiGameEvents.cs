using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits.AI;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Logging;
using log4net;
using SiphoningStrike.Game.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using static PacketVersioning.PktVersioning;

internal class TimedEvent
{
    internal float TimeOfExecution;
    internal Action Event;
}

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.API
{
    public static class GameAnnouncementManager
    {
        private static ILog _logger = LoggerProvider.GetLogger();

        static List<TimedEvent> Events = new();
        public static void Update()
        {
            while (Game.Time.GameTime >= Events.FirstOrDefault()?.TimeOfExecution)
            {
                TimedEvent timedEvent = Events.First();
                timedEvent.Event();
                Events.Remove(timedEvent);
            }
        }

        public static void CreateTimedEvent(Action timedEvent, float timeOfExecution)
        {
            if (timedEvent is not null)
            {
                Events.Add(new() { Event = timedEvent, TimeOfExecution = timeOfExecution * 1000 });
                Events = Events.OrderBy(x => x.TimeOfExecution).ToList();
            }
        }



        public static void AnnounceCapturePointCaptured(LaneTurret turret, char point, Champion? captor = null)
        {
            IEvent captured;
            switch (char.ToUpper(point))
            {
                case 'A':
                    captured = new OnCapturePointCaptured_A();
                    break;
                case 'B':
                    captured = new OnCapturePointCaptured_B();
                    break;
                case 'C':
                    captured = new OnCapturePointCaptured_C();
                    break;
                case 'D':
                    captured = new OnCapturePointCaptured_D();
                    break;
                case 'E':
                    captured = new OnCapturePointCaptured_E();
                    break;
                default:
                    _logger.Warn($"Announcement with Id {point} doesn't exist! Please use numbers between A and E");
                    return;
            }
            captured.OtherNetID = captor?.NetId ?? 0;

            OnEventNotify(captured, turret);
        }

        public static void AnnounceCapturePointNeutralized(LaneTurret turret, char point)
        {
            IEvent neutralized;
            switch (char.ToUpper(point))
            {
                case 'A':
                    neutralized = new OnCapturePointNeutralized_A();
                    break;
                case 'B':
                    neutralized = new OnCapturePointNeutralized_B();
                    break;
                case 'C':
                    neutralized = new OnCapturePointNeutralized_C();
                    break;
                case 'D':
                    neutralized = new OnCapturePointNeutralized_D();
                    break;
                case 'E':
                    neutralized = new OnCapturePointNeutralized_E();
                    break;
                default:
                    _logger.Warn($"Announcement with Id {point} doesn't exist! Please use numbers between 1 and 5");
                    return;
            }

            OnEventNotify(neutralized, turret);
        }





        public static void AnnounceKillDragon(DeathData data)
        {
            var killDragon = new OnSuperMonsterKill()
            {
                //TODO: Figure out all the parameters, their values look random(?).
                //All Map11 replays have the same values in this event besides OtherNetId.
                OtherNetID = data.Unit.NetId
            };
            OnEventWorldNotify(killDragon, data.Killer);
        }

        public static void AnnounceKillWorm(DeathData data)
        {
            var killDragon = new OnSuperMonsterKill()
            {
                //TODO: Figure out all the parameters, their values look random(?).
                OtherNetID = data.Unit.NetId
            };
            OnEventWorldNotify(killDragon, data.Killer);
        }





        public static void AnnounceMinionsSpawn()
        {
            OnEventWorldNotify(new OnMinionsSpawn());
        }

        public static void AnnouceNexusCrystalStart()
        {
            OnEventWorldNotify(new OnNexusCrystalStart());
        }

        public static void AnnounceStartGameMessage(int message, int map = 0)
        {
            IEvent annoucement;
            switch (message)
            {
                case 1:
                    annoucement = new OnStartGameMessage1();
                    break;
                case 2:
                    annoucement = new OnStartGameMessage2();
                    break;
                default:
                    _logger.Warn($"Announcement with Id {message} doesn't exist! Please use numbers between 1 and 5");
                    return;
            }
            (annoucement as ArgsGlobalMessageGeneric)!.MapNumber = map;

            OnEventWorldNotify(annoucement);
        }

        public static void AnnounceVictoryPointThreshold(LaneTurret turret, int index)
        {
            IEvent pointThreshHold;
            switch (index)
            {
                case 1:
                    pointThreshHold = new OnVictoryPointThreshold1();
                    break;
                case 2:
                    pointThreshHold = new OnVictoryPointThreshold2();
                    break;
                case 3:
                    pointThreshHold = new OnVictoryPointThreshold3();
                    break;
                case 4:
                    pointThreshHold = new OnVictoryPointThreshold4();
                    break;
                default:
                    _logger.Warn($"Announcement with Id {index} doesn't exist! Please use numbers between 1 and 4");
                    return;
            }

            OnEventWorldNotify(pointThreshHold, turret);
        }
    }
}
