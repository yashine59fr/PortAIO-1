﻿// Copyright 2014 - 2015 Esk0r
// SpellData.cs is part of Evade.
// 
// Evade is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Evade is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Evade. If not, see <http://www.gnu.org/licenses/>.

using ClipperLib;
using Color = System.Drawing.Color;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK;
using EloBuddy;
using Font = SharpDX.Direct3D9.Font;
using LeagueSharp.Common.Data;
using LeagueSharp.Common;
using SharpDX.Direct3D9;
using SharpDX;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Security.AccessControl;
using System;
using System.Speech.Synthesis;
using TargetSelector = PortAIO.TSManager; namespace VayneHunter_Reborn.External.Evade
{
    public class SpellData
    {
        public bool AddHitbox;
        public bool CanBeRemoved = false;
        public bool Centered;
        public string ChampionName;
        public CollisionObjectTypes[] CollisionObjects = { };
        public int DangerValue;
        public int Delay;
        public bool DisableFowDetection = false;
        public bool DontAddExtraDuration;
        public bool DontCross = false;
        public bool DontRemove = false;
        public int ExtraDuration;
        public string[] ExtraMissileNames = { };
        public int ExtraRange = -1;
        public string[] ExtraSpellNames = { };
        public bool FixedRange;
        public bool ForceRemove = false;
        public string FromObject = "";
        public string[] FromObjects = { };
        public int Id = -1;
        public bool Invert;
        public bool IsDangerous = false;
        public int MissileAccel = 0;
        public bool MissileDelayed;
        public bool MissileFollowsUnit;
        public int MissileMaxSpeed;
        public int MissileMinSpeed;
        public int MissileSpeed;
        public string MissileSpellName = "";
        public float MultipleAngle;
        public int MultipleNumber = -1;
        public int RingRadius;
        public SpellSlot Slot;
        public string SpellName;
        public string ToggleParticleName = "";
        public SkillShotType Type;
        private int _radius;
        private int _range;

        public SpellData()
        {
        }

        public SpellData(string championName,
            string spellName,
            SpellSlot slot,
            SkillShotType type,
            int delay,
            int range,
            int radius,
            int missileSpeed,
            bool addHitbox,
            bool fixedRange,
            int defaultDangerValue)
        {
            ChampionName = championName;
            SpellName = spellName;
            Slot = slot;
            Type = type;
            Delay = delay;
            Range = range;
            _radius = radius;
            MissileSpeed = missileSpeed;
            AddHitbox = addHitbox;
            FixedRange = fixedRange;
            DangerValue = defaultDangerValue;
        }

        public string MenuItemName
        {
            get { return ChampionName + " - " + SpellName; }
        }

        public int Radius
        {
            get
            {
                return (!AddHitbox)
                    ? _radius
                    : _radius + (int)ObjectManager.Player.BoundingRadius;
            }
            set { _radius = value; }
        }

        public int RawRadius
        {
            get { return _radius; }
        }

        public int RawRange
        {
            get { return _range; }
        }

        public int Range
        {
            get { return _range; }
            set { _range = value; }
        }
    }
}
