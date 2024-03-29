using System.Linq;
using EloBuddy;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core.Utils;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK;

using TargetSelector = PortAIO.TSManager; namespace NabbActivator
{
    /// <summary>
    ///     The activator class.
    /// </summary>
    internal partial class Activator
    {
        
        /// <summary>
        ///     Called on do-cast.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The args.</param>
        public static void Resetters(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Vars.TypesMenu["resetters"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }

            if (!PortAIO.OrbwalkerManager.isComboActive &&
                !PortAIO.OrbwalkerManager.isLaneClearActive)
            {
                return;
            }

            /// <summary>
            ///     If the player has no AA-Resets, triggers after normal AA, else after AA-Reset.
            /// </summary>
            if (sender.IsMe)
            {
                if ((!Vars.HasAnyReset && AutoAttack.IsAutoAttack(args.SData.Name)) ||
                    ObjectManager.Player.Buffs.Any(b => AutoAttack.IsAutoAttackReset(b.Name)))
                {
                    /// <summary>
                    ///     The Tiamat Melee Only logic.
                    /// </summary>
                    if (Items.CanUseItem(3077))
                    {
                        Items.UseItem(3077);
                    }

                    /// <summary>
                    ///     The Ravenous Hydra Melee Only logic.
                    /// </summary>
                    if (Items.CanUseItem(3074))
                    {
                        Items.UseItem(3074);
                    }

                    /// <summary>
                    ///     The Titanic Hydra Melee Only logic.
                    /// </summary>
                    if (Items.CanUseItem(3748))
                    {
                        Items.UseItem(3748);
                    }
                }
            }
        }
    }
}