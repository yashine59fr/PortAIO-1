﻿#region

using EloBuddy;
using EloBuddy.SDK;
using LeagueSharp.SDK;

#endregion

using TargetSelector = PortAIO.TSManager; namespace Spirit_Karma.Core
{
    internal class Core
    {
        public static AIHeroClient Player => ObjectManager.Player;
        public static AIHeroClient Target => TargetSelector.GetTarget(1050, DamageType.Magical);
    }
}
