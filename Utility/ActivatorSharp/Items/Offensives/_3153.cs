﻿using System;
using Activators.Base;
using LeagueSharp.Common;
using EloBuddy.SDK.Menu.Values;

using TargetSelector = PortAIO.TSManager; namespace Activators.Items.Offensives
{
    class _3153 : CoreItem
    {
        internal override int Id => 3153;
        internal override int Priority => 5;
        internal override string Name => "Botrk";
        internal override string DisplayName => "Blade of the Ruined King";
        internal override int Duration => 100;
        internal override float Range => 550f;
        internal override MenuType[] Category => new[] { MenuType.SelfLowHP, MenuType.EnemyLowHP };
        internal override MapType[] Maps => new[] { MapType.Common };
        internal override int DefaultHP => 90;
        internal override int DefaultMP => 0;


        public override void OnTick(EventArgs args)
        {
            if (!Menu["use" + Name].Cast<CheckBox>().CurrentValue || !IsReady())
                return;

            if (Tar != null)
            {
                if (Activator.omenu[Activator.omenu.UniqueMenuId + "useon" + Tar.Player.NetworkId] == null)
                {
                    return;
                }

                if (!Activator.omenu[Activator.omenu.UniqueMenuId + "useon" + Tar.Player.NetworkId].Cast<CheckBox>().CurrentValue)
                    return;

                if ((Tar.Player.Health / Tar.Player.MaxHealth * 100) <= Menu["enemylowhp" + Name + "pct"].Cast<Slider>().CurrentValue)
                {
                    UseItem(Tar.Player, true);
                }

                if ((Player.Health / Player.MaxHealth * 100) <= Menu["selflowhp" + Name + "pct"].Cast<Slider>().CurrentValue)
                {
                    UseItem(Tar.Player, true);
                }
            }
        }
    }
}
