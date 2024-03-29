using TargetSelector = PortAIO.TSManager; namespace ElRengarRevamped
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using EloBuddy;
    using LeagueSharp.Common;

    using ItemData = LeagueSharp.Common.Data.ItemData;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Menu.Values;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class ActiveModes : Standards
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Handles combo
        /// </summary>
        public static void Combo()
        {
            /*var target = TargetSelector.SelectedTarget
                             ?? TargetSelector.GetTarget(spells[Spells.E].Range, DamageType.Physical);
            if (target == null)
            {
                return;
            }*/

            var forced = PortAIO.OrbwalkerManager.ForcedTarget();
            if (forced != null && forced.LSIsValidTarget() && forced is AIHeroClient &&
                    Orbwalking.InAutoAttackRange(forced))
            {
                return;
            }

            var target = TargetSelector.GetTarget(spells[Spells.E].Range, DamageType.Physical);
            if (target == null)
            {
                return;
            }

            #region RengarR

            if (Ferocity <= 4)
            {
                if (spells[Spells.Q].IsReady() && MenuInit.getCheckBoxItem(MenuInit.comboMenu, "Combo.Use.Q")
                    && Player.LSCountEnemiesInRange(Player.AttackRange + Player.BoundingRadius + 100) != 0)
                {
                    spells[Spells.Q].Cast();
                }

                if (!RengarR)
                {
                    if (!HasPassive)
                    {
                        if (spells[Spells.E].IsReady() && MenuInit.getCheckBoxItem(MenuInit.comboMenu, "Combo.Use.E"))
                        {
                            CastE(target);
                        }
                    }
                }

                CastItems(target);

                if (spells[Spells.W].IsReady() && MenuInit.getCheckBoxItem(MenuInit.comboMenu, "Combo.Use.W"))
                {
                    CastW();
                    CastItems(target);
                }
            }

            if (Ferocity == 5)
            {
                switch (MenuInit.getBoxItem(MenuInit.comboMenu, "Combo.Prio"))
                {
                    case 0:
                        if (!RengarR)
                        {
                            if (spells[Spells.E].IsReady() && !HasPassive)
                            {
                                CastE(target);

                                if (MenuInit.getCheckBoxItem(MenuInit.comboMenu, "Combo.Switch.E") && Utils.GameTimeTickCount - LastSwitch >= 350)
                                {
                                    MenuInit.comboMenu["Combo.Prio"].Cast<ComboBox>().CurrentValue = 2;
                                    LastSwitch = Utils.GameTimeTickCount;
                                }
                            }
                        }
                        break;
                    case 1:
                        if (MenuInit.getCheckBoxItem(MenuInit.comboMenu, "Combo.Use.W") && spells[Spells.W].IsReady() && target.LSIsValidTarget(spells[Spells.W].Range))
                        {
                            CastW();
                        }
                        break;
                    case 2:
                        if (spells[Spells.Q].IsReady() && MenuInit.getCheckBoxItem(MenuInit.comboMenu, "Combo.Use.Q")
                            && Player.LSCountEnemiesInRange(Player.AttackRange + Player.BoundingRadius + 100) != 0)
                        {
                            spells[Spells.Q].Cast();
                        }
                        break;
                }
            }

            #region Summoner spells

            if (MenuInit.getCheckBoxItem(MenuInit.comboMenu, "Combo.Use.Ignite") && target.LSIsValidTarget(600f) && IgniteDamage(target) >= target.Health)
            {
                Player.Spellbook.CastSpell(Ignite, target);
            }

            #endregion
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        ///     Handles E cast
        /// </summary>
        /// <param name="target"></param>
        private static void CastE(Obj_AI_Base target)
        {
            try
            {
                if (!spells[Spells.E].IsReady() || !target.LSIsValidTarget(spells[Spells.E].Range))
                {
                    return;
                }

                if (RengarR)
                {
                    return;
                }

                var pred = spells[Spells.E].GetPrediction(target);
                if (pred.Hitchance >= HitChance.High)
                {
                    spells[Spells.E].Cast(pred.CastPosition);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        ///     Handles W casting
        /// </summary>
        private static void CastW()
        {
            try
            {
                if (!spells[Spells.W].IsReady())
                {
                    return;
                }

                if (GetWHits().Item1 > 0)
                {
                    spells[Spells.W].Cast();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        ///     Get W hits
        /// </summary>
        /// <returns></returns>
        private static Tuple<int, List<AIHeroClient>> GetWHits()
        {
            try
            {
                var hits =
                    HeroManager.Enemies.Where(
                        e =>
                        e.LSIsValidTarget() && e.LSDistance(Player) < 450f
                        || e.LSDistance(Player) < 450f).ToList();

                return new Tuple<int, List<AIHeroClient>>(hits.Count, hits);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return new Tuple<int, List<AIHeroClient>>(0, null);
        }

        #endregion

        /// <summary>
        ///     Harass
        /// </summary>
        public static void Harass()
        {
            // ReSharper disable once ConvertConditionalTernaryToNullCoalescing
            var target = TargetSelector.SelectedTarget != null
                             ? TargetSelector.SelectedTarget
                             : TargetSelector.GetTarget(spells[Spells.Q].Range, DamageType.Physical);

            

            if (target.LSIsValidTarget() == false)
            {
                return;
            }

            #region RengarR

            if (Ferocity == 5)
            {
                switch (MenuInit.getBoxItem(MenuInit.comboMenu, "Harass.Prio"))
                {
                    case 0:
                        if (!HasPassive && MenuInit.getCheckBoxItem(MenuInit.harassMenu, "Harass.Use.E") && spells[Spells.E].IsReady())
                        {
                            CastE(target);
                        }
                        break;

                    case 1:
                        if (MenuInit.getCheckBoxItem(MenuInit.harassMenu, "Harass.Use.Q") && target.LSIsValidTarget(spells[Spells.Q].Range))
                        {
                            spells[Spells.Q].Cast();
                        }
                        break;
                }
            }

            if (Ferocity <= 4)
            {
                if (MenuInit.getCheckBoxItem(MenuInit.harassMenu, "Harass.Use.Q") && target.LSIsValidTarget(spells[Spells.Q].Range))
                {
                    spells[Spells.Q].Cast();
                }

                if (RengarR)
                {
                    return;
                }

                CastItems(target);

                if (!HasPassive && MenuInit.getCheckBoxItem(MenuInit.harassMenu, "Harass.Use.E") && spells[Spells.E].IsReady())
                {
                    CastE(target);
                }

                if (MenuInit.getCheckBoxItem(MenuInit.harassMenu, "Harass.Use.W"))
                {
                    CastW();
                }
            }
        }

        /// <summary>
        ///     Jungle clear
        /// </summary>
        public static void Jungleclear()
        {
            if (RengarR)
            {
                return;
            }
            try
            {
                var minion =
                    MinionManager.GetMinions(
                        Player.ServerPosition,
                        spells[Spells.W].Range,
                        MinionTypes.All,
                        MinionTeam.Neutral,
                        MinionOrderTypes.MaxHealth).FirstOrDefault();

                if (minion == null)
                {
                    return;
                }

                CastItems(minion);

                if (Ferocity == 5 && MenuInit.getCheckBoxItem(MenuInit.jungleClear, "Jungle.Save.Ferocity"))
                {
                    if (minion.LSIsValidTarget(spells[Spells.W].Range) && !HasPassive)
                    {
                        LaneItems(minion);
                    }
                    return;
                }

                if (MenuInit.getCheckBoxItem(MenuInit.jungleClear, "Jungle.Use.Q") && spells[Spells.Q].IsReady()
                    && minion.LSIsValidTarget(spells[Spells.Q].Range + 100))
                {
                    spells[Spells.Q].Cast();
                }

                LaneItems(minion);

                if (Ferocity == 5 && (Player.Health / Player.MaxHealth) * 100 <= 20)
                {
                    spells[Spells.W].Cast();
                }

                if (!HasPassive)
                {
                    if (MenuInit.getCheckBoxItem(MenuInit.jungleClear, "Jungle.Use.W") && spells[Spells.W].IsReady()
                        && minion.LSIsValidTarget(spells[Spells.W].Range))
                    {
                        if (Ferocity == 5 && spells[Spells.Q].IsReady())
                        {
                            return;
                        }
                        spells[Spells.W].Cast();
                    }
                }

                if (MenuInit.getCheckBoxItem(MenuInit.jungleClear, "Jungle.Use.E") && spells[Spells.E].IsReady()
                    && minion.LSIsValidTarget(spells[Spells.E].Range))
                {
                    if (Ferocity == 5)
                    {
                        return;
                    }

                    spells[Spells.E].Cast(minion.Position);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        ///     Lane clear
        /// </summary>
        public static void Laneclear()
        {
            try
            {
                var minion = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.W].Range).FirstOrDefault();
                if (minion == null)
                {
                    return;
                }

                if (Player.Spellbook.IsAutoAttacking || Player.Spellbook.IsAutoAttacking)
                {
                    return;
                }

                if (Ferocity == 5 && MenuInit.getCheckBoxItem(MenuInit.laneClear, "Clear.Save.Ferocity"))
                {
                    if (minion.LSIsValidTarget(spells[Spells.W].Range))
                    {
                        LaneItems(minion);
                    }
                    return;
                }

                if (MenuInit.getCheckBoxItem(MenuInit.laneClear, "Clear.Use.Q") && spells[Spells.Q].IsReady()
                    && minion.LSIsValidTarget(spells[Spells.Q].Range))
                {
                    spells[Spells.Q].Cast();
                }

                LaneItems(minion);

                if (MenuInit.getCheckBoxItem(MenuInit.laneClear, "Clear.Use.W") && spells[Spells.W].IsReady()
                    && minion.LSIsValidTarget(spells[Spells.W].Range))
                {
                    spells[Spells.W].Cast();
                }

                if (MenuInit.getCheckBoxItem(MenuInit.laneClear, "Clear.Use.E") && spells[Spells.E].IsReady()
                    && minion.LSIsValidTarget(spells[Spells.E].Range))
                {
                    if (Ferocity == 5)
                    {
                        return;
                    }

                    spells[Spells.E].Cast(minion.Position);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        ///     Gets Youmuus Ghostblade
        /// </summary>
        /// <value>
        ///     Youmuus Ghostblade
        /// </value>
        private static new Item Youmuu => new Item(ItemId.Youmuus_Ghostblade);

        /// <summary>
        ///     Gets Ravenous Hydra
        /// </summary>
        /// <value>
        ///     Ravenous Hydra
        /// </value>
        private static Item Hydra => new Item(ItemId.Ravenous_Hydra_Melee_Only);

        /// <summary>
        ///     Gets Tiamat Item
        /// </summary>
        /// <value>
        ///     Tiamat Item
        /// </value>
        private static Item Tiamat => new Item(ItemId.Tiamat_Melee_Only);

        /// <summary>
        ///     Gets Titanic Hydra
        /// </summary>
        /// <value>
        ///     Titanic Hydra
        /// </value>
        private static Item Titanic => new Item(ItemId.Titanic_Hydra);

        /// <summary>
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool LaneItems(Obj_AI_Base target)
        {
            var units =
                MinionManager.GetMinions(385, MinionTypes.All, MinionTeam.NotAlly).Count(o => !(o is Obj_AI_Turret));
            var count = units;
            var tiamat = Tiamat;
            if (tiamat.IsOwned() && tiamat.IsReady() && count > 0 && tiamat.Cast())
            {
                return true;
            }

            var hydra = Hydra;
            if (Hydra.IsOwned() && Hydra.IsReady() && count > 0 && hydra.Cast())
            {
                return true;
            }

            return false;
        }
        
        /// <summary>
        ///     Cast items
        /// </summary>
        /// <param name="target"></param>
        /// <returns>true or false</returns>
        public static bool CastItems(Obj_AI_Base target)
        {
            if (Player.LSIsDashing() || Player.Spellbook.IsAutoAttacking || RengarR)
            {
                return false;
            }

            var heroes = Player.GetEnemiesInRange(385).Count;
            var count = heroes;

            var tiamat = Tiamat;
            if (tiamat.IsOwned() && tiamat.IsReady() && count > 0 && tiamat.Cast())
            {
                return true;
            }

            var hydra = Hydra;
            if (Hydra.IsOwned() && Hydra.IsReady() && count > 0 && hydra.Cast())
            {
                return true;
            }

            var youmuus = Youmuu;
            if (Youmuu.IsOwned() && Youmuu.IsReady() && PortAIO.OrbwalkerManager.isComboActive || PortAIO.OrbwalkerManager.isHarassActive && youmuus.Cast())
            {
                return true;
            }

            var titanic = Titanic;
            return Titanic.IsOwned() && titanic.IsReady() && count > 0 && titanic.Cast();
        }

        #endregion
    }
}