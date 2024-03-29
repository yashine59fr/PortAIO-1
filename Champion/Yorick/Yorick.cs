﻿using System;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using LeagueSharp.Common;
using UnderratedAIO.Helpers;
using Damage = LeagueSharp.Common.Damage;
using Environment = UnderratedAIO.Helpers.Environment;
using Prediction = LeagueSharp.Common.Prediction;
using Spell = LeagueSharp.Common.Spell;
using Utility = LeagueSharp.Common.Utility;

using TargetSelector = PortAIO.TSManager; namespace UnderratedAIO.Champions
{
    internal class Yorick
    {
        public static Menu config;
        public static readonly AIHeroClient player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static bool hasGhost = false;
        public static bool GhostDelay;
        public static int GhostRange = 2200;
        public static int LastAATick;
        public static Menu menuD, menuC, menuH, menuLC, menuM;

        public Yorick()
        {
            InitYorick();
            InitMenu();
            Game.OnUpdate += Game_OnGameUpdate;
            LSEvents.AfterAttack += AfterAttack;
            LSEvents.BeforeAttack += beforeAttack;
            Drawing.OnDraw += Game_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static bool Yorickghost
        {
            get { return player.Spellbook.GetSpell(SpellSlot.R).Name == "YorickReviveAllyGuide"; }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base hero, GameObjectProcessSpellCastEventArgs args)
        {
            if (Yorickghost)
            {
                if (args == null || clone == null)
                {
                    return;
                }
                if (hero.NetworkId != clone.NetworkId)
                {
                    return;
                }
                LastAATick = Utils.GameTimeTickCount;
            }
        }
        
        private void Game_OnGameUpdate(EventArgs args)
        {
            if (PortAIO.OrbwalkerManager.isComboActive)
            {
                Combo();
            }

            if (PortAIO.OrbwalkerManager.isHarassActive)
            {
                Harass();
            }

            if (PortAIO.OrbwalkerManager.isLaneClearActive)
            {
                Clear();
            }

            if (R.IsReady() && Yorickghost && !GhostDelay && getCheckBoxItem(menuM, "autoMoveGhost"))
            {
                moveGhost();
            }

            clone = (Obj_AI_Base)ObjectManager.Player.Pet;
            if (clone != null && !clone.IsValid)
            {
                clone = null;
            }
        }

        public static Obj_AI_Base clone;

        private void moveGhost()
        {
            var Gtarget = TargetSelector.GetTarget(GhostRange, DamageType.Magical);
            switch (getBoxItem(menuM, "ghostTarget"))
            {
                case 0:
                    Gtarget = TargetSelector.GetTarget(R.Range, DamageType.Magical);
                    break;
                case 1:
                    Gtarget =
                        HeroManager.Enemies.Where(i => player.LSDistance(i) <= 1500)
                            .OrderBy(i => i.Health)
                            .FirstOrDefault();
                    break;
                case 2:
                    Gtarget =
                        HeroManager.Enemies.Where(i => player.LSDistance(i) <= 1500)
                            .OrderBy(i => player.LSDistance(i))
                            .FirstOrDefault();
                    break;
                default:
                    break;
            }
            if (clone != null && Gtarget != null && Gtarget.IsValid && !clone.Spellbook.IsAutoAttacking)
            {
                R.CastOnUnit(Gtarget, getCheckBoxItem(config, "packets"));
                GhostDelay = true;
                Utility.DelayAction.Add(500, () => GhostDelay = false);
            }
        }

        private static void AfterAttack(AfterAttackArgs args)
        {
            var nearestMob = Jungle.GetNearest(player.Position);
            if (Q.IsReady() &&
                ((PortAIO.OrbwalkerManager.isComboActive && getCheckBoxItem(menuC, "useq") &&
                  args.Target is AIHeroClient) ||
                 (getCheckBoxItem(menuLC, "useqLC") && nearestMob != null &&
                   nearestMob.LSDistance(player.Position) < player.AttackRange + 30)))
            {
                Q.Cast(getCheckBoxItem(config, "packets"));
                PortAIO.OrbwalkerManager.ResetAutoAttackTimer();
            }
        }

        private void beforeAttack(BeforeAttackArgs args)
        {
            if (Q.IsReady() &&
                (PortAIO.OrbwalkerManager.isLaneClearActive) &&
                getCheckBoxItem(menuLC, "useqLC") && !(args.Target is AIHeroClient) && (args.Target.Health > 700))
            {
                Q.Cast(getCheckBoxItem(config, "packets"));
                Player.IssueOrder(GameObjectOrder.AutoAttack, args.Target);
            }
        }

        private void Combo()
        {
            AIHeroClient target = TargetSelector.GetTarget(1500, DamageType.Physical);

            if (target == null)
            {
                return;
            }
            var combodmg = ComboDamage(target);

            var hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerDot")) == SpellState.Ready;
            if (getCheckBoxItem(menuC, "usew") && W.CanCast(target))
            {
                W.Cast(target.Position, getCheckBoxItem(config, "packets"));
            }

            if (getCheckBoxItem(menuC, "usee") && E.CanCast(target))
            {
                E.CastOnUnit(target, getCheckBoxItem(config, "packets"));
            }
            
            var ally =
                HeroManager.Allies.Where(
                    i =>
                        !i.IsDead &&
                        ((i.Health * 100 / i.MaxHealth) <= menuC["atpercenty"].Cast<Slider>().CurrentValue ||
                         Program.IncDamages.GetAllyData(i.NetworkId).IsAboutToDie) && player.LSDistance(i) < R.Range &&
                        !menuM["ulty" + i.NetworkId].Cast<CheckBox>().CurrentValue && i.CountEnemiesInRange(750) > 0)
                    .OrderByDescending(i => Environment.Hero.GetAdOverTime(player, i, 5))
                    .FirstOrDefault();

            if (!Yorickghost && ally != null && getCheckBoxItem(menuC, "user") && R.IsInRange(ally) && R.IsReady())
            {
                R.Cast(ally, getCheckBoxItem(config, "packets"));
            }
            if (getCheckBoxItem(menuC, "useIgnite") && combodmg > target.Health && hasIgnite)
            {
                player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
            }

            if (R.IsReady() && Yorickghost && !GhostDelay && getCheckBoxItem(menuC, "moveGhost") &&
                !getCheckBoxItem(menuM, "autoMoveGhost"))
            {
                moveGhost();
            }
        }

        private void Harass()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            if (target == null)
            {
                return;
            }
            if (getCheckBoxItem(menuH, "usewH") && W.CanCast(target))
            {
                W.Cast(target, getCheckBoxItem(config, "packets"));
            }
            if (getCheckBoxItem(menuH, "useeH") && E.CanCast(target))
            {
                E.CastOnUnit(target, getCheckBoxItem(config, "packets"));
            }
        }

        private void Clear()
        {
            var perc = getSliderItem(menuLC, "minmana")/100f;
            if (player.Mana < player.MaxMana*perc)
            {
                return;
            }
            var bestpos =
                W.GetCircularFarmLocation(
                    MinionManager.GetMinions(W.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth),
                    100);
            if (getCheckBoxItem(menuLC, "usewLC") && W.IsReady() &&
                getSliderItem(menuLC, "usewLChit") <= bestpos.MinionsHit)
            {
                W.Cast(bestpos.Position, getCheckBoxItem(config, "packets"));
            }
            var target =
                MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.NotAlly)
                    .Where(i => i.Health < E.GetDamage(i) || i.Health > 600f)
                    .OrderByDescending(i => i.LSDistance(player))
                    .FirstOrDefault();
            if (getCheckBoxItem(menuLC, "useeLC") && E.CanCast(target))
            {
                E.CastOnUnit(target, getCheckBoxItem(config, "packets"));
            }
            if (getCheckBoxItem(menuLC, "useqLC") && Q.IsReady())
            {
                var targetQ =
                    MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly)
                        .Where(
                            i =>
                                (i.Health < Damage.LSGetSpellDamage(player, i, SpellSlot.Q) &&
                                 !(i.Health < player.GetAutoAttackDamage(i))))
                        .OrderByDescending(i => i.Health)
                        .FirstOrDefault();
                if (targetQ == null)
                {
                    return;
                }
                Q.Cast(getCheckBoxItem(config, "packets"));
                Player.IssueOrder(GameObjectOrder.AutoAttack, targetQ);
            }
        }

        private void Game_OnDraw(EventArgs args)
        {
            DrawHelper.DrawCircle(getCheckBoxItem(menuD, "drawaa"), player.AttackRange, Color.FromArgb(180, 116, 99, 45));
            DrawHelper.DrawCircle(getCheckBoxItem(menuD, "drawww"), W.Range, Color.FromArgb(180, 116, 99, 45));
            DrawHelper.DrawCircle(getCheckBoxItem(menuD, "drawee"), E.Range, Color.FromArgb(180, 116, 99, 45));
            DrawHelper.DrawCircle(getCheckBoxItem(menuD, "drawrr"), R.Range, Color.FromArgb(180, 116, 99, 45));
        }

        private float ComboDamage(AIHeroClient hero)
        {
            double damage = 0;
            if (W.IsReady())
            {
                damage += player.LSGetSpellDamage(hero, SpellSlot.W);
            }
            if (Q.IsReady())
            {
                damage += player.LSGetSpellDamage(hero, SpellSlot.Q);
            }
            if (E.IsReady())
            {
                damage += player.LSGetSpellDamage(hero, SpellSlot.E);
            }
            if (R.IsReady())
            {
                damage += player.LSGetSpellDamage(hero, SpellSlot.R);
            }

            var ignitedmg = player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            if (player.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready &&
                hero.Health < damage + ignitedmg)
            {
                damage += ignitedmg;
            }
            return (float) damage;
        }

        private void InitYorick()
        {
            Q = new Spell(SpellSlot.Q, player.AttackRange);
            W = new Spell(SpellSlot.W, 600);
            W.SetSkillshot(W.Instance.SData.SpellCastTime, W.Instance.SData.LineWidth, W.Speed, false,
                SkillshotType.SkillshotCircle);
            E = new Spell(SpellSlot.E, 550);
            R = new Spell(SpellSlot.R, 850);
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

        private void InitMenu()
        {
            config = MainMenu.AddMenu("Yorick", "Yorick");

            // Draw settings
            menuD = config.AddSubMenu("Drawings ", "dsettings");
            menuD.Add("drawaa", new CheckBox("Draw AA range"));
                //.SetValue(new Circle(false, Color.FromArgb(180, 116, 99, 45)));
            menuD.Add("drawqq", new CheckBox("Draw Q range"));
                //.SetValue(new Circle(false, Color.FromArgb(180, 116, 99, 45)));
            menuD.Add("drawww", new CheckBox("Draw W range"));
                //.SetValue(new Circle(false, Color.FromArgb(180, 116, 99, 45)));
            menuD.Add("drawee", new CheckBox("Draw E range"));
                //.SetValue(new Circle(false, Color.FromArgb(180, 116, 99, 45)));
            menuD.Add("drawrr", new CheckBox("Draw R range"));
                //.SetValue(new Circle(false, Color.FromArgb(180, 116, 99, 45)));

            // Combo Settings
            menuC = config.AddSubMenu("Combo ", "csettings");
            menuC.Add("useq", new CheckBox("Use Q"));
            menuC.Add("usew", new CheckBox("Use W"));
            menuC.Add("usee", new CheckBox("Use E"));
            menuC.Add("user", new CheckBox("Use R"));
            menuC.Add("moveGhost", new CheckBox("Move ghost"));
            menuC.Add("atpercenty", new Slider("Ult friend under", 30));
            menuC.Add("useIgnite", new CheckBox("Use Ignite"));

            // Harass Settings
            menuH = config.AddSubMenu("Harass ", "Hsettings");
            menuH.Add("usewH", new CheckBox("Use W"));
            menuH.Add("useeH", new CheckBox("Use E"));

            // LaneClear Settings
            menuLC = config.AddSubMenu("LaneClear ", "Lcsettings");
            menuLC.Add("useqLC", new CheckBox("Use Q"));
            menuLC.Add("usewLC", new CheckBox("Use W"));
            menuLC.Add("usewLChit", new Slider("Min hit", 3, 1, 8));
            menuLC.Add("useeLC", new CheckBox("Use E"));
            menuLC.Add("minmana", new Slider("Keep X% mana", 1, 1));

            // Misc Settings
            menuM = config.AddSubMenu("Misc ", "Msettings");
            menuM.Add("autoMoveGhost", new CheckBox("Always move ghost", false));
            menuM.Add("ghostTarget", new ComboBox("Ghost target priority", 0, "Targetselector", "Lowest health", "Closest to you"));
            menuM.AddGroupLabel("Don't ult on");
            foreach (var hero in ObjectManager.Get<AIHeroClient>().Where(hero => hero.IsAlly))
            {
                menuM.Add("ulty" + hero.NetworkId, new CheckBox(hero.BaseSkinName, false));
            }

            config.Add("packets", new CheckBox("Use Packets", false));
        }
    }
}