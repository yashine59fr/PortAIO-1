using EloBuddy;
using ExorAIO.Utilities;
using LeagueSharp;
using LeagueSharp.SDK;

using TargetSelector = PortAIO.TSManager; namespace ExorAIO.Champions.Warwick
{
    /// <summary>
    ///     The spells class.
    /// </summary>
    internal class Spells
    {
        /// <summary>
        ///     Sets the spells.
        /// </summary>
        public static void Initialize()
        {
            Vars.Q = new Spell(SpellSlot.Q, 400f);
            Vars.W = new Spell(SpellSlot.W, 1250f);
            Vars.E = new Spell(SpellSlot.E, 700f + 800f * GameObjects.Player.Spellbook.GetSpell(SpellSlot.E).Level);
            Vars.R = new Spell(SpellSlot.R, 700f);
        }
    }
}