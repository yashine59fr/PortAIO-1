﻿using System;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using LeagueSharp.Common;
using TargetSelector = PortAIO.TSManager;

namespace KogMaw
{
    public static class Program
    {

        

        private static EloBuddy.SDK.Spell.Skillshot Q, E, R;
        private static EloBuddy.SDK.Spell.Active W;

        private static Menu Menu;

        private static readonly bool IsZombie = myHero.HasBuff("kogmawicathiansurprise");
        private static readonly bool wActive = myHero.HasBuff("kogmawbioarcanebarrage");

        private static AIHeroClient myHero
        {
            get { return Player.Instance; }
        }

        public static void OnLoad()
        {
            if (myHero.Hero != Champion.KogMaw)
            {
                return;
            }

            Menu = MainMenu.AddMenu("SharpShooter Kog", "kogmaw");
            Menu.AddLabel("Ported from SharpShooter - Berb");
            Menu.AddSeparator();
            Menu.AddGroupLabel("Combo");
            Menu.Add("useQ", new CheckBox("Use Q"));
            Menu.Add("useW", new CheckBox("Use W"));
            Menu.Add("useE", new CheckBox("Use E"));
            Menu.Add("useR", new CheckBox("Use R"));
            Menu.Add("manaW", new CheckBox("Keep Mana For W"));
            Menu.Add("dontw", new KeyBind("Don't move in combo", false, KeyBind.BindTypes.PressToggle, 'A'));
            Menu.Add("rLimit", new Slider("R stack limit", 3, 1, 6));

            Menu.Add("onlyRHP", new CheckBox("Only R if HP of target < than X"));
            Menu.Add("hpOfTarget", new Slider("HP% Of Target"));
            Menu.AddSeparator();
            Menu.AddGroupLabel("Harass");
            Menu.Add("useQH", new CheckBox("Use Q"));
            Menu.Add("useEH", new CheckBox("Use E"));
            Menu.Add("useRH", new CheckBox("Use R"));
            Menu.Add("rLimitH", new Slider("R stack limit", 1, 1, 6));
            Menu.Add("manaH", new Slider("Do Harass if mana is greater than :", 60, 1));
            Menu.AddSeparator();
            Menu.AddGroupLabel("Lane Clear");
            Menu.Add("useELC", new CheckBox("Use E"));
            Menu.Add("useRLC", new CheckBox("Use R"));
            Menu.Add("rLimitLC", new Slider("R stack limit", 1, 1, 6));
            Menu.Add("manaLC", new Slider("Do Lane Clear if mana is greater than :", 60, 1));
            Menu.AddSeparator();
            Menu.AddGroupLabel("Jungle Clear");
            Menu.Add("useWJG", new CheckBox("Use W"));
            Menu.Add("useEJG", new CheckBox("Use E"));
            Menu.Add("useRJG", new CheckBox("Use R"));
            Menu.Add("rLimitJG", new Slider("R stack limit", 2, 1, 6));
            Menu.Add("manaJG", new Slider("Do Jungle if mana is greater than :", 60, 1));
            Menu.AddSeparator();

            Q = new EloBuddy.SDK.Spell.Skillshot(SpellSlot.Q, 950, SkillShotType.Linear, 250, 1650, 70);
            W = new EloBuddy.SDK.Spell.Active(SpellSlot.W, (uint)myHero.GetAutoAttackRange());
            E = new EloBuddy.SDK.Spell.Skillshot(SpellSlot.E, 650, SkillShotType.Linear, 500, 1400, 120);
            R = new EloBuddy.SDK.Spell.Skillshot(SpellSlot.R, 1800, SkillShotType.Circular, 1200, int.MaxValue, 120);

            Drawing.OnDraw += OnDraw;
            Game.OnTick += OnTick;
            LSEvents.BeforeAttack += OnPreAttack;
        }

        public static float getSpellMana(SpellSlot spell)
        {
            return myHero.Spellbook.GetSpell(spell).SData.Mana;
        }

