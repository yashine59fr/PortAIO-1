﻿using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using LeagueSharp.Common;

using TargetSelector = PortAIO.TSManager; namespace ThreshTherulerofthesoul
{
    class Helper
    {
        private static AIHeroClient Player { get { return ObjectManager.Player; } }

        public static AIHeroClient GetMostAD(bool IsAllyTeam, float range)
        {
            AIHeroClient MostAD = null;

            foreach (AIHeroClient hero in ObjectManager.Get<AIHeroClient>()
                .Where(x => (IsAllyTeam ? x.IsAlly : x.IsEnemy && x.LSIsValidTarget()) && 
                    !x.IsMe && !x.IsDead))
            {
                if (Player.LSDistance(hero.Position) < range)
                {
                    if (MostAD == null)
                    {
                        MostAD = hero;
                    }
                    else if (MostAD != null && MostAD.TotalAttackDamage < hero.TotalAttackDamage)
                    {
                        MostAD = hero;
                    }
                }
            }

            return MostAD;
        }

        public static IEnumerable<AIHeroClient> GetEnemiesNearTarget(AIHeroClient target)
        {
            return HeroManager.Enemies.Where(x => target.LSDistance(x.Position) < 1500 && !x.IsDead);
        }

        public static bool EnemyHasShield(AIHeroClient target)
        {
            var status = false;

            if (target.HasBuff("blackshield"))
            {
                status = true;
            }

            if (target.HasBuff("sivire"))
            {
                status = true;
            }

            if (target.HasBuff("nocturneshroudofdarkness"))
            {
                status = true;
            }

            if (target.HasBuff("bansheesveil"))
            {
                status = true;
            }
            return status;
        }

        public static double GetAlliesComboDmg(AIHeroClient target, AIHeroClient ally)
        {
            var SpellSlots = new List<SpellSlot>();
            double dmg = 0;
            #region SpellSots
            SpellSlots.Add(SpellSlot.Q);
            SpellSlots.Add(SpellSlot.W);
            SpellSlots.Add(SpellSlot.E);
            SpellSlots.Add(SpellSlot.R);
            #endregion

            foreach (var slot in SpellSlots)
            {
                var spell = ally.Spellbook.GetSpell(slot);

                dmg += ally.LSGetSpellDamage(target, slot);
                dmg += ally.LSGetAutoAttackDamage(target);
            }

            return dmg;
        }

    }
}
