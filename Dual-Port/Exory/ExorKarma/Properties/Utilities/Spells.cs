using EloBuddy;
using ExorAIO.Utilities;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Enumerations;

using TargetSelector = PortAIO.TSManager; namespace ExorAIO.Champions.Karma
{
    /// <summary>
    ///     The spell class.
    /// </summary>
    internal class Spells
    {
        /// <summary>
        ///     Sets the spells.
        /// </summary>
        public static void Initialize()
        {
            Vars.Q = new Spell(SpellSlot.Q, 950f);
            Vars.W = new Spell(SpellSlot.W, 675f);
            Vars.E = new Spell(SpellSlot.E, 800f);
            Vars.R = new Spell(SpellSlot.R);

            Vars.Q.SetSkillshot(0.25f, 60f, 1700f, true, SkillshotType.SkillshotLine);
        }
    }
}