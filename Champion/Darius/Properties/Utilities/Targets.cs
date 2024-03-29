using System.Collections.Generic;
using System.Linq;
using ExorAIO.Utilities;
using LeagueSharp;
using LeagueSharp.SDK;
using EloBuddy;
using EloBuddy.SDK;
using TargetSelector = PortAIO.TSManager;
namespace ExorAIO.Champions.Darius
{
    /// <summary>
    ///     The targets class.
    /// </summary>
    internal class Targets
    {
        
        /// <summary>
        ///     The main hero target.
        /// </summary>
        public static AIHeroClient Target => TargetSelector.GetTarget(Vars.E.Range, DamageType.Physical);

        /// <summary>
        ///     The minions target.
        /// </summary>
        public static List<Obj_AI_Minion> Minions
            =>
                GameObjects.EnemyMinions.Where(
                    m =>
                        m.IsMinion() &&
                        m.LSIsValidTarget(Vars.Q.Range)).ToList();

        /// <summary>
        ///     The jungle minion targets.
        /// </summary>
        public static List<Obj_AI_Minion> JungleMinions
            =>
                GameObjects.Jungle.Where(
                    m =>
                        m.LSIsValidTarget(Vars.Q.Range) &&
                        !GameObjects.JungleSmall.Contains(m)).ToList();
    }
}