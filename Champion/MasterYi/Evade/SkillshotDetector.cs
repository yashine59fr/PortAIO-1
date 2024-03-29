﻿#region LICENSE

// Copyright 2014 - 2014 Support
// SkillshotDetector.cs is part of Support.
// Support is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// Support is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with Support. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using LeagueSharp.Common;
using SharpDX;

#endregion

using TargetSelector = PortAIO.TSManager; namespace MasterSharp
{
    internal static class SkillshotDetector
    {
        public delegate void OnDeleteMissileH(Skillshot skillshot, MissileClient missile);

        public delegate void OnDetectSkillshotH(Skillshot skillshot);

        public static List<Skillshot> detectedSkillShots = new List<Skillshot>();

        static SkillshotDetector()
        {
            //Detect when the skillshots are created.
            //Game.OnProcessPacket += GameOnOnGameProcessPacket; // Used only for viktor's Laser :^)
            Obj_AI_Base.OnProcessSpellCast += ObjAiHeroOnOnProcessSpellCast;

            //Detect when projectiles collide.
            GameObject.OnDelete += ObjSpellMissileOnOnDelete;
            GameObject.OnCreate += ObjSpellMissileOnOnCreate;
            //GameObject.OnCreate += GameObject_OnCreate; //TODO: Detect lux R and other large skillshots.
            GameObject.OnDelete += GameObject_OnDelete;
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid || sender.Team == ObjectManager.Player.Team)
            {
                return;
            }

            for (var i = detectedSkillShots.Count - 1; i >= 0; i--)
            {
                var skillshot = detectedSkillShots[i];
                if (skillshot.SpellData.ToggleParticleName != "" &&
                    sender.Name.Contains(skillshot.SpellData.ToggleParticleName))
                {
                    detectedSkillShots.RemoveAt(i);
                }
            }
        }

        private static void ObjSpellMissileOnOnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid || !(sender is MissileClient))
            {
                return; //not sure if needed
            }

            var missile = (MissileClient) sender;

#if DEBUG
            if (missile.SpellCaster is AIHeroClient)
            {
                //Console.WriteLine(
                    //Environment.TickCount + " Projectile Created: " + missile.SData.Name + " distance: " +
                    //missile.StartPosition.LSDistance(missile.EndPosition) + "Radius: " +
                    //missile.SData.CastRadiusSecondary + " Speed: " + missile.SData.MissileSpeed);
            }

