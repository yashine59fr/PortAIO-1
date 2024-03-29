using System;
using System.Linq;
using ExorAIO.Utilities;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core.Utils;
using EloBuddy;

using TargetSelector = PortAIO.TSManager; namespace ExorAIO.Champions.Caitlyn
{
    /// <summary>
    ///     The logics class.
    /// </summary>
    internal partial class Logics
    {
        /// <summary>
        ///     Called when the game updates itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void Automatic(EventArgs args)
        {
            if (GameObjects.Player.LSIsRecalling())
            {
                return;
            }

            /// <summary>
            ///     The Automatic W Logic. 
            /// </summary>
            if (Vars.W.IsReady() &&
                Vars.getCheckBoxItem(Vars.WMenu, "logical"))
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(
                    t =>
                        Bools.IsImmobile(t) &&
                        t.LSIsValidTarget(Vars.W.Range) &&
                        !Invulnerable.Check(t, DamageType.Magical, false)))
                {
                    Vars.W.Cast(target.ServerPosition);
                }
                
            }

            /// <summary>
            ///     The Automatic Q Logic.
            /// </summary>
            if (Vars.Q.IsReady() &&
                GameObjects.Player.CountEnemyHeroesInRange(Vars.AARange) < 3 &&
                Vars.getCheckBoxItem(Vars.QMenu, "logical"))
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(
                    t =>
                        Bools.IsImmobile(t) &&
                        !Invulnerable.Check(t) &&
                        t.LSIsValidTarget(Vars.Q.Range) &&
                        t.HasBuff("caitlynyordletrapsight")))
                {
                    Vars.Q.Cast(target.ServerPosition);
                }
            }

            /// <summary>
            ///     The Semi-Automatic R Management.
            /// </summary>
            if (Vars.R.IsReady() &&
                Vars.getCheckBoxItem(Vars.RMenu, "bool") &&
                Vars.getKeyBindItem(Vars.RMenu, "key"))
            {
                if (!GameObjects.EnemyHeroes.Any(
                    t =>
                    !Invulnerable.Check(t) &&
                    t.LSIsValidTarget(Vars.R.Range)))
                {
                    return;
                }

                Vars.R.CastOnUnit(
                    GameObjects.EnemyHeroes.OrderBy(o => o.Health).FirstOrDefault(
                        t =>
                            !Invulnerable.Check(t) &&
                            t.LSIsValidTarget(Vars.R.Range)));
            }
        }
    }
}