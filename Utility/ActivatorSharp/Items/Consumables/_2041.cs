﻿using System;
using Activators.Base;
using LeagueSharp.Common;
using EloBuddy.SDK.Menu.Values;

using TargetSelector = PortAIO.TSManager; namespace Activators.Items.Consumables
{
    class _2041 : CoreItem
    {
        internal override int Id => 2041;
        internal override int Priority => 3;
        internal override string Name => "Crystalline Flask";
        internal override string DisplayName => "Crystalline Flask";
        internal override int Duration => 101;
        internal override float Range => float.MaxValue;
        internal override MenuType[] Category => new[] { MenuType.SelfLowMP, MenuType.SelfLowHP, MenuType.SelfMuchHP };
        internal override MapType[] Maps => new[] { MapType.SummonersRift, MapType.TwistedTreeline, MapType.CrystalScar };
        internal override int DefaultHP => 55;
        internal override int DefaultMP => 25;

        public override void OnTick(EventArgs args)
        {
            if (!Menu["use" + Name].Cast<CheckBox>().CurrentValue || !IsReady())
                return;

            foreach (var hero in Activator.Allies())
            {
                if (hero.Player.NetworkId == Player.NetworkId)
                {
                    if (hero.Player.HasBuff("ItemCrystalFlask"))
                        return;

                    if (hero.Player.MaxHealth - hero.Player.Health + hero.IncomeDamage > 120)
                    {
                        if (hero.Player.Health / hero.Player.MaxHealth * 100 <=
                            Menu["selflowhp" + Name + "pct"].Cast<Slider>().CurrentValue)
                        {
                            if ((hero.IncomeDamage > 0 || hero.MinionDamage > 0 || hero.TowerDamage > 0) ||
                                !Menu["use" + Name + "cbat"].Cast<CheckBox>().CurrentValue)
                            {
                                if (!hero.Player.LSIsRecalling() && !hero.Player.InFountain())
                                    UseItem();
                            }
                        }

                        if (hero.IncomeDamage / hero.Player.MaxHealth * 100 >=
                            Menu["selfmuchhp" + Name + "pct"].Cast<Slider>().CurrentValue)
                        {
                            if (!hero.Player.LSIsRecalling() && !hero.Player.InFountain())
                                UseItem();
                        }
                    }

                    if (hero.Player.MaxMana <= 200)
                        continue;

                    if (hero.Player.Mana / hero.Player.MaxMana * 100 <=
                        Menu["selflowmp" + Name + "pct"].Cast<Slider>().CurrentValue)
                    {
                        if (!hero.Player.LSIsRecalling() && !hero.Player.InFountain())
                            UseItem();
                    }
                }
            }
        }
    }
}