#endif


            var unit = missile.SpellCaster;
            if (!unit.IsValid || (unit.Team == ObjectManager.Player.Team))
            {
                return;
            }

            var spellData = SpellDatabase.GetByMissileName(missile.SData.Name);
            if (spellData == null)
            {
                return;
            }
            var missilePosition = missile.Position.LSTo2D();
            var unitPosition = missile.StartPosition.LSTo2D();
            var endPos = missile.EndPosition.LSTo2D();

            //Calculate the real end Point:
            var direction = (endPos - unitPosition).Normalized();
            if (unitPosition.LSDistance(endPos) > spellData.Range || spellData.FixedRange)
            {
                endPos = unitPosition + direction*spellData.Range;
            }

            if (spellData.ExtraRange != -1)
            {
                endPos = endPos +
                         Math.Min(spellData.ExtraRange, spellData.Range - endPos.LSDistance(unitPosition))*direction;
            }

            var castTime = Environment.TickCount - Game.Ping/2 - (spellData.MissileDelayed ? 0 : spellData.Delay) -
                           (int) (1000*missilePosition.LSDistance(unitPosition)/spellData.MissileSpeed);

            //Trigger the skillshot detection callbacks.
            TriggerOnDetectSkillshot(DetectionType.RecvPacket, spellData, castTime, unitPosition, endPos, unit);
        }

        /// <summary>
        ///     Delete the missiles that collide.
        /// </summary>
        private static void ObjSpellMissileOnOnDelete(GameObject sender, EventArgs args)
        {
            if (!(sender is MissileClient))
            {
                return;
            }

            var missile = (MissileClient) sender;

            if (!(missile.SpellCaster is AIHeroClient))
            {
                return;
            }

            var unit = (AIHeroClient) missile.SpellCaster;
            if (!unit.IsValid || (unit.Team == ObjectManager.Player.Team))
            {
                return;
            }

            var spellName = missile.SData.Name;

            if (OnDeleteMissile != null)
            {
                foreach (var skillshot in detectedSkillShots)
                {
                    if (skillshot.SpellData.MissileSpellName == spellName && skillshot.Unit.NetworkId == unit.NetworkId &&
                        (missile.EndPosition.LSTo2D() - missile.StartPosition.LSTo2D()).AngleBetween(skillshot.Direction) <
                        10 && skillshot.SpellData.CanBeRemoved)
                    {
                        OnDeleteMissile(skillshot, missile);
                        break;
                    }
                }
            }

#if DEBUG
            //Console.WriteLine(
                //"Missile deleted: " + missile.SData.Name + " D: " + missile.EndPosition.LSDistance(missile.Position));
#endif

            detectedSkillShots.RemoveAll(
                skillshot =>
                    (skillshot.SpellData.MissileSpellName == spellName ||
                     skillshot.SpellData.ExtraMissileNames.Contains(spellName)) &&
                    (skillshot.Unit.NetworkId == unit.NetworkId &&
                     ((missile.EndPosition.LSTo2D() - missile.StartPosition.LSTo2D()).AngleBetween(skillshot.Direction) < 10) &&
                     skillshot.SpellData.CanBeRemoved || skillshot.SpellData.ForceRemove)); // 
        }

        /// <summary>
        ///     This event is fired after a skillshot is detected.
        /// </summary>
        public static event OnDetectSkillshotH OnDetectSkillshot;

        /// <summary>
        ///     This event is fired after a skillshot missile collides.
        /// </summary>
        public static event OnDeleteMissileH OnDeleteMissile;


        private static void TriggerOnDetectSkillshot(DetectionType detectionType,
            SpellData spellData,
            int startT,
            Vector2 start,
            Vector2 end,
            Obj_AI_Base unit)
        {
            var skillshot = new Skillshot(detectionType, spellData, startT, start, end, unit);

            if (OnDetectSkillshot != null)
            {
                OnDetectSkillshot(skillshot);
            }
        }

        /// <summary>
        ///     Gets triggered when a unit casts a spell and the unit is visible.
        /// </summary>
        private static void ObjAiHeroOnOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.SData.Name == "dravenrdoublecast")
            {
                detectedSkillShots.RemoveAll(
                    s => s.Unit.NetworkId == sender.NetworkId && s.SpellData.SpellName == "DravenRCast");
            }

            if (!sender.IsValid || sender.Team == ObjectManager.Player.Team)
            {
                return;
            }
            //Get the skillshot data.
            var spellData = SpellDatabase.GetByName(args.SData.Name);

            //Skillshot not added in the database.
            if (spellData == null)
            {
                return;
            }

            var startPos = new Vector2();

            if (spellData.FromObject != "")
            {
                foreach (var o in ObjectManager.Get<GameObject>())
                {
                    if (o.Name.Contains(spellData.FromObject))
                    {
                        startPos = o.Position.LSTo2D();
                    }
                }
            }
            else
            {
                startPos = sender.ServerPosition.LSTo2D();
            }

            //For now only zed support.
            if (spellData.FromObjects != null && spellData.FromObjects.Length > 0)
            {
                foreach (var obj in ObjectManager.Get<GameObject>())
                {
                    if (obj.IsEnemy && spellData.FromObjects.Contains(obj.Name))
                    {
                        var start = obj.Position.LSTo2D();
                        var end = start + spellData.Range*(args.End.LSTo2D() - obj.Position.LSTo2D()).Normalized();
                        TriggerOnDetectSkillshot(
                            DetectionType.ProcessSpell, spellData, Environment.TickCount - Game.Ping/2, start, end,
                            sender);
                    }
                }
            }

            if (!startPos.IsValid())
            {
                return;
            }

            var endPos = args.End.LSTo2D();

            if (spellData.SpellName == "LucianQ" && args.Target != null &&
                args.Target.NetworkId == ObjectManager.Player.NetworkId)
            {
                return;
            }

            //Calculate the real end Point:
            var direction = (endPos - startPos).Normalized();
            if (startPos.LSDistance(endPos) > spellData.Range || spellData.FixedRange)
            {
                endPos = startPos + direction*spellData.Range;
            }

            if (spellData.ExtraRange != -1)
            {
                endPos = endPos +
                         Math.Min(spellData.ExtraRange, spellData.Range - endPos.LSDistance(startPos))*direction;
            }


            //Trigger the skillshot detection callbacks.
            TriggerOnDetectSkillshot(
                DetectionType.ProcessSpell, spellData, Environment.TickCount - Game.Ping/2, startPos, endPos, sender);
        }
    }
}