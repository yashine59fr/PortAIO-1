using System;
using ExorAIO.Utilities;
using LeagueSharp;
using LeagueSharp.SDK;
using EloBuddy;
using EloBuddy.SDK;

using TargetSelector = PortAIO.TSManager; namespace ExorAIO.Champions.Warwick
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
        public static void Clear(EventArgs args)
        {
            if (Bools.HasSheenBuff() ||
                !(PortAIO.OrbwalkerManager.LastTarget() as Obj_AI_Minion).IsValidTarget())
            {
                return;
            }

            /// <summary>
            ///     The W Clear Logic.
            /// </summary>
            if (Vars.W.IsReady() &&
                GameObjects.Player.Spellbook.IsAutoAttacking &&
                GameObjects.Player.ManaPercent >
                    ManaManager.GetNeededMana(Vars.W.Slot, Vars.getSliderItem(Vars.WMenu, "clear")) &&
                Vars.getSliderItem(Vars.WMenu, "clear") != 101)
            {
                Vars.W.Cast();
            }
            
            if (GameObjects.Player.Spellbook.IsAutoAttacking)
            {
                return;
            }

            /// <summary>
            ///     The Q Clear Logic.
            /// </summary>
            if (Vars.Q.IsReady() &&
                GameObjects.Player.ManaPercent >
                    ManaManager.GetNeededMana(Vars.Q.Slot, Vars.getSliderItem(Vars.QMenu, "clear")) &&
                Vars.getSliderItem(Vars.QMenu, "clear") != 101)
            {
                if (GameObjects.Player.MaxHealth >
                        GameObjects.Player.Health +
                        (float)GameObjects.Player.LSGetSpellDamage(PortAIO.OrbwalkerManager.LastTarget() as Obj_AI_Minion, SpellSlot.Q) * 0.8)
                {
                    Vars.Q.CastOnUnit(PortAIO.OrbwalkerManager.LastTarget() as Obj_AI_Minion);
                }
            }
        }
    }
}