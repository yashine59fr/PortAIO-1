﻿using TargetSelector = PortAIO.TSManager; namespace ElEasy.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Color = System.Drawing.Color;
    using EloBuddy.SDK.Menu.Values;
    using EloBuddy;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK;
    internal class Nasus
    {
        #region Static Fields

        public static int Sheen = 3057, Iceborn = 3025;

        private static readonly Dictionary<Spells, LeagueSharp.Common.Spell> spells = new Dictionary<Spells, LeagueSharp.Common.Spell>
                                                                       {
                                                                           {
                                                                               Spells.Q,
                                                                               new LeagueSharp.Common.Spell(
                                                                               SpellSlot.Q,
                                                                               Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) + 100)
                                                                           },
                                                                           { Spells.W, new LeagueSharp.Common.Spell(SpellSlot.W, 600) },
                                                                           { Spells.E, new LeagueSharp.Common.Spell(SpellSlot.E, 650) },
                                                                           { Spells.R, new LeagueSharp.Common.Spell(SpellSlot.R) }
                                                                       };

        private static SpellSlot Ignite;

        #endregion

        #region Enums

        public enum Spells
        {
            Q,

            W,

            E,

            R
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        private static Menu Menu { get; set; }

        /// <summary>
        ///     Gets the player.
        /// </summary>
        /// <value>
        ///     The player.
        /// </value>
        private static AIHeroClient Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        #endregion

        #region Public Methods and Operators

        public static Menu comboMenu, harassMenu, clearMenu, lasthitMenu, miscellaneousMenu;

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        /// <param name="rootMenu">The root menu.</param>
        /// <returns></returns>
        public static void CreateMenu()
        {
            Menu = MainMenu.AddMenu("ElNasus", "ElNasus");
            {
                comboMenu = Menu.AddSubMenu("Combo", "Combo");
                {
                    comboMenu.Add("ElEasy.Nasus.Combo.Q", new CheckBox("Use Q"));
                    comboMenu.Add("ElEasy.Nasus.Combo.W", new CheckBox("Use W"));
                    comboMenu.Add("ElEasy.Nasus.Combo.E", new CheckBox("Use E"));
                    comboMenu.Add("ElEasy.Nasus.Combo.R", new CheckBox("Use R"));
                    comboMenu.Add("ElEasy.Nasus.Combo.Count.R", new Slider("Minimum champions in range for R", 2, 1, 5));
                    comboMenu.Add("ElEasy.Nasus.Combo.HP", new Slider("Minimum HP for R", 55));
                    comboMenu.Add("ElEasy.Nasus.Combo.Ignite", new CheckBox("Use Ignite"));
                }

                harassMenu = Menu.AddSubMenu("Harass", "Harass");
                {
                    harassMenu.Add("ElEasy.Nasus.Harass.E", new CheckBox("Use E"));
                    harassMenu.Add("ElEasy.Nasus.Harass.Player.Mana", new Slider("Minimum Mana", 55));
                }

                clearMenu = Menu.AddSubMenu("Clear", "Clear");
                {
                    clearMenu.AddGroupLabel("LaneClear");
                    clearMenu.Add("ElEasy.Nasus.LaneClear.Q", new CheckBox("Use Q"));
                    clearMenu.Add("ElEasy.Nasus.LaneClear.E", new CheckBox("Use E"));
                    clearMenu.AddGroupLabel("JungleClear");
                    clearMenu.Add("ElEasy.Nasus.JungleClear.Q", new CheckBox("Use Q"));
                    clearMenu.Add("ElEasy.Nasus.JungleClear.E", new CheckBox("Use E"));
                }

                lasthitMenu = Menu.AddSubMenu("Lasthit", "Lasthit");
                {
                    lasthitMenu.Add("ElEasy.Nasus.Lasthit.Activated", new KeyBind("Auto Lasthit", false, KeyBind.BindTypes.PressToggle, 'L'));
                    lasthitMenu.Add("ElEasy.Nasus.Lasthitrange", new Slider("Auto last hit range", 100, 100, 500));
                }

                miscellaneousMenu = Menu.AddSubMenu("Miscellaneous", "Miscellaneous");
                {
                    miscellaneousMenu.Add("ElEasy.Nasus.Draw.off", new CheckBox("Turn drawings off"));
                    miscellaneousMenu.Add("ElEasy.Nasus.Draw.LastHitRange", new CheckBox("Draw Last hit range"));//.SetValue(new Circle()));
                    miscellaneousMenu.Add("ElEasy.Nasus.Draw.W", new CheckBox("Draw W"));//.SetValue(new Circle()));
                    miscellaneousMenu.Add("ElEasy.Nasus.Draw.E", new CheckBox("Draw E"));//.SetValue(new Circle()));
                    miscellaneousMenu.Add("ElEasy.Nasus.Draw.Text", new CheckBox("Draw text"));
                    miscellaneousMenu.Add("ElEasy.Nasus.Draw.MinionHelper", new CheckBox("Draw killable minions"));
                }
            }
        }

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

        public static void Load()
        {
            Console.WriteLine("Loaded Nasus");
            CreateMenu();
            Ignite = Player.GetSpellSlot("summonerdot");
            spells[Spells.E].SetSkillshot(
                spells[Spells.E].Instance.SData.SpellCastTime,
                spells[Spells.E].Instance.SData.LineWidth,
                spells[Spells.E].Instance.SData.MissileSpeed,
                false,
                SkillshotType.SkillshotCircle);

            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        #endregion

        #region Methods
        
        private static void AutoLastHit()
        {
            if (PortAIO.OrbwalkerManager.isComboActive || PortAIO.OrbwalkerManager.isHarassActive || Player.LSIsRecalling() || !spells[Spells.Q].IsReady())
            {
                return;
            }

            var minions = MinionManager.GetMinions(
                Player.Position,
                spells[Spells.E].Range,
                MinionTypes.All,
                MinionTeam.Enemy,
                MinionOrderTypes.MaxHealth).OrderByDescending(m => GetBonusDmg(m) > m.Health);

            foreach (var minion in minions)
            {
                if (GetBonusDmg(minion) > minion.Health && Vector3.Distance(ObjectManager.Player.ServerPosition, minion.Position) < Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) + getSliderItem(lasthitMenu, "ElEasy.Nasus.Lasthitrange"))
                {
                    spells[Spells.Q].Cast();
                    if (PortAIO.OrbwalkerManager.isNoneActive)
                    {
                        EloBuddy.Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                    }
                    else
                    {
                        PortAIO.OrbwalkerManager.ForcedTarget(minion);
                    }
                    break;
                }
            }
        }

        private static double GetBonusDmg(Obj_AI_Base target)
        {
            double dmgItem = 0;
            if (Items.HasItem(Sheen) && (Items.CanUseItem(Sheen) || Player.HasBuff("sheen"))
                && Player.BaseAttackDamage > dmgItem)
            {
                dmgItem = Player.LSGetAutoAttackDamage(target);
            }

            if (Items.HasItem(Iceborn) && (Items.CanUseItem(Iceborn) || Player.HasBuff("itemfrozenfist"))
                && Player.BaseAttackDamage * 1.25 > dmgItem)
            {
                dmgItem = Player.LSGetAutoAttackDamage(target) * 1.25;
            }

            return spells[Spells.Q].GetDamage(target) + Player.LSGetAutoAttackDamage(target) + dmgItem;
        }

        private static float IgniteDamage(AIHeroClient target)
        {
            if (Ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float)Player.GetSummonerSpellDamage(target, LeagueSharp.Common.Damage.SummonerSpell.Ignite);
        }

        private static void Jungleclear()
        {
            var useQ = getCheckBoxItem(clearMenu, "ElEasy.Nasus.JungleClear.Q");
            var useE = getCheckBoxItem(clearMenu, "ElEasy.Nasus.JungleClear.E");

            var minions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition,
                spells[Spells.E].Range,
                MinionTypes.All,
                MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);

            if (minions.Count <= 0)
            {
                return;
            }

            if (useQ && spells[Spells.Q].IsReady())
            {
                if (minions.Find(x => x.Health >= spells[Spells.Q].GetDamage(x) && x.LSIsValidTarget()) != null)
                {
                    spells[Spells.Q].Cast();
                }
            }

            if (useE && spells[Spells.E].IsReady())
            {
                if (minions.Count > 1)
                {
                    var farmLocation = spells[Spells.E].GetCircularFarmLocation(minions);
                    spells[Spells.E].Cast(farmLocation.Position);
                }
            }
        }

        private static void Laneclear()
        {
            var useQ = getCheckBoxItem(clearMenu, "ElEasy.Nasus.LaneClear.Q");
            var useE = getCheckBoxItem(clearMenu, "ElEasy.Nasus.LaneClear.E");

            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.E].Range);
            if (minions.Count <= 0)
            {
                return;
            }

            if (spells[Spells.Q].IsReady() && useQ)
            {
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.E].Range);
                {
                    foreach (var minion in
                        allMinions.Where(
                            minion => minion.Health <= ObjectManager.Player.LSGetSpellDamage(minion, SpellSlot.Q)))
                    {
                        if (minion.LSIsValidTarget())
                        {
                            spells[Spells.Q].CastOnUnit(minion);
                            return;
                        }
                    }
                }
            }

            if (useE && spells[Spells.E].IsReady())
            {
                if (minions.Count > 1)
                {
                    var farmLocation = spells[Spells.E].GetCircularFarmLocation(minions);
                    spells[Spells.E].Cast(farmLocation.Position);
                }
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!args.SData.Name.ToLower().Contains("attack") || !sender.IsMe)
            {
                return;
            }

            var unit = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>((uint)args.Target.NetworkId);
            if ((GetBonusDmg(unit) > unit.Health))
            {
                spells[Spells.Q].Cast();
            }
        }

        private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.W].Range, DamageType.Physical);
            if (!target.LSIsValidTarget())
            {
                return;
            }

            var useQ = getCheckBoxItem(comboMenu, "ElEasy.Nasus.Combo.Q");
            var useW = getCheckBoxItem(comboMenu, "ElEasy.Nasus.Combo.W");
            var useE = getCheckBoxItem(comboMenu, "ElEasy.Nasus.Combo.E");
            var useR = getCheckBoxItem(comboMenu, "ElEasy.Nasus.Combo.R");
            var useI = getCheckBoxItem(comboMenu, "ElEasy.Nasus.Combo.Ignite");
            var countEnemies = getSliderItem(comboMenu, "ElEasy.Nasus.Combo.Count.R");
            var playerHp = getSliderItem(comboMenu, "ElEasy.Nasus.Combo.HP");

            if (useW && spells[Spells.W].IsReady())
            {
                spells[Spells.W].Cast(target);
            }

            if (useQ && spells[Spells.Q].IsReady())
            {
                spells[Spells.Q].Cast();
            }


            if (useE && spells[Spells.E].IsReady() && target.LSIsValidTarget())
            {
                var pred = spells[Spells.E].GetPrediction(target);
                if (pred.Hitchance >= HitChance.High)
                {
                    spells[Spells.E].Cast(pred.CastPosition);
                }
            }

            if (useR && spells[Spells.R].IsReady()
                && Player.LSCountEnemiesInRange(spells[Spells.W].Range) >= countEnemies
                || (Player.Health / Player.MaxHealth) * 100 <= playerHp)
            {
                spells[Spells.R].CastOnUnit(Player);
            }

            if (Player.LSDistance(target) <= 600 && IgniteDamage(target) >= target.Health && useI)
            {
                Player.Spellbook.CastSpell(Ignite, target);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            var drawOff = getCheckBoxItem(miscellaneousMenu, "ElEasy.Nasus.Draw.off");
            var drawW = getCheckBoxItem(miscellaneousMenu, "ElEasy.Nasus.Draw.W");
            var drawE = getCheckBoxItem(miscellaneousMenu, "ElEasy.Nasus.Draw.E");
            var drawText = getCheckBoxItem(miscellaneousMenu, "ElEasy.Nasus.Draw.Text");
            var rBool = getKeyBindItem(lasthitMenu, "ElEasy.Nasus.Lasthit.Activated");
            var helper = getCheckBoxItem(miscellaneousMenu, "ElEasy.Nasus.Draw.MinionHelper");
            var drawLastHit = getCheckBoxItem(miscellaneousMenu, "ElEasy.Nasus.Draw.LastHitRange");




            var playerPos = Drawing.WorldToScreen(Player.Position);

            if (drawOff)
            {
                return;
            }

            if (drawLastHit)
            {
                if (spells[Spells.Q].Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) + getSliderItem(lasthitMenu, "ElEasy.Nasus.Lasthitrange"), Color.Orange);
                }
            }

            if (drawE)
            {
                if (spells[Spells.E].Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, spells[Spells.E].Range, Color.White);
                }
            }

            if (drawW)
            {
                if (spells[Spells.W].Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, spells[Spells.W].Range, Color.White);
                }
            }

            if (drawText)
            {
                Drawing.DrawText(
                    playerPos.X - 70,
                    playerPos.Y + 40,
                    (rBool ? Color.Green : Color.Red),
                    (rBool ? "Auto lasthit enabled" : "Auto lasthit disabled"));
            }

            if (helper)
            {
                var minions = MinionManager.GetMinions(
                    ObjectManager.Player.ServerPosition,
                    spells[Spells.E].Range,
                    MinionTypes.All,
                    MinionTeam.NotAlly);
                foreach (var minion in minions)
                {
                    if (minion != null)
                    {
                        if ((GetBonusDmg(minion) > minion.Health))
                        {
                            Render.Circle.DrawCircle(minion.ServerPosition, minion.BoundingRadius, Color.Green);
                        }
                    }
                }
            }
        }

        private static void OnHarass()
        {
            var eTarget = TargetSelector.GetTarget(spells[Spells.E].Range + spells[Spells.E].Width, DamageType.Magical);
            if (eTarget == null || !eTarget.IsValid)
            {
                return;
            }

            var useE = getCheckBoxItem(harassMenu, "ElEasy.Nasus.Harass.E");

            if (useE && spells[Spells.E].IsReady() && eTarget.LSIsValidTarget() && spells[Spells.E].IsInRange(eTarget))
            {
                var pred = spells[Spells.E].GetPrediction(eTarget).Hitchance;
                if (pred >= HitChance.High)
                {
                    spells[Spells.E].Cast(eTarget);
                }
            }
        }

        private static void OnLastHit()
        {
            var minion =
                MinionManager.GetMinions(
                    Player.Position,
                    spells[Spells.Q].Range + 100,
                    MinionTypes.All,
                    MinionTeam.NotAlly,
                    MinionOrderTypes.MaxHealth).OrderByDescending(m => GetBonusDmg(m) > m.Health).FirstOrDefault();

            if (minion == null)
            {
                return;
            }

            if (GetBonusDmg(minion) > minion.Health && spells[Spells.Q].IsReady())
            {
                spells[Spells.Q].Cast();
                PortAIO.OrbwalkerManager.ForcedTarget(minion);
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            if (PortAIO.OrbwalkerManager.isComboActive)
            {
                OnCombo();
            }

            if (PortAIO.OrbwalkerManager.isHarassActive)
            {
                OnHarass();
            }

            if (PortAIO.OrbwalkerManager.isLastHitActive)
            {
                OnLastHit();
            }

            if (PortAIO.OrbwalkerManager.isLaneClearActive)
            {
                Laneclear();
                Jungleclear();
            }

            if (getKeyBindItem(lasthitMenu, "ElEasy.Nasus.Lasthit.Activated"))
            {
                AutoLastHit();
            }
        }

        #endregion
    }
}