using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using ExorAIO.Utilities;
using LeagueSharp.Data.Enumerations;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core.Utils;
using System.Linq;
using TargetSelector = PortAIO.TSManager;
namespace ExorAIO.Champions.Sivir
{
    /// <summary>
    ///     The menu class.
    /// </summary>
    internal class Menus
    {

        public static bool getCheckBoxItem(Menu m, string item)
        {
            return m[item].Cast<CheckBox>().CurrentValue;
        }

        public static int getSliderItem(Menu m, string item)
        {
            return m[item].Cast<Slider>().CurrentValue;
        }

        public static bool getKeyBindItem(Menu m, string item)
        {
            return m[item].Cast<KeyBind>().CurrentValue;
        }

        public static int getBoxItem(Menu m, string item)
        {
            return m[item].Cast<ComboBox>().CurrentValue;
        }

        /// <summary>
        ///     Sets the menu.
        /// </summary>
        public static void Initialize()
        {
            /// <summary>
            ///     Sets the menu for the Q.
            /// </summary>
            Vars.QMenu = Vars.Menu.AddSubMenu("Use Q to:", "q");
            {
                Vars.QMenu.Add("combo", new CheckBox("Combo", true));
                Vars.QMenu.Add("killsteal", new CheckBox("KillSteal", true));
                Vars.QMenu.Add("logical", new CheckBox("Logical", true));
                Vars.QMenu.AddLabel("Set the sliders to 101 below to disable them.");
                Vars.QMenu.Add("harass", new Slider("Harass / if Mana >= x%", 50, 0, 101));
                Vars.QMenu.Add("clear", new Slider("Clear / if Mana >= x%", 50, 0, 101));
            }

            /// <summary>
            ///     Sets the menu for the W.
            /// </summary>
            Vars.WMenu = Vars.Menu.AddSubMenu("Use W to:", "w");
            {
                Vars.WMenu.Add("combo", new CheckBox("Combo", true));
                Vars.QMenu.AddLabel("Set the sliders to 101 below to disable them.");
                Vars.WMenu.Add("clear", new Slider("Clear / if Mana >= x%", 50, 0, 101));
                Vars.WMenu.Add("buildings", new Slider("Buildings / if Mana >= x%", 50, 0, 101));
            }

            /// <summary>
            ///     Sets the menu for the E.
            /// </summary>
            Vars.EMenu = Vars.Menu.AddSubMenu("Use E to:", "e");
            {
                Vars.EMenu.AddLabel("It has to be used in conjunction with Evade, else it will not shield Skillshots");
                Vars.EMenu.AddLabel("It it meant to shield what Evade doesn't support, like targetted spells, AoE, etc.");
                Vars.EMenu.Add("logical", new CheckBox("Logical", true));
                Vars.EMenu.Add("delay", new Slider("E Delay (ms)", 0, 0, 250));
            }

            /// <summary>
            ///     Sets the menu for the E Whitelist.
            /// </summary>
            Vars.WhiteListMenu = Vars.Menu.AddSubMenu("Shield: Whitelist Menu");
            {
                Vars.WhiteListMenu.Add("minions", new CheckBox("Shield: Dragon/Baron Attacks"));
                foreach (var enemy in GameObjects.EnemyHeroes)
                {
                    if (enemy.ChampionName.Equals("Alistar"))
                    {
                        Vars.WhiteListMenu.Add($"{enemy.ChampionName.ToLower()}.pulverize", new CheckBox($"Shield: {enemy.ChampionName}'s Q"));
                    }

                    if (enemy.ChampionName.Equals("Braum"))
                    {
                        Vars.WhiteListMenu.Add($"{enemy.ChampionName.ToLower()}.braumbasicattackpassiveoverride", new CheckBox($"Shield: {enemy.ChampionName}'s Passive Stun"));
                    }

                    if (enemy.ChampionName.Equals("Udyr"))
                    {
                        Vars.WhiteListMenu.Add($"{enemy.ChampionName.ToLower()}.udyrbearattack", new CheckBox($"Shield: {enemy.ChampionName}'s E Stun"));
                    }

                    foreach (var spell in SpellDatabase.Get().Where(
                        s =>
                            !s.SpellName.Equals("KatarinaE") &&
                            !s.SpellName.Equals("TalonCutthroat") &&
                            s.ChampionName.Equals(enemy.ChampionName) &&
                            (s.CastType.Contains(CastType.EnemyChampions) ||
                            ((s.CastType.Contains(CastType.Activate) &&
                            AutoAttack.IsAutoAttackReset(s.SpellName))))))
                    {
                        if (spell.SpellType.HasFlag(SpellType.Targeted) ||
                            spell.SpellType.HasFlag(SpellType.Activated) ||
                            spell.SpellType.HasFlag(SpellType.TargetedMissile))
                        {
                            Vars.WhiteListMenu.Add($"{enemy.ChampionName.ToLower()}.{spell.SpellName.ToLower()}", new CheckBox($"Shield: {enemy.ChampionName}'s {spell.Slot}"));
                        }
                    }
                }
            }

            /// <summary>
            ///     Sets the drawings menu.
            /// </summary>
            Vars.DrawingsMenu = Vars.Menu.AddSubMenu("Drawings");
            {
                Vars.DrawingsMenu.Add("q", new CheckBox("Q Range"));
            }
        }
    }
}