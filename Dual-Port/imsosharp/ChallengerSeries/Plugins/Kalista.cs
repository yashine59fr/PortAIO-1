﻿using System;
using System.Collections.Generic;
using System.Linq;
using Challenger_Series.Utils;
using LeagueSharp.SDK;
using Color = System.Drawing.Color;
using LeagueSharp.Data.Enumerations;
using ClipperLib;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK;
using EloBuddy;
//using LeagueSharp.Common;

using TargetSelector = PortAIO.TSManager; namespace Challenger_Series.Plugins
{
    using LeagueSharp.SDK.Core.Utils;
    using LeagueSharp.SDK.Enumerations;
    using Plugins;
    using Collision = LeagueSharp.SDK.Collision;

    public class Kalista : CSPlugin
    {

        #region Spells
        public LeagueSharp.SDK.Spell Q2 { get; set; }
        public LeagueSharp.SDK.Spell W2 { get; set; }
        public LeagueSharp.Common.Spell ELS { get; set; }
        public LeagueSharp.SDK.Spell E2 { get; set; }
        public LeagueSharp.SDK.Spell R2 { get; set; }
        #endregion Spells

        public Kalista()
        {
            Q = new LeagueSharp.SDK.Spell(SpellSlot.Q, 1150f);
            W = new LeagueSharp.SDK.Spell(SpellSlot.W, 5000);
            E = new LeagueSharp.SDK.Spell(SpellSlot.E, 1000f);
            R = new LeagueSharp.SDK.Spell(SpellSlot.R, 1400f);

            ELS = new LeagueSharp.Common.Spell(SpellSlot.E, 950);

            Q.SetSkillshot(0.25f, 40f, 1200f, true, SkillshotType.SkillshotLine);
            InitMenu();
            DelayedOnUpdate += OnUpdate;
            Drawing.OnDraw += HpBarDamageIndicator.Drawing_OnDraw;
            Drawing.OnDraw += OnDraw;
            LeagueSharp.Common.LSEvents.AfterAttack += Orbwalker_OnPostAttack;
            LeagueSharp.Common.LSEvents.BeforeAttack += Orbwalker_OnPreAttack;
            Obj_AI_Base.OnProcessSpellCast += UltLogic_OnSpellcast;
            Game.OnUpdate += UltLogic_OnUpdate;
        }

        private void Orbwalker_OnPreAttack(LeagueSharp.Common.BeforeAttackArgs args)
        {
            if (FocusWBuffedEnemyBool)
            {
                PortAIO.OrbwalkerManager.ForcedTarget(
                    ValidTargets.FirstOrDefault(
                        h =>
                        h.Distance(ObjectManager.Player.ServerPosition) < 600
                        && h.HasBuff("kalistacoopstrikemarkally")));
            }
        }

        private void Orbwalker_OnPostAttack(LeagueSharp.Common.AfterAttackArgs args)
        {
            var target = args.Target;
            PortAIO.OrbwalkerManager.ForcedTarget(null);
            var t = target as Obj_AI_Base;
            if (Q.IsReady() && target.LSIsValidTarget() && !t.IsMinion)
            {
                this.QLogic(target);
                if (UseQStackTransferBool)
                {
                    this.QLogic(target);
                }
            }
        }
        

        public override void OnUpdate(EventArgs args)
        {
            if (E.IsReady()) this.ELogic();
            if (PortAIO.OrbwalkerManager.isComboActive && Q.IsReady() && PortAIO.OrbwalkerManager.CanMove(0))
            {
                foreach (var enemy in ValidTargets.Where(e => e.Distance(ObjectManager.Player) < 900))
                {
                    var pred = Q.GetPrediction(enemy);
                    if (pred.Hitchance >= LeagueSharp.SDK.Enumerations.HitChance.High && !pred.CollisionObjects.Any())
                    {
                        Q.Cast(enemy);
                    }
                }
            }

            #region Orbwalk On Minions
            if (OrbwalkOnMinions && PortAIO.OrbwalkerManager.isComboActive && ValidTargets.Count(e => e.IsInAutoAttackRange(ObjectManager.Player)) == 0)
            {
                var minion = GameObjects.EnemyMinions.Where(m => m.IsInAutoAttackRange(ObjectManager.Player)).OrderBy(m => m.Health).FirstOrDefault();
                if (minion != null)
                {
                    EloBuddy.Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                }
            }
            #endregion Orbwalk On Minions
        }

        public override void OnDraw(EventArgs args)
        {
            base.OnDraw(args);
            if (DrawERangeBool)
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.Position,
                    1000,
                    Color.LightGreen);
            }
            if (DrawRRangeBool)
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.Position,
                    1400,
                    Color.DarkRed);
            }

            if (DrawEDamage)
            {
                HpBarDamageIndicator.DamageToUnit = GetFloatRendDamage;
            }
            HpBarDamageIndicator.Enabled = DrawEDamage;
        }

        private Menu ComboMenu;
        private Menu WomboComboMenu;
        private bool BalistaBool;
        private bool TalistaBool;
        private bool SalistaBool;
        //private MenuKeyBind UseQWalljumpKey;
        private int UseQManaSlider;
        private bool FocusWBuffedEnemyBool;
        //private bool UseEBeforeYouDieBool;
        private bool UseRAllySaverBool;
        private bool UseREngageBool;
        private bool UseRCounterEngageBool;
        private bool UseRInterruptBool;
        //private bool OrbwalkOnMinionsBool;
        private Menu HarassMenu;
        private bool UseQStackTransferBool;
        private int UseQStackTransferMinStacksSlider;
        private bool UseEIfResettedByAMinionBool;
        private int EResetByAMinionMinManaSlider;
        private int MinEnemyStacksForEMinionResetSlider;
        private Menu FarmMenu;
        private bool AlwaysUseEIf2MinionsKillableBool;
        private Menu RendDamageMenu;
        private int ReduceRendDamageBySlider;
        private Menu DrawMenu;
        private bool DrawERangeBool;
        private bool DrawRRangeBool;
        private bool DrawEDamage;
        private bool OrbwalkOnMinions;

        private void InitMenu()
        {
            ComboMenu = MainMenu.AddSubMenu("Combo Settings: ");
            ComboMenu.Add("kaliorbwalkonminions", new CheckBox("Orbwalk On Minions", false));
            ComboMenu.Add("kaliuseqmanaslider", new Slider("Use Q if Mana% > ", 20));
            ComboMenu.Add("kalifocuswbuffedenemy", new CheckBox("Focus Enemy with W Buff", true));
            ComboMenu.Add("kaliusersaveally", new CheckBox("Use R to save Soulbound", true));
            ComboMenu.Add("userengage", new CheckBox("Use R to engage", false));
            ComboMenu.Add("kaliusercounternengage", new CheckBox("Use R counter-engage", true));
            ComboMenu.Add("kaliuserinterrupt", new CheckBox("Use R to Interrupt"));

            WomboComboMenu = MainMenu.AddSubMenu("Wombo Combos: ");
            WomboComboMenu.Add("Balista", new CheckBox("Balista", true));
            WomboComboMenu.Add("Talista", new CheckBox("Talista", true));
            WomboComboMenu.Add("Salista", new CheckBox("Salista", true));

            HarassMenu = MainMenu.AddSubMenu("Harass Settings: ");
            HarassMenu.Add("kaliuseqstacktransfer", new CheckBox("Use Q Stack Transfer"));
            HarassMenu.Add("kaliuseqstacktransferminstacks", new Slider("Min stacks for Stack Transfer", 3, 0, 15));
            HarassMenu.Add("useeresetharass", new CheckBox("Use E if resetted by a minion"));
            HarassMenu.Add("useeresetmana", new Slider("Use E Reset by Minion if Mana% > ", 90));
            HarassMenu.Add("useeresetminenstacks", new Slider("Use E Reset if Enemy stacks > ", 3, 0, 25));

            FarmMenu = MainMenu.AddSubMenu("Farm Settings");
            FarmMenu.Add("alwaysuseeif2minkillable", new CheckBox("Always use E if resetted with no mana cost", true));

            RendDamageMenu = MainMenu.AddSubMenu("Adjust Rend (E) DMG Prediction: ");
            RendDamageMenu.Add("kalirendreducedmg", new Slider("Reduce E DMG by: ", 0, 0, 300));

            DrawMenu = MainMenu.AddSubMenu("Drawing Settings: ");
            DrawMenu.Add("drawerangekali", new CheckBox("Draw E Range", true));
            DrawMenu.Add("kalidrawrrange", new CheckBox("Draw R Range", true));
            DrawMenu.Add("kalidrawedmg", new CheckBox("Draw E Damage", true));

            UseQStackTransferBool = HarassMenu["kaliuseqstacktransfer"].Cast<CheckBox>().CurrentValue;
            UseEIfResettedByAMinionBool = HarassMenu["useeresetharass"].Cast<CheckBox>().CurrentValue;
            UseQStackTransferMinStacksSlider = HarassMenu["kaliuseqstacktransferminstacks"].Cast<Slider>().CurrentValue;
            EResetByAMinionMinManaSlider = HarassMenu["useeresetmana"].Cast<Slider>().CurrentValue;
            MinEnemyStacksForEMinionResetSlider = HarassMenu["useeresetminenstacks"].Cast<Slider>().CurrentValue;
            OrbwalkOnMinions = ComboMenu["kaliorbwalkonminions"].Cast<CheckBox>().CurrentValue;
            UseQManaSlider = ComboMenu["kaliuseqmanaslider"].Cast<Slider>().CurrentValue;
            FocusWBuffedEnemyBool = ComboMenu["kalifocuswbuffedenemy"].Cast<CheckBox>().CurrentValue;
            UseRAllySaverBool = ComboMenu["kaliusersaveally"].Cast<CheckBox>().CurrentValue;
            UseREngageBool = ComboMenu["userengage"].Cast<CheckBox>().CurrentValue;
            UseRCounterEngageBool = ComboMenu["kaliusercounternengage"].Cast<CheckBox>().CurrentValue;
            UseRInterruptBool = ComboMenu["kaliuserinterrupt"].Cast<CheckBox>().CurrentValue;
            BalistaBool = WomboComboMenu["Balista"].Cast<CheckBox>().CurrentValue;
            TalistaBool = WomboComboMenu["Talista"].Cast<CheckBox>().CurrentValue;
            SalistaBool = WomboComboMenu["Salista"].Cast<CheckBox>().CurrentValue;
            DrawERangeBool = DrawMenu["drawerangekali"].Cast<CheckBox>().CurrentValue;
            DrawRRangeBool = DrawMenu["kalidrawrrange"].Cast<CheckBox>().CurrentValue;
            DrawEDamage = DrawMenu["kalidrawedmg"].Cast<CheckBox>().CurrentValue;
            AlwaysUseEIf2MinionsKillableBool = FarmMenu["alwaysuseeif2minkillable"].Cast<CheckBox>().CurrentValue;
            ReduceRendDamageBySlider = RendDamageMenu["kalirendreducedmg"].Cast<Slider>().CurrentValue;
        }

        #region Champion Logic
        void QLogic(AttackableUnit target = null)
        {
            if (target != null)
            {
                if (PortAIO.OrbwalkerManager.isComboActive)
                {
                    var hero = target as AIHeroClient;
                    if (hero != null)
                    {
                        if (hero.IsHPBarRendered)
                        {
                            var pred = Q.GetPrediction(hero);
                            if (pred.Hitchance >= LeagueSharp.SDK.Enumerations.HitChance.High)
                            {
                                Q.Cast(hero);
                                return;
                            }
                        }
                    }
                    else
                    {
                        foreach (var tar in ValidTargets.Where(t => t.Distance(ObjectManager.Player) < 900))
                        {
                            if (ObjectManager.Player.ManaPercent > UseQManaSlider)
                            {
                                var pred = Q.GetPrediction(tar);
                                if (pred.Hitchance >= LeagueSharp.SDK.Enumerations.HitChance.High)
                                {
                                    Q.Cast(hero);
                                    return;
                                }
                            }
                        }
                    }
                }
                if (PortAIO.OrbwalkerManager.isLaneClearActive || PortAIO.OrbwalkerManager.isHarassActive)
                {
                    var minion = target as Obj_AI_Minion;
                    if (minion != null && GetRendBuff(minion).Count >= UseQStackTransferMinStacksSlider
                        && target.Health < Q.GetDamage(minion))
                    {
                        foreach (var enemy in ValidTargets.Where(en => en.Distance(ObjectManager.Player) < 900))
                        {
                            var pred = Q.GetPrediction(enemy, false);
                            if (pred.Hitchance >= LeagueSharp.SDK.Enumerations.HitChance.High
                                && pred.CollisionObjects.All(co => co is Obj_AI_Minion && co.Health < Q.GetDamage(co))
                                && pred.CollisionObjects.Any(m => m.NetworkId == target.NetworkId))
                            {
                                Q.Cast(enemy);
                            }
                        }
                    }
                }
            }
        }

        void ELogic()
        {
            if (EntityManager.Heroes.Enemies.Any(t => IsRendKillable(t)))
            {
                E.Cast();
            }

            if (EntityManager.MinionsAndMonsters.Monsters.Any(IsRendKillable) || ObjectManager.Get<Obj_AI_Minion>().Any(m => (m.CharData.BaseSkinName.Contains("Baron") || m.CharData.BaseSkinName.Contains("Dragon") || m.CharData.BaseSkinName.Contains("Crab") || m.CharData.BaseSkinName.Contains("Herald")) && this.IsRendKillable(m)))
            {
                E.Cast();
            }

            if (AlwaysUseEIf2MinionsKillableBool && GameObjects.EnemyMinions.Count(IsRendKillable) >= 2)
            {
                E.Cast();
            }

            if (UseEIfResettedByAMinionBool && ObjectManager.Player.ManaPercent > EResetByAMinionMinManaSlider)
            {
                if (ValidTargets.Any(e => e != null ? e.Distance(ObjectManager.Player.ServerPosition) > 615 : false && GetRendBuff(e).Count >= MinEnemyStacksForEMinionResetSlider) && EntityManager.MinionsAndMonsters.EnemyMinions.Any(m => IsRendKillable(m)))
                {
                    E.Cast();
                }
            }

            if ((PortAIO.OrbwalkerManager.isLaneClearActive || PortAIO.OrbwalkerManager.isLastHitActive) &&
                GameObjects.EnemyMinions.Any(m => IsRendKillable(m) && Health.GetPrediction(m, (int)((Game.Ping / 2) + ObjectManager.Player.AttackCastDelay * 1000)) < 1 && Health.GetPrediction(m, (int)((Game.Ping / 2) + 100)) > 1))
            {
                E.Cast();
            }
        }
        #region Ult Logic

        private static AIHeroClient SoulboundAlly;
        private static Dictionary<float, float> IncomingDamageToSoulboundAlly = new Dictionary<float, float>();
        private static Dictionary<float, float> InstantDamageOnSoulboundAlly = new Dictionary<float, float>();

        public static float AllIncomingDamageToSoulbound
        {
            get
            {
                return IncomingDamageToSoulboundAlly.Sum(e => e.Value) + InstantDamageOnSoulboundAlly.Sum(e => e.Value);
            }
        }

        public void UltLogic_OnSpellcast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy)
            {
                if (R.IsReady())
                {
                    if (SoulboundAlly != null)
                    {
                        var sdata = SpellDatabase.GetByName(args.SData.Name);
                        if (UseRCounterEngageBool && sdata != null &&
                            (args.End.Distance(ObjectManager.Player.ServerPosition) < 550 || args.Target.IsMe) &&
                            sdata.SpellTags != null &&
                            sdata.SpellTags.Any(st => st == SpellTags.Dash || st == SpellTags.Blink))
                        {
                            R.Cast();
                        }
                        if (UseRInterruptBool && sdata != null && sdata.SpellTags != null &&
                            sdata.SpellTags.Any(st => st == SpellTags.Interruptable) && sender.Distance(ObjectManager.Player.ServerPosition) < sdata.Range)
                        {
                            R.Cast();
                        }
                        if (UseRAllySaverBool)
                        {
                            if (args.Target != null &&
                                args.Target.NetworkId == SoulboundAlly.NetworkId)
                            {
                                if (args.SData.ConsideredAsAutoAttack)
                                {
                                    IncomingDamageToSoulboundAlly.Add(
                                        SoulboundAlly.ServerPosition.Distance(sender.ServerPosition) /
                                        args.SData.MissileSpeed +
                                        Game.Time, (float)sender.GetAutoAttackDamage(SoulboundAlly));
                                    return;
                                }
                                if (sender is AIHeroClient)
                                {
                                    var attacker = (AIHeroClient)sender;
                                    var slot = attacker.GetSpellSlot(args.SData.Name);

                                    if (slot != SpellSlot.Unknown)
                                    {
                                        var igniteSlot = attacker.GetSpellSlot("SummonerDot");
                                        if (slot == igniteSlot && args.Target != null &&
                                            args.Target.NetworkId == SoulboundAlly.NetworkId)
                                        {
                                            InstantDamageOnSoulboundAlly.Add(Game.Time + 2,
                                                (float)
                                                    attacker.LSGetSpellDamage(SoulboundAlly,
                                                        attacker.GetSpellSlot("SummonerDot")));
                                            return;
                                        }
                                        if (slot.HasFlag(SpellSlot.Q | SpellSlot.W | SpellSlot.E | SpellSlot.R))
                                        {
                                            InstantDamageOnSoulboundAlly.Add(Game.Time + 2,
                                                (float)attacker.LSGetSpellDamage(SoulboundAlly, slot));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void UltLogic_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsRecalling() || ObjectManager.Player.InFountain())
                return;

            if (SoulboundAlly == null)
            {
                SoulboundAlly = GameObjects.AllyHeroes.FirstOrDefault(a => a.HasBuff("kalistacoopstrikeally"));
                return;
            }
            if (UseRAllySaverBool && AllIncomingDamageToSoulbound > SoulboundAlly.Health &&
                SoulboundAlly.CountEnemyHeroesInRange(800) > 0)
            {
                R.Cast();
            }
            if ((SoulboundAlly.ChampionName == "Blitzcrank" || SoulboundAlly.ChampionName == "Skarner" ||
                 SoulboundAlly.ChampionName == "TahmKench"))
            {
                foreach (
                    var unit in
                        ObjectManager.Get<AIHeroClient>()
                            .Where(
                                h =>
                                    h.IsEnemy && h.IsHPBarRendered && h.Distance(ObjectManager.Player.ServerPosition) > 700 &&
                                    h.Distance(ObjectManager.Player.ServerPosition) < 1400)
                    )
                {
                    if ((unit.HasBuff("rocketgrab2") && BalistaBool) ||
                        (unit.HasBuff("tahmkenchwdevoured") && TalistaBool) ||
                        (unit.HasBuff("skarnerimpale") && SalistaBool))
                    {
                        R.Cast();
                    }
                }
            }
            if (UseREngageBool)
            {
                foreach (var enemy in ValidTargets.Where(en => en.LSIsValidTarget(1000) && en.LSIsFacing(ObjectManager.Player)))
                {
                    var waypoints = enemy.GetWaypoints();
                    if (waypoints.LastOrDefault().Distance(ObjectManager.Player.ServerPosition) < 400)
                    {
                        R.Cast();
                    }
                }
            }
        }

        #endregion Ult Logic

        #endregion Champion Logic

        #region Damages


        private static readonly float[] RawRendDamage = { 20, 30, 40, 50, 60 };
        private static readonly float[] RawRendDamageMultiplier = { 0.6f, 0.6f, 0.6f, 0.6f, 0.6f };
        private static readonly float[] RawRendDamagePerSpear = { 10, 14, 19, 25, 32 };
        private static readonly float[] RawRendDamagePerSpearMultiplier = { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };

        /// <summary>
        /// Those buffs make the target either unkillable or a pain in the ass to kill, just wait until they end
        /// </summary>
        private List<string> UndyingBuffs = new List<string>
        {
            "JudicatorIntervention",
            "UndyingRage",
            "FerociousHowl",
            "ChronoRevive",
            "ChronoShift",
            "lissandrarself",
            "kindredrnodeathbuff"
        };

        private bool ShouldntRend(AIHeroClient target)
        {
            //Dead or not a hero
            if (target == null || !target.IsHPBarRendered) return false;
            //Undying
            if (this.UndyingBuffs.Any(buff => target.HasBuff(buff))) return true;
            //Blitzcrank
            if (target.CharData.BaseSkinName == "Blitzcrank" && !target.HasBuff("BlitzcrankManaBarrierCD")
                && !target.HasBuff("ManaBarrier"))
            {
                return true;
            }
            //SpellShield
            return target.HasBuffOfType(BuffType.SpellShield) || target.HasBuffOfType(BuffType.SpellImmunity);
        }

        private BuffInstance GetRendBuff(Obj_AI_Base target)
        {
            return target.Buffs.FirstOrDefault(b => b.Name == "kalistaexpungemarker");
        }

        private bool HasRendBuff(Obj_AI_Base target)
        {
            return this.GetRendBuff(target) != null;
        }

        private double GetTotalHealthWithShieldsApplied(Obj_AI_Base target)
        {
            var debuffer = 0f;

            /// <summary>
            ///     Gets the predicted reduction from Blitzcrank Shield.
            /// </summary>
            if (target is AIHeroClient)
            {
                if ((target as AIHeroClient).ChampionName.Equals("Blitzcrank") &&
                    !(target as AIHeroClient).HasBuff("BlitzcrankManaBarrierCD"))
                {
                    debuffer += target.Mana / 2;
                }
            }

            return target.Health +
                target.HPRegenRate +
                debuffer;
        }

        public bool IsRendKillable(Obj_AI_Base target)
        {
            // Validate unit
            if (target == null) { return false; }
            if (!HasRendBuff(target)) { return false; }
            if (target is AIHeroClient && target.Health > 1)
            {
                if (ShouldntRend((AIHeroClient)target)) return false;
            }

            // Take into account all kinds of shields

            var dmg = EDamage(target) - ReduceRendDamageBySlider;

            return dmg >= GetTotalHealthWithShieldsApplied(target);
        }

        private float EDamage(Obj_AI_Base target)
        {
            if ((target.IsMinion || target.IsMonster) && !(target is AIHeroClient))
            {
                int stacksMin = GetMinionStacks(target);

                var EDamageMinion = new float[] {20, 30, 40, 50, 60 }[ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level - 1] + (0.6 * ObjectManager.Player.FlatPhysicalDamageMod);

                if (stacksMin > 1)
                {
                    EDamageMinion += ((new float[] {10, 14, 19, 25, 32 }[ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level - 1] + (new float[] {0.2f, 0.225f, 0.25f, 0.275f, 0.3f }[ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level - 1] * ObjectManager.Player.FlatPhysicalDamageMod)) * (stacksMin - 1));
                }

                return (float)ObjectManager.Player.CalculateDamage(target, DamageType.Physical, EDamageMinion) * 0.9f;
            }
            if (target is AIHeroClient)
            {
                if (GetStacks(target) == 0) return 0;

                int stacksChamps = GetStacks(target);

                var EDamageChamp = new[] {20, 30, 40, 50, 60 }[ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level - 1] + (0.6 * ObjectManager.Player.FlatPhysicalDamageMod);

                if (stacksChamps > 1)
                {
                    EDamageChamp += ((new[] {10, 14, 19, 25, 32 }[ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level - 1] + (new[] {0.2, 0.225, 0.25, 0.275, 0.3 }[ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level - 1] * ObjectManager.Player.FlatPhysicalDamageMod)) * (stacksChamps - 1));
                }

                return (float)ObjectManager.Player.CalculateDamage(target, DamageType.Physical, EDamageChamp);
            }
            return 0;
        }

        private int GetMinionStacks(Obj_AI_Base minion)
        {
            int stacks = 0;
            foreach (var rendbuff in minion.Buffs.Where(x => x.Name.ToLower().Contains("kalistaexpungemarker")))
            {
                stacks = rendbuff.Count;
            }

            if (stacks == 0 || !minion.HasBuff("kalistaexpungemarker")) return 0;
            return stacks;
        }

        private int GetStacks(Obj_AI_Base target)
        {
            int stacks = 0;

            if (target.HasBuff("kalistaexpungemarker"))
            {
                foreach (var rendbuff in target.Buffs.Where(x => x.Name.ToLower().Contains("kalistaexpungemarker")))
                {
                    stacks = rendbuff.Count;
                }
            }
            else
            {
                return 0;
            }
            return stacks;
        }

        public float GetFloatRendDamage(Obj_AI_Base target)
        {
            return (float)GetRendDamage(target, -1);
        }
        public double GetRendDamage(Obj_AI_Base target)
        {
            return GetRendDamage(target, -1);
        }

        public double GetRendDamage(Obj_AI_Base target, int customStacks = -1, BuffInstance rendBuff = null)
        {
            // Calculate the damage and return
            return (float)ObjectManager.Player.CalculateDamage(target, DamageType.Physical, GetRawRendDamage(target, customStacks, rendBuff));
        }

        public float GetRawRendDamage(Obj_AI_Base target, int customStacks = -1, BuffInstance rendBuff = null)
        {
            rendBuff = rendBuff ?? GetRendBuff(target);
            var stacks = (customStacks > -1 ? customStacks : rendBuff != null ? rendBuff.Count : 0) - 1;
            if (stacks > -1)
            {
                return RawRendDamage[ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level-1] + stacks * RawRendDamagePerSpear[ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level-1] +
                       ObjectManager.Player.FlatPhysicalDamageMod * (RawRendDamageMultiplier[ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level-1] + stacks * RawRendDamagePerSpearMultiplier[ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level-1]);
            }

            return 0;
        }
        #endregion

    }
}