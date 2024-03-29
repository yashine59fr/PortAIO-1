﻿using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SPrediction;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.Helpers;
using VayneHunter_Reborn.Utility.MenuUtility;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Menu;

using TargetSelector = PortAIO.TSManager; namespace VayneHunter_Reborn.Skills.Condemn.Methods
{
    class Marksman
    {
        public static bool getCheckBoxItem(Menu m, string item)
        {
            return m[item].Cast<CheckBox>().CurrentValue;
        }

        public static int getSliderItem(Menu m, string item)
        {
            return m[item].Cast<Slider>().CurrentValue;
        }

        public static bool getKeyBindItem(Menu m, string item)
        {
            return m[item].Cast<KeyBind>().CurrentValue;
        }

        public static int getBoxItem(Menu m, string item)
        {
            return m[item].Cast<ComboBox>().CurrentValue;
        }

        public static AIHeroClient GetTarget(Vector3 fromPosition)
        {
            foreach (var target in HeroManager.Enemies.Where(h => h.LSIsValidTarget(Variables.spells[SpellSlot.E].Range)))
            {
                var pushDistance = MenuGenerator.miscMenu["dz191.vhr.misc.condemn.pushdistance"].Cast<Slider>().CurrentValue;

                var targetPosition = Vector3.Zero;

                var pred = Variables.spells[SpellSlot.E].GetPrediction(target);

                if (pred.Hitchance > HitChance.Impossible)
                {
                    targetPosition = pred.UnitPosition;
                }

                if (targetPosition == Vector3.Zero)
                {
                    return null;
                }

                var finalPosition = targetPosition.LSExtend(fromPosition, -pushDistance);
                var finalPosition2 = targetPosition.LSExtend(fromPosition, -(pushDistance / 2f));
                var j4Flag = getCheckBoxItem(MenuGenerator.miscMenu, "dz191.vhr.misc.condemn.condemnflag") && (finalPosition.IsJ4Flag(target) || finalPosition2.IsJ4Flag(target));
                if (finalPosition.IsWall() || finalPosition2.IsWall() || j4Flag)
                {
                    if (getCheckBoxItem(MenuGenerator.miscMenu, "dz191.vhr.misc.condemn.onlystuncurrent") && PortAIO.OrbwalkerManager.LastTarget() != null && !target.NetworkId.Equals(PortAIO.OrbwalkerManager.LastTarget().NetworkId))
                    {
                        return null;
                    }

                    if (target.Health + 10 <=
                            ObjectManager.Player.GetAutoAttackDamage(target) *
                            getSliderItem(MenuGenerator.miscMenu, "dz191.vhr.misc.condemn.noeaa"))
                    {
                        return null;
                    }

                    return target;
                }
            }
            return null;
        }
    }
}
