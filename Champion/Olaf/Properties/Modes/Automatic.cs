using System;
using ExorAIO.Utilities;
using LeagueSharp.SDK;

using TargetSelector = PortAIO.TSManager; namespace ExorAIO.Champions.Olaf
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
            /// <summary>
            ///     The R Automatic Logic.
            /// </summary>
            if (Vars.R.IsReady() &&
                Bools.ShouldCleanse(GameObjects.Player) &&
                Vars.getCheckBoxItem(Vars.RMenu, "logical"))
            {
                Vars.R.Cast();
            }
        }
    }
}