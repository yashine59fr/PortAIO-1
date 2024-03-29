﻿using System;
using System.Collections.Generic;
using System.Drawing;
using DZAwarenessAIO.Utility.Logs;
using DZAwarenessAIO.Utility.MenuUtility;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy;
using PortAIO.Properties;

using TargetSelector = PortAIO.TSManager; namespace DZAwarenessAIO.Modules.WardTracker
{
    /// <summary>
    /// The WardTracker Variables class
    /// </summary>
    class WardTrackerVariables
    {
        /// <summary>
        /// The ward durations
        /// </summary>
        public static Dictionary<WardType, float> wardDurations = new Dictionary<WardType, float>()
        {
            { WardType.Green, 60 * 3 * 1000},
            { WardType.Trinket, 60 * 1000},
            { WardType.TrinketUpgrade, 60 * 3 * 1000},
            { WardType.Pink, float.MaxValue },
            { WardType.TeemoShroom, 60 * 10 * 1000},
            { WardType.ShacoBox, 60 * 1 * 1000}
        };


        /// <summary>
        /// The wards wrappers containing a list of wards.
        /// </summary>
        public static List<WardTypeWrapper> wrapperTypes = new List<WardTypeWrapper>
        {
            new WardTypeWrapper
            {
                ObjectName = "YellowTrinket",
                SpellName = "TrinketTotemLvl1",
                WardType = WardType.Trinket,
                WardVisionRange = 1100
            },
            new WardTypeWrapper
            {
                ObjectName = "YellowTrinketUpgrade",
                SpellName = "TrinketTotemLvl2",
                WardType = WardType.TrinketUpgrade,
                WardVisionRange = 1100
            },
            new WardTypeWrapper
            {
                ObjectName = "SightWard",
                SpellName = "TrinketTotemLvl3",
                WardType = WardType.Green,
                WardVisionRange = 1100
            },
            new WardTypeWrapper
            {
                ObjectName = "SightWard",
                SpellName = "SightWard",
                WardType = WardType.Green,
                WardVisionRange = 1100
            },
            new WardTypeWrapper
            {
                ObjectName = "SightWard",
                SpellName = "ItemGhostWard",
                WardType = WardType.Green,
                WardVisionRange = 1100
            },
            //Pink Wards
            new WardTypeWrapper
            {
                ObjectName = "VisionWard",
                SpellName = "VisionWard",
                WardType = WardType.Pink,
                WardVisionRange = 1100
            },

            //Traps
            new WardTypeWrapper
            {
                ObjectName = "TeemoMushroom",
                SpellName = "BantamTrap",
                WardType = WardType.TeemoShroom,
                WardVisionRange = 212
            },
            new WardTypeWrapper
            {
                ObjectName = "ShacoBox",
                SpellName = "JackInTheBox",
                WardType = WardType.ShacoBox,
                WardVisionRange = 212
            },

        };

        /// <summary>
        /// The detected wards list
        /// </summary>
        public static List<Ward> detectedWards = new List<Ward>();

    }

    /// <summary>
    /// The Ward Class
    /// </summary>
    class Ward
    {
        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the start tick.
        /// </summary>
        /// <value>
        /// The start tick.
        /// </value>
        public float startTick { get; set; }

        /// <summary>
        /// Gets or sets the ward type wrapper.
        /// </summary>
        /// <value>
        /// The ward type w.
        /// </value>
        public WardTypeWrapper WardTypeW { get; set; }

        /// <summary>
        /// Gets or sets the text render object.
        /// </summary>
        /// <value>
        /// The text object.
        /// </value>
        public Render.Text TextObject { get; set; }

        /// <summary>
        /// Gets or sets the minimap sprite object.
        /// </summary>
        /// <value>
        /// The minimap sprite object.
        /// </value>
        public Render.Sprite MinimapSpriteObject { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ward"/> class.
        /// </summary>
        /// <param name="wrapper">The wrapper.</param>
        public Ward(WardTypeWrapper wrapper)
        {
            WardTypeW = wrapper;
            CreateRenderObjects();
        }

        /// <summary>
        /// Creates the render objects.
        /// </summary>
        public void CreateRenderObjects()
        {

            TextObject = new Render.Text((int)Drawing.WorldToScreen(Position).X, (int)Drawing.WorldToScreen(Position).Y, "", 17, new ColorBGRA(255, 255, 255, 255))
            {
                VisibleCondition = sender => Render.OnScreen(Drawing.WorldToScreen(Position)) && WardTrackerBase.moduleMenu["dz191.dza.ward.track"].Cast<CheckBox>().CurrentValue,
                PositionUpdate = () => new Vector2(Drawing.WorldToScreen(Position).X, Drawing.WorldToScreen(Position).Y + 12),
                TextUpdate = () => (Environment.TickCount < startTick + WardTypeW.WardDuration && WardTypeW.WardDuration < float.MaxValue) ? (Utils.FormatTime(Math.Abs(Environment.TickCount - (startTick + WardTypeW.WardDuration)) / 1000f)) : string.Empty
            };
            TextObject.Add(0);


            MinimapSpriteObject = new Render.Sprite(MinimapBitmap, new Vector2())
            {
                PositionUpdate =  () => MinimapPosition,
                VisibleCondition = sender => WardTrackerBase.moduleMenu["dz191.dza.ward.track"].Cast<CheckBox>().CurrentValue && Environment.TickCount <  this.startTick + this.WardTypeW.WardDuration,
                Scale = new Vector2(0.7f, 0.7f)
            };
            MinimapSpriteObject.Add(0);
        }

        /// <summary>
        /// Removes the render objects.
        /// </summary>
        public void RemoveRenderObjects()
        {
            TextObject.Remove();
            MinimapSpriteObject.Remove();
        }

        /**Credits to Tracker */
        /// <summary>
        /// Gets the minimap position.
        /// </summary>
        /// <value>
        /// The minimap position.
        /// </value>
        private Vector2 MinimapPosition => Drawing.WorldToMinimap(Position) +
                                           new Vector2(-32 / 2f * 0.7f, -32 / 2f * 0.7f);


        /// <summary>
        /// Gets the minimap sprite bitmap.
        /// </summary>
        /// <value>
        /// The minimap sprite bitmap.
        /// </value>
        public Bitmap MinimapBitmap
        {
            get
            {
                switch (WardTypeW.WardType)
                {
                    case WardType.Green:
                        return Resources.Minimap_Ward_Green_Enemy;
                    case WardType.Pink:
                        return Resources.Minimap_Ward_Pink_Enemy;
                    default:
                        return Resources.Minimap_Ward_Green_Enemy;
                }
            }
        }

    }

    /// <summary>
    /// The Ward Type wrapper class
    /// </summary>
    class WardTypeWrapper
    {
        /// <summary>
        /// Gets or sets the name of the object.
        /// </summary>
        /// <value>
        /// The name of the object.
        /// </value>
        public string ObjectName { get; set; }

        /// <summary>
        /// Gets or sets the name of the spell.
        /// </summary>
        /// <value>
        /// The name of the spell.
        /// </value>
        public string SpellName { get; set; }

        /// <summary>
        /// Gets or sets the ward vision range.
        /// </summary>
        /// <value>
        /// The ward vision range.
        /// </value>
        public float WardVisionRange { get; set; }

        /// <summary>
        /// Gets or sets the type of the ward.
        /// </summary>
        /// <value>
        /// The type of the ward.
        /// </value>
        public WardType WardType { get; set; }

        /// <summary>
        /// Gets the duration of the ward.
        /// </summary>
        /// <value>
        /// The duration of the ward.
        /// </value>
        public float WardDuration
        {
            get
            {
                try
                {
                    float val;
                    WardTrackerVariables.wardDurations.TryGetValue(WardType, out val);
                    return val;
                }
                catch (NullReferenceException ex)
                {
                    LogHelper.AddToLog(new LogItem("WardTracker_Var", ex, LogSeverity.Error));
                }

                return 0;
            }
        }

    }

    /// <summary>
    /// The Enumeration containing the ward types.
    /// </summary>
    enum WardType
    {
        Trinket, TrinketUpgrade, Pink, Green, TeemoShroom, ShacoBox
    }
}