        public static bool QIsReadyPerfectly()
        {
            var spell = Q;
            return spell != null && spell.Slot != SpellSlot.Unknown && spell.State != SpellState.Cooldown &&
                   spell.State != SpellState.Disabled && spell.State != SpellState.NoMana &&
                   spell.State != SpellState.NotLearned && spell.State != SpellState.Surpressed &&
                   spell.State != SpellState.Unknown && spell.State == SpellState.Ready;
        }

        public static bool WIsReadyPerfectly()
        {
            var spell = W;
            return spell != null && spell.Slot != SpellSlot.Unknown && spell.State != SpellState.Cooldown &&
                   spell.State != SpellState.Disabled && spell.State != SpellState.NoMana &&
                   spell.State != SpellState.NotLearned && spell.State != SpellState.Surpressed &&
                   spell.State != SpellState.Unknown && spell.State == SpellState.Ready;
        }

        public static bool EIsReadyPerfectly()
        {
            var spell = E;
            return spell != null && spell.Slot != SpellSlot.Unknown && spell.State != SpellState.Cooldown &&
                   spell.State != SpellState.Disabled && spell.State != SpellState.NoMana &&
                   spell.State != SpellState.NotLearned && spell.State != SpellState.Surpressed &&
                   spell.State != SpellState.Unknown && spell.State == SpellState.Ready;
        }

        public static bool RIsReadyPerfectly()
        {
            var spell = R;
            return spell != null && spell.Slot != SpellSlot.Unknown && spell.State != SpellState.Cooldown &&
                   spell.State != SpellState.Disabled && spell.State != SpellState.NoMana &&
                   spell.State != SpellState.NotLearned && spell.State != SpellState.Surpressed &&
                   spell.State != SpellState.Unknown && spell.State == SpellState.Ready;
        }

