﻿using System;
using Activators.Base;
using LeagueSharp.Common;
using EloBuddy.SDK.Menu.Values;

using TargetSelector = PortAIO.TSManager; namespace Activators.Items.Consumables
{
    class _2003 : CoreItem
    {
        internal override int Id => 2003;
        internal override int Priority => 3;
        internal override string Name => "Health Potion";
        internal override string DisplayName => "Health Potion";
        internal override int Duration => 101;
        internal override float Range => float.MaxValue;
        internal override MenuType[] Category => new[] { MenuType.SelfLowHP, MenuType.SelfMuchHP };
        internal override MapType[] Maps => new[] { MapType.Common };
        internal override int DefaultHP => 55;
        internal override int DefaultMP => 0;

        public override void OnTick(EventArgs args)
        {
            if (!Menu["use" + Name].Cast<CheckBox>().CurrentValue || !IsReady())
                return;

            foreach (var hero in Activator.Allies())
            {
                if (hero.Player.NetworkId == Player.NetworkId)
                {
                    if (hero.Player.HasBuff("RegenerationPotion") || 
                        hero.Player.MaxHealth - hero.Player.Health + hero.IncomeDamage <= 150)
                        return;

                    if (hero.Player.Health/hero.Player.MaxHealth*100 <=
                        Menu["selflowhp" + Name + "pct"].Cast<Slider>().CurrentValue)
                    {
                        if ((hero.IncomeDamage > 0 || hero.MinionDamage > 0 || hero.TowerDamage > 0) ||
                            !Menu["use" + Name + "cbat"].Cast<CheckBox>().CurrentValue)
                        {
                            if (!hero.Player.LSIsRecalling() && !hero.Player.InFountain())
                                UseItem();
                        }
                    }

                    if (hero.IncomeDamage/hero.Player.MaxHealth*100 >=
                        Menu["selfmuchhp" + Name + "pct"].Cast<Slider>().CurrentValue)
                    {
                        if (!hero.Player.LSIsRecalling() && !hero.Player.InFountain())
                            UseItem();
                    }        
                }
            }
        }
    }
}
