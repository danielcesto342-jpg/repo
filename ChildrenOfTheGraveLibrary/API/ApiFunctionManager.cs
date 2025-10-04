using System;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ChildrenOfTheGraveEnumNetwork;
using ChildrenOfTheGraveEnumNetwork.Enums;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Content;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits.AI;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits.Buildings;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.SpellNS;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.StatsNS;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Inventory;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Logging;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Scripting.CSharp;
using log4net;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.API
{
    /// <summary>
    /// Class housing functions most commonly used by scripts.
    /// </summary>
    public static class ApiFunctionManager
    {
        // Required variables.
        private static ILog _logger = LoggerProvider.GetLogger();
        private static readonly Random rng = new();

        /// <summary>
        /// Converts the given string of hex values into an array of bytes.
        /// </summary>
        /// <param name="hex">String of hex values.</param>
        /// <returns>Array of bytes.</returns>
        public static byte[] StringToByteArray(string hex)
        {
            hex = hex.Replace(" ", string.Empty);
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        /// <summary>
        /// Sets the visibility of the specified GameObject.
        /// </summary>
        /// <param name="gameObject">GameObject to set.</param>
        /// <param name="visibility">Whether or not the GameObject should be visible.</param>
        public static void SetGameObjectVisibility(GameObject gameObject, bool visibility)
        {
            var teams = GetTeams();
            foreach (var id in teams)
            {
                gameObject.SetVisibleByTeam(id, visibility);
            }
        }

        /// <summary>
        /// Gets the possible teams.
        /// </summary>
        /// <returns>Usually BLUE/PURPLE/NEUTRAL.</returns>
        public static List<TeamId> GetTeams()
        {
            return Game.ObjectManager.Teams;
        }

        internal static int ConvertAPISlot(SpellSlotType slotType, int slot)
        {
            if ((slotType == SpellSlotType.SpellSlots && (slot < 0 || slot > 3))
                || (slotType == SpellSlotType.InventorySlots && (slot < 0 || slot > 6))
                || (slotType == SpellSlotType.ExtraSlots && (slot < 0 || slot > 15)))
            {
                return -1;
            }

            if (slotType is SpellSlotType.SummonerSpellSlots
                         or SpellSlotType.InventorySlots
                         or SpellSlotType.TempItemSlot
                         or SpellSlotType.ExtraSlots)
            {
                slot += (int)slotType;
            }

            return slot;
        }

        internal static int ConvertAPISlot(SpellbookType spellbookType, SpellSlotType slotType, int slot)
        {
            if (spellbookType == SpellbookType.SPELLBOOK_UNKNOWN
                || (spellbookType == SpellbookType.SPELLBOOK_SUMMONER && (slotType != SpellSlotType.SummonerSpellSlots))
                || (spellbookType == SpellbookType.SPELLBOOK_CHAMPION
                    && ((slotType == SpellSlotType.SpellSlots && (slot < 0 || slot > 3))
                        || (slotType == SpellSlotType.InventorySlots && (slot < 0 || slot > 6))
                        || (slotType == SpellSlotType.ExtraSlots && (slot < 0 || slot > 15)))))
            {
                return -1;
            }

            if ((spellbookType == SpellbookType.SPELLBOOK_CHAMPION &&
                slotType is SpellSlotType.InventorySlots
                         or SpellSlotType.TempItemSlot
                         or SpellSlotType.ExtraSlots) ||
                (spellbookType == SpellbookType.SPELLBOOK_SUMMONER &&
                slotType is SpellSlotType.SummonerSpellSlots))
            {
                slot += (int)slotType;
            }

            return slot;
        }

        /// <summary>
        /// Teleports an AI unit to the specified coordinates.
        /// Instant.
        /// </summary>
        /// <param name="unit">Unit to be moved</param>
        /// <param name="position">The end position</param>
        /// <param name="clearMovement">Clear the unit pathfinding movement</param>
        public static void TeleportTo(AttackableUnit unit, Vector2 position, bool clearMovement = true)
        {
            if (unit.MovementParameters != null)
            {
                CancelDash(unit);
            }
            unit.TeleportTo(position, !clearMovement);
        }





        /// <summary>
        /// Adds a named buff with the given duration, stacks, and origin spell to a unit.
        /// From = owner of the spell (usually caster).
        /// </summary>
        /// <param name="buffName">Internally named buff to add.</param>
        /// <param name="duration">Time in seconds the buff should last.</param>
        /// <param name="stacks">Stacks of the buff to add.</param>
        /// <param name="originspell">Spell which called this function.</param>
        /// <param name="onto">Target of the buff.</param>
        /// <param name="from">Owner of the buff.</param>
        /// <param name="parent">Custom hash name Source</param>
        /// <returns>New buff instance.</returns>
        public static Buff AddBuff
        (
            string buffName,
            float duration,
            byte stacks,
            Spell originspell,
            AttackableUnit onto,
            AttackableUnit from,

            IEventSource parent = null
        )
        {
            onto.Buffs.Add(buffName, duration, stacks, originspell, onto, from as ObjAIBase, parent);

            // For compatibility with the legacy LS-Scripts
            return onto.Buffs.GetStacks(buffName, from as ObjAIBase)?.Last();
        }




        /// <summary>
        /// Removes all buffs of the given name from the specified AI unit and runs OnDeactivate callback for the buff's script.
        /// Even if the buff's BuffAddType is STACKS_AND_OVERLAPS, it will still remove all buff instances.
        /// </summary>
        /// <param name="target">AI unit to check.</param>
        /// <param name="buff">Buff name to remove.</param>
        public static void RemoveBuff(AttackableUnit target, string buff)
        {
            target.Buffs.RemoveAllStacks(buff, null);
        }






        /// <summary>
        /// Creates a new particle tied to the bindObj, and optionaly End position
        /// </summary>
        /// <param name="caster">The Pacakge Hash Owner, used on the client.</param>
        /// <param name="name">Internal name of the particle.</param>
        /// <param name="bindObj">GameObject that the particle should bind to, usually the start point.</param>
        /// <param name="end">Position to end at.</param>
        /// <param name="lifetime">Time in seconds the particle should last.</param>
        /// <param name="scale">Particle strech scale.</param>
        /// <param name="direction">3D direction the particle should face.</param>
        /// <param name="bindBone">Bone on the bindObj the particle should be attached to.</param>
        /// <param name="enemyParticle">Internal name of the particle shown to the enemy team</param>
        /// <param name="followGroundTilt">Whether or not the particle should be titled along the ground towards its end position.</param>
        /// <param name="flags">Flags which determine how the particle behaves. Refer to FXFlags enum.</param>
        /// <param name="forceTeam">Particle team if caster is null</param>
        /// <param name="visibilityOwner">The only player allowed to see the particle</param>
        public static Particle AddParticleBind
        (
            GameObject caster,
            string name,
            GameObject bindObj,

            Vector2 end = default,
            float lifetime = 1.0f,
            float scale = 1.0f,
            Vector3 direction = default,
            string bindBone = "",
            string enemyParticle = null,
            bool followGroundTilt = false,
            FXFlags flags = FXFlags.BindDirection,
            TeamId forceTeam = TeamId.TEAM_UNKNOWN,
            Champion visibilityOwner = null
        )
        {
            return new Particle(name, caster, bindObj?.Position ?? default, bindObj, bindBone, end, null, "",
                lifetime, scale, direction, enemyParticle, followGroundTilt, flags, forceTeam, visibilityOwner);
        }

        /// <summary>
        /// Creates a new particle tied to the targetObj, and optionaly Start position
        /// </summary>
        /// <param name="caster">GameObject that caused this particle to spawn.</param>
        /// <param name="name">Internal name of the particle.</param>
        /// <param name="targetObj">GameObject that the particle should bind to, usually the end point.</param>
        /// <param name="start">Position to spawn at.</param>
        /// <param name="lifetime">Time in seconds the particle should last.</param>
        /// <param name="scale">Particle strech scale.</param>
        /// <param name="direction">3D direction the particle should face.</param>
        /// <param name="targetBone">Bone on the targetObj the particle should be attached to.</param>
        /// <param name="enemyParticle">Internal name of the particle shown to the enemy team</param>
        /// <param name="followGroundTilt">Whether or not the particle should be titled along the ground towards its end position.</param>
        /// <param name="flags">Flags which determine how the particle behaves. Refer to FXFlags enum.</param>
        /// <param name="forceTeam">Particle team if caster is null</param>
        /// <param name="visibilityOwner">The only player allowed to see the particle</param>
        public static Particle AddParticleTarget
        (
            GameObject caster,
            string name,
            GameObject targetObj,

            Vector2 start = default,
            float lifetime = 1.0f,
            float scale = 1.0f,
            Vector3 direction = default,
            string targetBone = "",
            string enemyParticle = null,
            bool followGroundTilt = false,
            FXFlags flags = FXFlags.BindDirection,
            TeamId forceTeam = TeamId.TEAM_UNKNOWN,
            Champion visibilityOwner = null
        )
        {
            return new Particle(name, caster, start, null, "", targetObj?.Position ?? default, targetObj, targetBone,
                lifetime, scale, direction, enemyParticle, followGroundTilt, flags, forceTeam, visibilityOwner);
        }

        /// <summary>
        /// Creates a new particle liked from the bindObj to the targetObj
        /// </summary>
        /// <param name="caster">GameObject that caused this particle to spawn.</param>
        /// <param name="name">Internal name of the particle.</param>
        /// <param name="bindObj">GameObject that the particle should bind to, usually the start point.</param>
        /// <param name="targetObj">GameObject that the particle should bind to, usually the end point.</param>
        /// <param name="lifetime">Time in seconds the particle should last.</param>
        /// <param name="scale">Particle strech scale.</param>
        /// <param name="direction">3D direction the particle should face.</param>
        /// <param name="bindBone">Bone on the bindObj the particle should be attached to.</param>
        /// <param name="targetBone">Bone on the targetObj the particle should be attached to.</param>
        /// <param name="enemyParticle">Internal name of the particle shown to the enemy team</param>
        /// <param name="followGroundTilt">Whether or not the particle should be titled along the ground towards its end position.</param>
        /// <param name="flags">Flags which determine how the particle behaves. Refer to FXFlags enum.</param>
        /// <param name="forceTeam">Particle team if caster is null</param>
        /// <param name="visibilityOwner">The only player allowed to see the particle</param>
        public static Particle AddParticleLink
        (
            GameObject caster,
            string name,
            GameObject bindObj,
            GameObject targetObj,

            float lifetime = 1.0f,
            float scale = 1.0f,
            Vector3 direction = default,
            string bindBone = "",
            string targetBone = "",
            string enemyParticle = null,
            bool followGroundTilt = false,
            FXFlags flags = FXFlags.BindDirection,
            TeamId forceTeam = TeamId.TEAM_UNKNOWN,
            Champion visibilityOwner = null
        )
        {
            return new Particle(name, caster, bindObj?.Position ?? default, bindObj, bindBone, targetObj?.Position ?? default, targetObj, targetBone,
                lifetime, scale, direction, enemyParticle, followGroundTilt, flags, forceTeam, visibilityOwner);
        }




        /// <summary>
        /// Creates a stationary perception bubble at the given location.
        /// </summary>
        /// <param name="position">Position to spawn the perception bubble at.</param>
        /// <param name="radius">Size of the perception bubble.</param>
        /// <param name="duration">Number of seconds the perception bubble should exist.</param>
        /// <param name="team">Team the perception bubble should be owned by.</param>
        /// <param name="revealStealthed">Whether or not the perception bubble should reveal stealthed units while they are in range.</param>
        /// <param name="revealSpecificUnitOnly">Specific unit to reveal. Perception bubble will not reveal any other units when used. *NOTE* Currently does nothing.</param>
        /// <param name="collisionArea">Area around the perception bubble where units are not allowed to move into.</param>
        /// <param name="regionType">Unkown data. Use Default</param>
        /// <returns>New Region instance.</returns>
        public static Region AddPosPerceptionBubble
        (
            Vector2 position,
            float radius,
            float duration,
            TeamId team = TeamId.TEAM_NEUTRAL,
            bool revealStealthed = false,
            AttackableUnit revealSpecificUnitOnly = null,
            float collisionArea = 0f,
            RegionType regionType = RegionType.Default
        )
        {
            return new Region
            (
                team, position, regionType,
                visionTarget: revealSpecificUnitOnly,
                visionRadius: radius,
                revealStealth: revealStealthed,
                collisionRadius: collisionArea,
                lifetime: duration
            );
        }

        /// <summary>
        /// Creates a perception bubble which is attached to a specific unit.
        /// </summary>
        /// <param name="target">Unit to attach the perception bubble to.</param>
        /// <param name="radius">Size of the perception bubble.</param>
        /// <param name="duration">Number of seconds the perception bubble should exist.</param>
        /// <param name="team">Team the perception bubble should be owned by.</param>
        /// <param name="revealStealthed">Whether or not the perception bubble should reveal stealthed units while they are in range.</param>
        /// <param name="revealSpecificUnitOnly">Specific unit to reveal. Perception bubble will not reveal any other units when used. *NOTE* Currently does nothing.</param>
        /// <param name="collisionArea">Area around the perception bubble where units are not allowed to move into.</param>
        /// <param name="regionType">Unkown data. Use Default</param>
        /// <returns>New Region instance.</returns>
        public static Region AddUnitPerceptionBubble
        (
            AttackableUnit target,
            float radius,
            float duration,
            TeamId team = TeamId.TEAM_NEUTRAL,
            bool revealStealthed = false,
            AttackableUnit revealSpecificUnitOnly = null,
            float collisionArea = 0f,
            RegionType regionType = RegionType.Default
        )
        {
            return new Region
            (
                team, target.Position, regionType,
                collisionUnit: target,
                visionTarget: revealSpecificUnitOnly,
                visionRadius: radius,
                revealStealth: revealStealthed,
                collisionRadius: collisionArea,
                lifetime: duration
            );
        }





        /// <summary>
        /// Acquires all units in range filtered by filterFlags
        /// </summary>
        /// <param name="targetPos">Origin of the range to check.</param>
        /// <param name="range">Range to check from the target position.</param>
        /// <param name="sourceTeam">Source team to validate the targets</param>
        /// <param name="filterFlags">Flags to filters units types and teams</param>
        /// <returns>List of AttackableUnits.</returns>
        public static List<AttackableUnit> FilterUnitsInRange
        (
            AttackableUnit self,
            Vector2 targetPos,
            float range,
            SpellDataFlags filterFlags
        )
        {
            var units = EnumerateFilterUnitsInRange(self, targetPos, range, filterFlags);
            var unitList = units.ToList();
            unitList.Sort((a, b) => Vector2.DistanceSquared(a.Position, targetPos).CompareTo(Vector2.DistanceSquared(b.Position, targetPos)));
            return unitList;
        }

        /// <summary>
        /// Acquires all units in range filtered by filterFlags
        /// </summary>
        /// <param name="targetPos">Origin of the range to check.</param>
        /// <param name="range">Range to check from the target position.</param>
        /// <param name="sourceTeam">Source team to validate the targets</param>
        /// <param name="filterFlags">Flags to filters units types and teams</param>
        /// <returns>List of AttackableUnits.</returns>
        public static IEnumerable<AttackableUnit> EnumerateFilterUnitsInRange
        (
            AttackableUnit self,
            Vector2 targetPos,
            float range,
            SpellDataFlags filterFlags
        )
        {
            foreach (var obj in Game.Map.CollisionHandler.GetNearestObjects(new Circle(targetPos, range)))
            {
                if (obj is AttackableUnit u && IsValidTarget(self, u, filterFlags))
                {
                    yield return u;
                }
            }
        }

        /// <summary>
        /// Acquires all dead or alive AttackableUnits within the specified range of a target position.
        /// </summary>
        /// <param name="targetPos">Origin of the range to check.</param>
        /// <param name="range">Range to check from the target position.</param>
        /// <param name="isAlive">Whether or not to return alive AttackableUnits.</param>
        /// <returns>List of AttackableUnits.</returns>
        public static IEnumerable<AttackableUnit> EnumerateUnitsInRange
        (
            Vector2 targetPos,
            float range,
            bool isAlive = true
        )
        {
            foreach (var obj in Game.Map.CollisionHandler.GetNearestObjects(new Circle(targetPos, range)))
            {
                if (obj is AttackableUnit u && (!isAlive || !u.Stats.IsDead))
                {
                    yield return u;
                }
            }
        }

        /// <summary>
        /// Acquires all dead or alive AttackableUnits within the specified range of a unit.
        /// </summary>
        /// <param name="unit">Unit to check around.</param>
        /// <param name="range">Range to check from the target.</param>
        /// <param name="isAlive">Whether or not to return alive AttackableUnits.</param>
        /// <param name="selfIncluded">Include the unit itself to the list</param>
        /// <param name="excludeTeam">Exclude all units from this Team</param>
        /// <returns>List of AttackableUnits.</returns>
        public static List<AttackableUnit> GetUnitsInRange
        (
            AttackableUnit unit,
            float range,
            bool isAlive,

            bool selfIncluded = false,
            TeamId excludeTeam = TeamId.TEAM_UNKNOWN
        )
        {
            List<AttackableUnit> toReturn = GetUnitsInRange(unit.Position, range, isAlive, excludeTeam);

            if (!selfIncluded)
            {
                toReturn.Remove(unit);
            }

            return toReturn;
        }

        /// <summary>
        /// Acquires all dead or alive AttackableUnits within the specified range of a target position.
        /// </summary>
        /// <param name="targetPos">Origin of the range to check.</param>
        /// <param name="range">Range to check from the target position.</param>
        /// <param name="isAlive">Whether or not to return alive AttackableUnits.</param>
        /// <param name="excludeTeam">Exclude all units from this Team</param>
        /// <returns>List of AttackableUnits.</returns>
        public static List<AttackableUnit> GetUnitsInRange
        (
            Vector2 targetPos,
            float range,

            bool isAlive = true,
            TeamId excludeTeam = TeamId.TEAM_UNKNOWN
        )
        {
            var units = EnumerateUnitsInRange(targetPos, range, isAlive);
            var filteredList = new List<AttackableUnit>();

            foreach (var unit in units)
            {
                if (excludeTeam == TeamId.TEAM_UNKNOWN || unit.Team != excludeTeam)
                {
                    filteredList.Add(unit);
                }
            }

            filteredList.Sort((a, b) => Vector2.DistanceSquared(a.Position, targetPos).CompareTo(Vector2.DistanceSquared(b.Position, targetPos)));
            return filteredList;
        }

        /// <summary>
        /// Acquires the closest alive or dead AttackableUnit within the specified range of a target position.
        /// </summary>
        /// <param name="targetPos">Origin of the range to check.</param>
        /// <param name="range">Range to check from the target position.</param>
        /// <param name="isAlive">Whether or not to return alive AttackableUnits.</param>
        /// <param name="excludedTeam">Exclude all units from this Team</param>
        /// <returns>Closest AttackableUnit.</returns>
        public static AttackableUnit? GetClosestUnitInRange
        (
            Vector2 targetPos,
            float range,

            bool isAlive = true,
            TeamId excludedTeam = TeamId.TEAM_UNKNOWN
        )
        {
            var units = GetUnitsInRange(targetPos, range, isAlive);
            units.RemoveAll(x => x.Team == excludedTeam);

            if (units.Count == 0) return null;

            AttackableUnit? closest = null;
            float closestDistSqr = float.MaxValue;

            foreach (var unit in units)
            {
                float distSqr = Vector2.DistanceSquared(targetPos, unit.Position);
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    closest = unit;
                }
            }

            return closest;
        }




        /// <summary>
        /// Returns a new list of all players in the game.
        /// Players are designated as clients, this includes bot champions.
        /// Currently only a single champion is designated to each player.
        /// </summary>
        /// <returns></returns>
        public static List<Champion> GetAllPlayers()
        {
            var toreturn = new List<Champion>();
            foreach (var player in Game.PlayerManager.GetPlayers(true))
            {
                toreturn.Add(player.Champion);
            }
            return toreturn;
        }





        /// <summary>
        /// Instantly cancels any dashes the specified unit is performing.
        /// </summary>
        /// <param name="unit">Unit to stop dashing.</param>
        public static void CancelDash(AttackableUnit unit)
        {
            // Allow the user to move the champion
            unit.SetDashingState(false);
        }


        /// <summary>
        /// Cast a Spell to a given positon or to a given target list
        /// </summary>
        /// <param name="spell">The spell to cast</param>
        /// <param name="pos">Spell target position</param>
        /// <param name="endPos">Spell target end position for line spells</param>
        /// <param name="fireWithoutCasting">Does not play animations and cast delays</param>
        /// <param name="overrideCastPos">Override the spell start position to fire</param>
        /// <param name="targets">Target for target spells</param>
        /// <param name="isForceCastingOrChanneling">Some spells need to be forced</param>
        /// <param name="overrideForceLevel">Set the spell level in case of ExtraSlots</param>
        /// <param name="updateAutoAttackTimer">The spell work as an autoattack</param>
        /// <param name="useAutoAttackSpell">The spell should use the current autoattack spell</param>
        public static void SpellCast
        (
            Spell spell,
            Vector2 pos,
            Vector2 endPos,
            bool fireWithoutCasting,
            Vector2 overrideCastPos = default,
            List<CastTarget> targets = null,
            bool isForceCastingOrChanneling = false,
            int overrideForceLevel = -1,
            bool updateAutoAttackTimer = false,
            bool useAutoAttackSpell = false
        )
        {
            var target = targets == null || targets.Count == 0 ? null : targets[0].Unit;
            spell.TryCast(
                target, pos.ToVector3(0), endPos.ToVector3(0),
                overrideForceLevel,
                false,
                fireWithoutCasting,
                useAutoAttackSpell,
                isForceCastingOrChanneling,
                updateAutoAttackTimer,
                overrideCastPos != default,
                overrideCastPos.ToVector3(0)
            );
        }

        /// <summary>
        /// Cast a Spell to a given target unit
        /// </summary>
        /// <param name="spell">The spell to cast</param>
        /// <param name="target">Spell target unit</param>
        /// <param name="fireWithoutCasting">Does not play animations and cast delays</param>
        /// <param name="overrideCastPos">Override the spell start position to fire</param>
        /// <param name="isForceCastingOrChanneling">Some spells need to be forced</param>
        /// <param name="overrideForceLevel">Set the spell level in case of ExtraSlots</param>
        /// <param name="updateAutoAttackTimer">The spell should cause interference on autoattack timer</param>
        /// <param name="useAutoAttackSpell">The spell should use the current autoattack spell</param>
        public static void SpellCast
        (
            Spell spell,
            AttackableUnit target,
            bool fireWithoutCasting,
            Vector2 overrideCastPos = default,
            bool isForceCastingOrChanneling = false,
            int overrideForceLevel = -1,
            bool updateAutoAttackTimer = false,
            bool useAutoAttackSpell = false
        )
        {
            CastTarget castTarget = new(target, CastTarget.GetHitResult(target, useAutoAttackSpell, spell.Caster.IsNextAutoCrit));
            if (target != null)
            {
                var targetPosition = Extensions.GetClosestCircleEdgePoint(spell.Caster.Position, target.Position, target.CollisionRadius);

                SpellCast(spell, target.Position, target.Position, fireWithoutCasting, overrideCastPos, new List<CastTarget> { castTarget }, isForceCastingOrChanneling, overrideForceLevel, updateAutoAttackTimer, useAutoAttackSpell);

            }
        }






        public static ItemData GetItemData(int Id)
        {
            return ContentManager.GetItemData(Id);
        }




        public static Pet AddPet
        (
            Champion owner,
            Spell spell,
            Vector2 position,
            string name,
            string model,
            string buffName,
            float lifeTime,
            bool cloneInventory = true,
            bool showMinimapIfClone = true,
            bool disallowPlayerControl = false,
            bool doFade = false,
            bool isClone = true,
            Stats stats = null,
            string aiScript = "Pet.lua"

        )
        {


            var p = new Pet(
                owner,
                spell,
                position,
                name,
                model,
                buffName,
                lifeTime,
                stats,
                cloneInventory,
                showMinimapIfClone,
                disallowPlayerControl,
                doFade,
                isClone,
                aiScript);


            /*  p.SetStatus(StatusFlags.NoRender
                  | StatusFlags.ForceRenderParticles
                  | StatusFlags.Ghosted
                  | StatusFlags.SuppressCallForHelp
                  | StatusFlags.IgnoreCallForHelp
                  | StatusFlags.CallForHelpSuppressor
                  | StatusFlags.Invulnerable
                  , false);

              p.SetStatus(StatusFlags.Stunned
                  | StatusFlags.Rooted
                  | StatusFlags.Silenced
                  | StatusFlags.SuppressCallForHelp
                  | StatusFlags.Invulnerable
                  | StatusFlags.MagicImmune

                  , false);
              p.SetStatus(StatusFlags.Targetable, true);

              */

            //    p.HasSkins = false;
            //  p.SetPosition(position);
            //  p.SetTeam(owner.Team);
            //p.IsAffectedByFoW = true;
            // p.VisibilityOwner
            //  p.LevelUp(1, true);
            //  p.VisionRadius = 1;
            //    p.Stats.PerceptionRange.FlatBonus = owner.Stats.PerceptionRange.Total;
            // p.IsWard = false;

            // var pminion = (p as Minion);

            Game.ObjectManager.AddObject(p);

            return p;

        }

        public static Pet AddPetClone
        (
            Champion owner,
            Spell spell,
            ObjAIBase cloned,
            Vector2 position,
            string buffName,
            float lifeTime,
            bool cloneInventory = true,
            bool showMinimapIfClone = true,
            bool disallowPlayerControl = false,
            bool doFade = false,
            Stats stats = null,
            string aiScript = "Pet.lua"
        )
        {
            var p = new Pet(owner, spell, cloned, position, buffName, lifeTime, cloned.Stats, cloneInventory, showMinimapIfClone, disallowPlayerControl, doFade, aiScript);

            Game.ObjectManager.AddObject(p);

            return p;

        }



        public static void Heal(AttackableUnit target, float ammount, AttackableUnit source = null, AddHealthType type = AddHealthType.HEAL, IEventSource sourceScript = null)
        {
            target.TakeHeal(new HealData(target, ammount, source, type, sourceScript));
        }

        public static void SetFullHealth(AttackableUnit unit)
        {
            Heal(unit, unit.Stats.HealthPoints.Total, type: AddHealthType.RGEN);
        }





        /// <summary>
        /// Increase the Spell ManaCost, preventing usage on lower mana and spending the ammout.
        /// </summary>
        /// <param name="spell">The spell to change</param>
        /// <param name="modifier">Additional value.</param>
        public static void IncreaseSpellManaCost(Spell spell, float modifier)
        {
            spell.IncreaseManaCost(modifier);
        }

        /// <summary>
        /// Decrease the Spell ManaCost, preventing usage on lower mana and spending the ammout.
        /// </summary>
        /// <param name="spell">The spell to change</param>
        /// <param name="modifier">Decrease value.</param>
        public static void DecreaseSpellManaCost(Spell spell, float modifier)
        {
            spell.IncreaseManaCost(-modifier);
        }

        /// <summary>
        /// Increase the Spell ManaCost percentage, preventing usage on lower mana and spending the ammout.
        /// </summary>
        /// <param name="spell">The spell to change</param>
        /// <param name="percent">Value ranged by 100</param>
        public static void IncreaseSpellManaCostMultiplier(Spell spell, float percent)
        {
            spell.IncreaseMultiplicativeManaCost(percent / 100);
        }

        /// <summary>
        /// Decrease the Spell ManaCost percentage, preventing usage on lower mana and spending the ammout.
        /// </summary>
        /// <param name="spell">The spell to change</param>
        /// <param name="percent">Value ranged by 100</param>
        public static void DecreaseSpellManaCostMultiplier(Spell spell, float percent)
        {
            spell.IncreaseMultiplicativeManaCost(-percent / 100);
        }




        /// <summary>
        /// Valid if the target is an enabled target on the flags
        /// </summary>
        /// <param name="target">The target unit</param>
        /// <param name="sourceTeam">Source team to validate the target</param>
        /// <param name="useFlags">Spell Flags to filter</param>
        public static bool IsValidTarget(AttackableUnit self, AttackableUnit target, SpellDataFlags useFlags)
        {

            return

                (!target.Model.Contains("Shrine")
                ) && (

               ((useFlags & SpellDataFlags.AlwaysSelf) != 0 && target == self) //TODO: Verify
            || ((
                   ((useFlags & SpellDataFlags.AffectMinions) != 0 && target is Minion) //TODO: Verify
                || ((useFlags & SpellDataFlags.AffectHeroes) != 0 && (target is Champion || (target is Pet p1 && p1.IsClone))
               // || ((useFlags & SpellDataFlags.AffectMinions & SpellDataFlags.AffectTurrets) != 0 && (target is Minion && (target as Minion).MinionFlags == MinionFlags.IsTower) || (target is BaseTurret || target is ObjBuilding || target is Inhibitor) && target is not Shop)
               )

                || ((useFlags & SpellDataFlags.AffectWards) != 0 && target is Minion m && m.IsWard)
                || ((useFlags & SpellDataFlags.AffectTurrets) != 0 && (target is BaseTurret || target is ObjBuilding || target is Inhibitor))
                || ((useFlags & SpellDataFlags.AffectBuildings) != 0 && target is ObjBuilding)
                || ((useFlags & SpellDataFlags.AffectBarrackOnly) != 0 && target is Inhibitor) //TODO: Verify
                                                                                               // It is assumed that the monsters are always in the neutral team
                || ((useFlags & SpellDataFlags.AffectNeutral) != 0 && target is NeutralMinion) //TODO: Verify
                || ((useFlags & SpellDataFlags.AffectUseable) != 0 && target.CharData.IsUseable)

            )
            && (
                   ((useFlags & SpellDataFlags.AffectFriends) != 0 && target.Team == self.Team)
                || ((useFlags & SpellDataFlags.AffectEnemies) != 0 && target.Team != self.Team && target.Team != TeamId.TEAM_NEUTRAL)
                || ((useFlags & SpellDataFlags.AffectNeutral) != 0 && target.Team == TeamId.TEAM_NEUTRAL)
            )
            && !(
                   ((useFlags & SpellDataFlags.NotAffectSelf) != 0 && target == self)
                || ((useFlags & SpellDataFlags.AffectNotPet) != 0 && target is Pet) //TODO: Verify
                || ((useFlags & SpellDataFlags.IgnoreLaneMinion) != 0 && target is LaneMinion)
                || ((useFlags & SpellDataFlags.IgnoreAllyMinion) != 0 && target.Team == self.Team && target is Minion)
                || ((useFlags & SpellDataFlags.IgnoreEnemyMinion) != 0 && target.Team != self.Team && target is Minion)
                || ((useFlags & SpellDataFlags.IgnoreClones) != 0 && target is Pet p2 && p2.IsClone)
                || ((useFlags & SpellDataFlags.NotAffectZombie) != 0 && target.Stats.IsZombie)
            )
            && ((useFlags & SpellDataFlags.AffectDead) != 0 || !target.Stats.IsDead)
            && ((useFlags & SpellDataFlags.AffectUntargetable) != 0 || (target.Status & StatusFlags.Targetable) != 0))

            );
        }


        /// <summary>
        /// Creates a new instance of a GameScriptTimer with the specified arguments.
        /// </summary>
        /// <param name="duration">Time till the timer ends.</param>
        /// <param name="callback">Action to perform when the timer ends.</param>
        /// <returns>New GameScriptTimer instance.</returns>
        [Obsolete]
        public static GameScriptTimer CreateTimer(float duration, Action callback, bool executeImmediately = true, bool repeat = false)
        {
            var newTimer = new GameScriptTimer(duration, callback, executeImmediately, !repeat);
            Game.AddGameScriptTimer(newTimer);

            return newTimer;
        }
    }
}
