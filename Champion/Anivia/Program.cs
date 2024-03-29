﻿using System;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using LeagueSharp.Common;
using SebbyLib;
using Orbwalking = SebbyLib.Orbwalking;
using Spell = LeagueSharp.Common.Spell;
using TargetSelector = PortAIO.TSManager;

namespace PortAIO.Champion.Anivia
{
    class Anivia
    {
        private static readonly Menu Config = SebbyLib.Program.Config;
        private static Spell E, Q, R, W;
        private static float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        private static int Rwidth = 400;
        private static GameObject QMissile, RMissile;
        private static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }
        
        public static Menu drawMenu, QMenu, WMenu, EMenu, RMenu, FarmMenu, AniviaMenu;

        public static void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 1000);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 685);

            Q.SetSkillshot(0.25f, 110f, 870f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.6f, 1f, float.MaxValue, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.5f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            LoadMenuOKTW();

            Game.OnUpdate += Game_OnGameUpdate;
            GameObject.OnDelete += Obj_AI_Base_OnDelete;
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Interrupter2_OnInterruptableTarget(AIHeroClient sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (getCheckBoxItem(WMenu, "inter") && W.IsReady() && sender.LSIsValidTarget(W.Range))
                W.Cast(sender);
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var Target = gapcloser.Sender;
            if (Q.IsReady() && getCheckBoxItem(QMenu, "AGCQ"))
            {
                if (Target.LSIsValidTarget(300))
                {
                    Q.Cast(Target);
                    Program.debug("AGC Q");
                }
            }
            else if (W.IsReady() && getCheckBoxItem(WMenu, "AGCW"))
            {
                if (Target.LSIsValidTarget(W.Range))
                {
                    W.Cast(ObjectManager.Player.Position.LSExtend(Target.Position, 50), true);
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

        private static void LoadMenuOKTW()
        {
            drawMenu = Config.AddSubMenu("Draw");
            drawMenu.Add("qRange", new CheckBox("Q range"));
            drawMenu.Add("wRange", new CheckBox("W range"));
            drawMenu.Add("eRange", new CheckBox("E range"));
            drawMenu.Add("rRange", new CheckBox("R range"));
            drawMenu.Add("onlyRdy", new CheckBox("Draw only ready spells"));

            QMenu = Config.AddSubMenu("Q Config");
            QMenu.Add("autoQ", new CheckBox("Auto Q"));
            QMenu.Add("AGCQ", new CheckBox("Q gapcloser"));
            QMenu.Add("harrasQ", new CheckBox("Harass Q"));
            foreach (var enemy in  HeroManager.Enemies)
            {
                QMenu.Add("haras" + enemy.NetworkId, new CheckBox("Harass :" + enemy.ChampionName));
            }

            WMenu = Config.AddSubMenu("W Config");
            WMenu.Add("autoW", new CheckBox("Auto W"));
            WMenu.Add("AGCW", new CheckBox("AntiGapcloser W"));
            WMenu.Add("inter", new CheckBox("OnPossibleToInterrupt W"));

            EMenu = Config.AddSubMenu("E Config");
            EMenu.Add("autoE", new CheckBox("Auto E"));

            RMenu = Config.AddSubMenu("R Config");
            RMenu.Add("autoR", new CheckBox("Auto R"));

            FarmMenu = Config.AddSubMenu("Farm");
            FarmMenu.Add("farmE", new CheckBox("Lane clear E"));
            FarmMenu.Add("farmR", new CheckBox("Lane clear R"));
            FarmMenu.Add("Mana", new Slider("LaneClear Mana", 80));
            FarmMenu.Add("LCminions", new Slider("LaneClear minimum minions", 2, 0, 10));
            FarmMenu.Add("jungleQ", new CheckBox("Jungle clear Q"));
            FarmMenu.Add("jungleW", new CheckBox("Jungle clear W"));
            FarmMenu.Add("jungleE", new CheckBox("Jungle clear E"));
            FarmMenu.Add("jungleR", new CheckBox("Jungle clear R"));

            AniviaMenu = Config.AddSubMenu(Player.ChampionName);
            AniviaMenu.Add("AACombo", new CheckBox("Disable AA if can use E"));
        }

        private static void Obj_AI_Base_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.IsValid)
            {
                if (obj.Name == "cryo_FlashFrost_Player_mis.troy")
                {
                    QMissile = obj;
                }
                if (obj.Name.Contains("cryo_storm"))
                {
                    RMissile = obj;
                    Program.debug("dupa");
                }
            }
        }

        private static void Obj_AI_Base_OnDelete(GameObject obj, EventArgs args)
        {
            if (obj.IsValid)
            {
                if (obj.Name == "cryo_FlashFrost_Player_mis.troy")
                    QMissile = null;
                if (obj.Name.Contains("cryo_storm"))
                    RMissile = null;
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Program.Combo && getCheckBoxItem(AniviaMenu, "AACombo"))
            {
                if (!E.IsReady())
                    PortAIO.OrbwalkerManager.SetAttack(true);

                else
                    PortAIO.OrbwalkerManager.SetAttack(false);
            }
            else
                PortAIO.OrbwalkerManager.SetAttack(true);

            if (Q.IsReady() && QMissile != null && QMissile.Position.LSCountEnemiesInRange(220) > 0)
                Q.Cast();


            if (Program.LagFree(0))
            {
                SetMana();
            }

            if (Program.LagFree(1) && R.IsReady() && getCheckBoxItem(RMenu, "autoR"))
                LogicR();

            if (Program.LagFree(2) && W.IsReady() && getCheckBoxItem(WMenu, "autoW"))
                LogicW();

            if (Program.LagFree(3) && Q.IsReady() && QMissile == null && getCheckBoxItem(QMenu, "autoQ"))
                LogicQ();

            if (Program.LagFree(4))
            {
                if (E.IsReady() && getCheckBoxItem(EMenu, "autoE"))
                    LogicE();

                Jungle();
            }
        }

        private static void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (t.LSIsValidTarget())
            {
                if (Program.Combo && Player.Mana > EMANA + QMANA - 10)
                    Program.CastSpell(Q, t);
                else if (Program.Farm && getCheckBoxItem(QMenu, "harrasQ") && getCheckBoxItem(QMenu, "haras" + t.NetworkId) && Player.Mana > RMANA + EMANA + QMANA + WMANA && OktwCommon.CanHarras())
                {
                    Program.CastSpell(Q, t);
                }
                else
                {
                    var qDmg = OktwCommon.GetKsDamage(t, Q);
                    var eDmg = E.GetDamage(t);
                    if (qDmg > t.Health)
                        Program.CastSpell(Q, t);
                    else if (qDmg + eDmg > t.Health && Player.Mana > QMANA + WMANA)
                        Program.CastSpell(Q, t);
                }
                if (!Program.None && Player.Mana > RMANA + EMANA)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.LSIsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                        Q.Cast(enemy, true);
                }
            }
        }

        private static void LogicW()
        {
            if (Program.Combo && Player.Mana > RMANA + EMANA + WMANA)
            {
                var t = TargetSelector.GetTarget(W.Range, DamageType.Magical);
                if (t.LSIsValidTarget(W.Range) && W.GetPrediction(t).CastPosition.LSDistance(t.Position) > 100)
                {
                    if (Player.Position.LSDistance(t.ServerPosition) > Player.Position.LSDistance(t.Position))
                    {
                        if (t.Position.LSDistance(Player.ServerPosition) < t.Position.LSDistance(Player.Position))
                            Program.CastSpell(W, t);
                    }
                    else
                    {
                        if (t.Position.LSDistance(Player.ServerPosition) > t.Position.LSDistance(Player.Position) && t.LSDistance(Player) < R.Range)
                            Program.CastSpell(W, t);
                    }
                }
            }
        }

        private static void LogicE()
        {
            var t = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            if (t.LSIsValidTarget())
            {
                var qCd = Q.Instance.CooldownExpires - Game.Time;
                var rCd = R.Instance.CooldownExpires - Game.Time;
                if (Player.Level < 7)
                    rCd = 10;
                //debug("Q " + qCd + "R " + rCd + "E now " + E.Instance.Cooldown);
                var eDmg = OktwCommon.GetKsDamage(t, E);

                if (eDmg > t.Health)
                    E.Cast(t, true);

                if (t.HasBuff("chilled") || qCd > E.Instance.Cooldown - 1 && rCd > E.Instance.Cooldown - 1)
                {
                    if (eDmg * 3 > t.Health)
                        E.Cast(t, true);
                    else if (Program.Combo && (t.HasBuff("chilled") || Player.Mana > RMANA + EMANA))
                    {
                        E.Cast(t, true);
                    }
                    else if (Program.Farm && Player.Mana > RMANA + EMANA + QMANA + WMANA && !Player.UnderTurret(true) && QMissile == null)
                    {
                        E.Cast(t, true);
                    }
                }
                else if (Program.Combo && R.IsReady() && Player.Mana > RMANA + EMANA && QMissile == null)
                {
                    R.Cast(t, true, true);
                }
            }
            farmE();
        }

        private static void farmE()
        {
            if (Program.LaneClear && getCheckBoxItem(FarmMenu, "farmE") && Player.Mana > QMANA + EMANA + WMANA && !Orbwalking.CanAttack() && Player.ManaPercent > getSliderItem(FarmMenu, "Mana"))
            {
                var minions = Cache.GetMinions(Player.ServerPosition, E.Range);
                foreach (var minion in minions.Where(minion => minion.Health > Player.LSGetAutoAttackDamage(minion)))
                {
                    var eDmg = E.GetDamage(minion) * 2;
                    if (minion.Health < eDmg && minion.HasBuff("chilled"))
                        E.Cast(minion);
                }
            }
        }

        private static void LogicR()
        {
            if (RMissile == null)
            {
                var t = TargetSelector.GetTarget(R.Range + 400, DamageType.Magical);
                if (t.LSIsValidTarget())
                {
                    if (R.GetDamage(t) > t.Health)
                        R.Cast(t, true, true);
                    else if (Player.Mana > RMANA + EMANA && E.GetDamage(t) * 2 + R.GetDamage(t) > t.Health)
                        R.Cast(t, true, true);
                    if (Player.Mana > RMANA + EMANA + QMANA + WMANA && Program.Combo)
                        R.Cast(t, true, true);
                }
                if (Program.LaneClear && Player.ManaPercent > getSliderItem(FarmMenu, "Mana") && getCheckBoxItem(FarmMenu, "farmR"))
                {
                    var allMinions = Cache.GetMinions(Player.ServerPosition, R.Range);
                    var farmPos = R.GetCircularFarmLocation(allMinions, Rwidth);
                    if (farmPos.MinionsHit >= getSliderItem(FarmMenu, "LCminions"))
                        R.Cast(farmPos.Position);
                }
            }
            else
            {
                if (Program.LaneClear && getCheckBoxItem(FarmMenu, "farmR"))
                {
                    var allMinions = Cache.GetMinions(RMissile.Position, Rwidth);
                    var mobs = Cache.GetMinions(RMissile.Position, Rwidth, MinionTeam.Neutral);
                    if (mobs.Count > 0)
                    {
                        if (!getCheckBoxItem(FarmMenu, "jungleR"))
                        {
                            R.Cast();
                        }
                    }
                    else if (allMinions.Count > 0)
                    {
                        if (allMinions.Count < 2 || Player.ManaPercent < getSliderItem(FarmMenu, "Mana"))
                            R.Cast();
                        else if (Player.ManaPercent < getSliderItem(FarmMenu, "Mana"))
                            R.Cast();
                    }
                    else
                        R.Cast();

                }
                else if (!Program.None && (RMissile.Position.LSCountEnemiesInRange(470) == 0 || Player.Mana < EMANA + QMANA))
                {
                    R.Cast();
                }
            }
        }

        private static void Jungle()
        {
            if (Program.LaneClear)
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, E.Range, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (Q.IsReady() && getCheckBoxItem(FarmMenu, "jungleQ"))
                    {
                        if (QMissile != null)
                        {
                            if (QMissile.Position.LSDistance(mob.ServerPosition) < 230)
                                Q.Cast();
                        }
                        else
                        {
                            Q.Cast(mob.ServerPosition);
                        }

                        return;
                    }
                    if (R.IsReady() && getCheckBoxItem(FarmMenu, "jungleR") && RMissile == null)
                    {
                        R.Cast(mob.ServerPosition);
                        return;
                    }
                    if (E.IsReady() && getCheckBoxItem(FarmMenu, "jungleE") && mob.HasBuff("chilled"))
                    {
                        E.Cast(mob);
                        return;
                    }
                    if (W.IsReady() && getCheckBoxItem(FarmMenu, "jungleW"))
                    {
                        W.Cast(mob.Position.LSExtend(Player.Position, 100));
                        return;
                    }
                }
            }
        }

        private static void SetMana()
        {
            if ((Program.getCheckBoxItem("manaDisable") && Program.Combo) || Player.HealthPercent < 20)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
                return;
            }

            QMANA = Q.Instance.SData.Mana;
            WMANA = W.Instance.SData.Mana;
            EMANA = E.Instance.SData.Mana;

            if (!R.IsReady())
                RMANA = QMANA - Player.PARRegenRate * Q.Instance.Cooldown;
            else
                RMANA = R.Instance.SData.Mana;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (getCheckBoxItem(drawMenu, "qRange"))
            {
                if (getCheckBoxItem(drawMenu, "onlyRdy"))
                {
                    if (Q.IsReady())
                        LeagueSharp.Common.Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.Cyan, 1, 1);
                }
                else
                    LeagueSharp.Common.Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.Cyan, 1, 1);
            }
            if (getCheckBoxItem(drawMenu, "wRange"))
            {
                if (getCheckBoxItem(drawMenu, "onlyRdy"))
                {
                    if (W.IsReady())
                        LeagueSharp.Common.Utility.DrawCircle(ObjectManager.Player.Position, W.Range, Color.Orange, 1, 1);
                }
                else
                    LeagueSharp.Common.Utility.DrawCircle(ObjectManager.Player.Position, W.Range, Color.Orange, 1, 1);
            }
            if (getCheckBoxItem(drawMenu, "eRange"))
            {
                if (getCheckBoxItem(drawMenu, "onlyRdy"))
                {
                    if (E.IsReady())
                        LeagueSharp.Common.Utility.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Yellow, 1, 1);
                }
                else
                    LeagueSharp.Common.Utility.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Yellow, 1, 1);
            }
            if (getCheckBoxItem(drawMenu, "rRange"))
            {
                if (getCheckBoxItem(drawMenu, "onlyRdy"))
                {
                    if (R.IsReady())
                        LeagueSharp.Common.Utility.DrawCircle(ObjectManager.Player.Position, R.Range, Color.Gray, 1, 1);
                }
                else
                    LeagueSharp.Common.Utility.DrawCircle(ObjectManager.Player.Position, R.Range, Color.Gray, 1, 1);
            }
        }
    }
}
