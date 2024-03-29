﻿using System;
using Activators.Base;
using LeagueSharp.Common;
using EloBuddy.SDK.Menu.Values;

using TargetSelector = PortAIO.TSManager; namespace Activators.Items.Consumables
{
    class _2137 : CoreItem
    {
        internal override int Id => 2137;
        internal override int Priority => 3;
        internal override string Name => "Elixir of Ruin";
        internal override string DisplayName => "Elixir of Ruin";
        internal override int Duration => 101;
        internal override float Range => float.MaxValue;
        internal override MenuType[] Category => new[] { MenuType.SelfLowHP, MenuType.SelfMuchHP };
        internal override MapType[] Maps => new[] { MapType.SummonersRift, MapType.TwistedTreeline, MapType.HowlingAbyss };
        internal override int DefaultHP => 10;
        internal override int DefaultMP => 0;

        public override void OnTick(EventArgs args)
        {
            if (!Menu["use" + Name].Cast<CheckBox>().CurrentValue || !IsReady())
                return;

            foreach (var hero in Activator.Allies())
            {
                if (hero.Player.NetworkId == Player.NetworkId)
                {
                    if (hero.Player.HasBuff("ElixirOfRuin"))
                        return;

                    if (hero.Player.LSIsRecalling() || hero.Player.InFountain())
                        return;

                    if (hero.Player.Health/hero.Player.MaxHealth*100 <=
                        Menu["selflowhp" + Name + "pct"].Cast<Slider>().CurrentValue && hero.IncomeDamage > 0)
                        UseItem();

                    if (hero.IncomeDamage / hero.Player.MaxHealth * 100 >=
                        Menu["selfmuchhp" + Name + "pct"].Cast<Slider>().CurrentValue)
                        UseItem();
                }
            }
        }
    }
}
