﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using LeagueSharp.Common.Data;
using ItemData = LeagueSharp.Common.Data.ItemData;
using FioraProject.Evade;
using EloBuddy;
using TargetSelector = PortAIO.TSManager;
namespace FioraProject
{
    public class CustomDamageIndicator
    {
        private const int BAR_WIDTH = 104;
        private const int LINE_THICKNESS = 9;
        
        private static LeagueSharp.Common.Utility.HpBarDamageIndicator.DamageToUnitDelegate damageToUnit;

        private static readonly Vector2 BarOffset = new Vector2(10, 25);

        private static System.Drawing.Color _drawingColor;
        public static System.Drawing.Color DrawingColor
        {
            get { return _drawingColor; }
            set { _drawingColor = Color.FromArgb(170, value); }
        }

        public static bool Enabled { get; set; }

        public static void Initialize(LeagueSharp.Common.Utility.HpBarDamageIndicator.DamageToUnitDelegate damageToUnit)
        {
            // Apply needed field delegate for damage calculation
            CustomDamageIndicator.damageToUnit = damageToUnit;
            DrawingColor = System.Drawing.Color.DeepPink;
            Enabled = true;

            // Register event handlers
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Enabled)
            {
                foreach (var unit in HeroManager.Enemies.Where(u => u.LSIsValidTarget() && u.IsHPBarRendered))
                {
                    // Get damage to unit
                    var damage = damageToUnit(unit);

                    // Continue on 0 damage
                    if (damage <= 0)
                        continue;

                    // Get remaining HP after damage applied in percent and the current percent of health
                    var damagePercentage = ((unit.Health - damage) > 0 ? (unit.Health - damage) : 0) / unit.MaxHealth;
                    var currentHealthPercentage = unit.Health / unit.MaxHealth;

                    // Calculate start and end point of the bar indicator
                    var startPoint = new Vector2((int)(unit.HPBarPosition.X + BarOffset.X + damagePercentage * BAR_WIDTH), (int)(unit.HPBarPosition.Y + BarOffset.Y) - 5);
                    var endPoint = new Vector2((int)(unit.HPBarPosition.X + BarOffset.X + currentHealthPercentage * BAR_WIDTH) + 1, (int)(unit.HPBarPosition.Y + BarOffset.Y) - 5);

                    // Draw the line
                    Drawing.DrawLine(startPoint, endPoint, LINE_THICKNESS, DrawingColor);
                }
            }
        }
    }
}