        private static void OnTick(EventArgs args)
        {
            if (myHero.IsDead) return;

            W = new EloBuddy.SDK.Spell.Active(SpellSlot.W, (uint)(565 + 60 + W.Level * 30 + 65));
            R = new EloBuddy.SDK.Spell.Skillshot(SpellSlot.R, (uint)(900 + R.Level * 300), SkillShotType.Circular, 1500, int.MaxValue,
                225);

            if (PortAIO.OrbwalkerManager.CanMove(100))
            {
                if (PortAIO.OrbwalkerManager.isComboActive)
                {
                    if (useQ)
                    {
                        if (QIsReadyPerfectly())
                        {
                            if (!manaW || W.Level <= 0 || myHero.Mana - getSpellMana(SpellSlot.Q) >= getSpellMana(SpellSlot.W))
                            {
                                var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                                if (target != null)
                                {
                                    Q.Cast(target);
                                }
                            }
                        }
                    }


                    if (useW)
                    {
                        if (WIsReadyPerfectly())
                        {
                            if (EntityManager.Heroes.Enemies.Any(x => x.IsValidTarget(W.Range)))
                            {
                                W.Cast();
                            }
                        }
                    }


                    if (useE)
                    {
                        if (EIsReadyPerfectly())
                        {
                            if (!manaW || W.Level <= 0 || myHero.Mana - getSpellMana(SpellSlot.E) >= getSpellMana(SpellSlot.W))
                            {
                                var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                                if (target != null)
                                {
                                    E.Cast(target);
                                }
                            }
                        }
                    }


                    if (useR)
                    {
                        if (RIsReadyPerfectly())
                        {
                            if (!manaW || W.Level <= 0 || myHero.Mana - getSpellMana(SpellSlot.R) >= getSpellMana(SpellSlot.W))
                            {
                                if (myHero.GetBuffCount("kogmawlivingartillerycost") < rLimit)
                                {
                                    var target = TargetSelector.GetTarget(R.Range, DamageType.Magical);
                                    if (target != null)
                                    {
                                        if (onlyRHP)
                                        {
                                            if (target.HealthPercent < hpOfTarget)
                                            {
                                                R.Cast(target);
                                            }
                                        }
                                        else
                                        {
                                            R.Cast(target);
                                        }
                                    }
                                }
                                else
                                {
                                    var killableTarget = EntityManager.Heroes.Enemies.FirstOrDefault(x => x.IsKillableAndValidTarget(myHero.GetSpellDamage(x, SpellSlot.R), DamageType.Magical, R.Range) && R.GetPrediction(x).HitChance >= EloBuddy.SDK.Enumerations.HitChance.High);
                                    if (killableTarget != null)
                                    {
                                        R.Cast(killableTarget);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (PortAIO.OrbwalkerManager.isHarassActive)
                {
                    if (useQH)
                    {
                        if (QIsReadyPerfectly())
                            if (myHero.IsManaPercentOkay(manaH))
                            {
                                var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                                if (target != null)
                                    Q.Cast(target);
                            }
                    }

                    if (useEH)
                    {
                        if (EIsReadyPerfectly())
                            if (myHero.IsManaPercentOkay(manaH))
                            {
                                var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                                if (target != null)
                                    E.Cast(target);
                            }
                    }


                    if (useRH)
                    {
                        if (RIsReadyPerfectly())
                        {
                            if (myHero.IsManaPercentOkay(manaH))
                            {
                                if (myHero.GetBuffCount("kogmawlivingartillerycost") < rLimitH)
                                {
                                    var target = TargetSelector.GetTarget(R.Range, DamageType.Magical);
                                    if (target != null)
                                        R.Cast(target);
                                }
                            }
                        }
                    }
                }
                else if (PortAIO.OrbwalkerManager.isLaneClearActive)
                {
                    foreach (
                        var minion in
                            EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                                myHero.ServerPosition, myHero.GetAutoAttackRange()))
                    {
                        if (useELC)
                        {
                            if (myHero.IsManaPercentOkay(manaLC))
                            {
                                if (EIsReadyPerfectly())
                                {
                                    var minions =
                                        EntityManager.MinionsAndMonsters.GetLaneMinions(
                                            EntityManager.UnitTeam.Enemy, myHero.ServerPosition, E.Range);
                                    var farmLocation = EntityManager.MinionsAndMonsters.GetLineFarmLocation(
                                        minions, E.Width, (int)E.Range);
                                    if (farmLocation.HitNumber >= 3)
                                        E.Cast(farmLocation.CastPosition);
                                }
                            }
                        }

                        if (useRLC)
                        {
                            if (myHero.IsManaPercentOkay(manaLC))
                            {
                                if (RIsReadyPerfectly())
                                {
                                    if (myHero.GetBuffCount("kogmawlivingartillerycost") < rLimitLC)
                                    {
                                        var minions =
                                            EntityManager.MinionsAndMonsters.GetLaneMinions(
                                                EntityManager.UnitTeam.Enemy, myHero.ServerPosition, R.Range);
                                        var farmLocation =
                                            EntityManager.MinionsAndMonsters.GetCircularFarmLocation(minions,
                                                R.Width, (int)R.Range);
                                        if (farmLocation.HitNumber >= 2)
                                        {
                                            R.Cast(farmLocation.CastPosition);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (
                        var jungleMobs in
                            ObjectManager.Get<Obj_AI_Minion>()
                                .Where(
                                    o =>
                                        o.IsValidTarget(W.Range) && o.Team == GameObjectTeam.Neutral && o.IsVisible &&
                                        !o.IsDead))
                    {
                        if (WIsReadyPerfectly())
                            if (useWJG)
                                W.Cast();

                        if (useEJG)
                        {
                            if (myHero.IsManaPercentOkay(manaJG))
                            {
                                if (EIsReadyPerfectly())
                                {
                                    var minions =
                                        EntityManager.MinionsAndMonsters.GetJungleMonsters(myHero.ServerPosition,
                                            E.Range);
                                    var farmLocation = EntityManager.MinionsAndMonsters.GetLineFarmLocation(
                                        minions, E.Width, (int)E.Range);
                                    if (farmLocation.HitNumber >= 2)
                                    {
                                        E.Cast(farmLocation.CastPosition);
                                    }
                                }
                            }
                        }

                        if (useRJG)
                        {
                            if (myHero.IsManaPercentOkay(manaJG))
                            {
                                if (RIsReadyPerfectly())
                                {
                                    if (myHero.GetBuffCount("kogmawlivingartillerycost") < rLimitJG)
                                    {
                                        var minions =
                                            EntityManager.MinionsAndMonsters.GetJungleMonsters(
                                                myHero.ServerPosition, R.Range);
                                        var farmLocation =
                                            EntityManager.MinionsAndMonsters.GetCircularFarmLocation(minions,
                                                R.Width, (int)R.Range);
                                        if (farmLocation.HitNumber >= 2)
                                        {
                                            R.Cast(farmLocation.CastPosition);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static bool IsManaPercentOkay(this AIHeroClient hero, int manaPercent)
        {
            return myHero.ManaPercent > manaPercent;
        }

        internal static bool IsKillableAndValidTarget(this AIHeroClient target, double calculatedDamage,
            DamageType damageType, float distance = float.MaxValue)
        {
            if (target == null || !target.IsValidTarget(distance) || target.CharData.BaseSkinName == "gangplankbarrel")
                return false;

            if (target.HasBuff("kindredrnodeathbuff"))
            {
                return false;
            }

            if (target.HasBuff("Undying Rage"))
            {
                return false;
            }

            if (target.HasBuff("JudicatorIntervention"))
            {
                return false;
            }

            if (target.HasBuff("DiplomaticImmunity") && !myHero.HasBuff("poppyulttargetmark"))
            {
                return false;
            }

            if (target.HasBuff("BansheesVeil"))
            {
                return false;
            }

            if (target.HasBuff("SivirShield"))
            {
                return false;
            }

            if (target.HasBuff("ShroudofDarkness"))
            {
                return false;
            }

            if (myHero.HasBuff("summonerexhaust"))
                calculatedDamage *= 0.6;

            if (target.ChampionName == "Blitzcrank")
                if (!target.HasBuff("manabarriercooldown"))
                    if (target.Health + target.HPRegenRate +
                        (damageType == DamageType.Physical ? target.AttackShield : target.MagicShield) +
                        target.Mana * 0.6 + target.PARRegenRate < calculatedDamage)
                        return true;

            if (target.ChampionName == "Garen")
                if (target.HasBuff("GarenW"))
                    calculatedDamage *= 0.7;

            if (target.HasBuff("FerociousHowl"))
                calculatedDamage *= 0.3;

            return target.Health + target.HPRegenRate +
                   (damageType == DamageType.Physical ? target.AttackShield : target.MagicShield) < calculatedDamage - 2;
        }

        private static void OnDraw(EventArgs args)
        {
            if (dontw)
            {
                var target = TargetSelector.GetTarget(myHero.GetAutoAttackRange(), DamageType.Physical);
                if (PortAIO.OrbwalkerManager.isComboActive && target != null)
                {
                    PortAIO.OrbwalkerManager.SetAttack(false);
                }
                else
                {
                    PortAIO.OrbwalkerManager.SetAttack(true);
                }
            }
            else
            {
                PortAIO.OrbwalkerManager.SetAttack(true);
            }

            if (PortAIO.OrbwalkerManager.isComboActive && dontw)
            {
                Drawing.DrawText(Drawing.Width * 0.5f, Drawing.Height * 0.3f, Color.Orange, "Not moving when W is active is on.", 50);
            }
        }

        private static void OnPreAttack(BeforeAttackArgs args)
        {
            if (!args.Target.IsMe) return;

            if (IsZombie)
                args.Process = false;

            if (PortAIO.OrbwalkerManager.isComboActive)
            {
                if (WIsReadyPerfectly())
                    if (useW)
                        if (args.Target.IsValidTarget(W.Range))
                            W.Cast();
            }
            if (PortAIO.OrbwalkerManager.isLaneClearActive)
            {
                foreach (
                    var jungleMobs in
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(
                                o =>
                                    o.IsValidTarget(W.Range) && o.Team == GameObjectTeam.Neutral && o.IsVisible &&
                                    !o.IsDead))
                {
                    if (jungleMobs.BaseSkinName == "SRU_Red" || jungleMobs.BaseSkinName == "SRU_Blue" ||
                        jungleMobs.BaseSkinName == "SRU_Gromp" || jungleMobs.BaseSkinName == "SRU_Murkwolf" ||
                        jungleMobs.BaseSkinName == "SRU_Razorbeak" || jungleMobs.BaseSkinName == "SRU_Krug" ||
                        jungleMobs.BaseSkinName == "Sru_Crab")
                    {
                        if (WIsReadyPerfectly())
                            if (useWJG)
                                W.Cast();
                    }
                }
            }
        }

        #region Menu Items

        public static bool useQ
        {
            get { return Menu["useQ"].Cast<CheckBox>().CurrentValue; }
        }

        public static bool useW
        {
            get { return Menu["useW"].Cast<CheckBox>().CurrentValue; }
        }

        public static bool useE
        {
            get { return Menu["useE"].Cast<CheckBox>().CurrentValue; }
        }

        public static bool useR
        {
            get { return Menu["useR"].Cast<CheckBox>().CurrentValue; }
        }

        public static int rLimit
        {
            get { return Menu["rLimit"].Cast<Slider>().CurrentValue; }
        }

        public static int rLimitH
        {
            get { return Menu["rLimitH"].Cast<Slider>().CurrentValue; }
        }

        public static int rLimitLC
        {
            get { return Menu["rLimitLC"].Cast<Slider>().CurrentValue; }
        }

        public static int rLimitJG
        {
            get { return Menu["rLimitJG"].Cast<Slider>().CurrentValue; }
        }

        public static int manaH
        {
            get { return Menu["manaH"].Cast<Slider>().CurrentValue; }
        }

        public static int manaLC
        {
            get { return Menu["manaLC"].Cast<Slider>().CurrentValue; }
        }

        public static int manaJG
        {
            get { return Menu["manaJG"].Cast<Slider>().CurrentValue; }
        }

        public static bool manaW
        {
            get { return Menu["manaW"].Cast<CheckBox>().CurrentValue; }
        }

        public static bool useWJG
        {
            get { return Menu["useWJG"].Cast<CheckBox>().CurrentValue; }
        }

        public static bool useEJG
        {
            get { return Menu["useEJG"].Cast<CheckBox>().CurrentValue; }
        }

        public static bool useRJG
        {
            get { return Menu["useRJG"].Cast<CheckBox>().CurrentValue; }
        }

        public static bool useQH
        {
            get { return Menu["useQH"].Cast<CheckBox>().CurrentValue; }
        }

        public static bool useEH
        {
            get { return Menu["useEH"].Cast<CheckBox>().CurrentValue; }
        }

        public static bool useRH
        {
            get { return Menu["useRH"].Cast<CheckBox>().CurrentValue; }
        }

        public static bool useELC
        {
            get { return Menu["useELC"].Cast<CheckBox>().CurrentValue; }
        }

        public static bool useRLC
        {
            get { return Menu["useRLC"].Cast<CheckBox>().CurrentValue; }
        }

        public static bool dontw
        {
            get { return Menu["dontw"].Cast<KeyBind>().CurrentValue; }
        }

        public static bool onlyRHP
        {
            get { return Menu["onlyRHP"].Cast<CheckBox>().CurrentValue; }
        }

        public static int hpOfTarget
        {
            get { return Menu["hpOfTarget"].Cast<Slider>().CurrentValue; }
        }

        #endregion
    }
}