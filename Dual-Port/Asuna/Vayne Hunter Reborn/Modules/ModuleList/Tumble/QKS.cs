﻿using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.Modules.ModuleHelpers;
using VayneHunter_Reborn.Skills.Tumble;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.MenuUtility;

using TargetSelector = PortAIO.TSManager; namespace VayneHunter_Reborn.Modules.ModuleList.Tumble
{
    class QKS : IModule
    {
        public void OnLoad()
        {

        }

        public bool ShouldGetExecuted()
        {
            return MenuGenerator.miscMenu["dz191.vhr.misc.tumble.qinrange"].Cast<CheckBox>().CurrentValue && Variables.spells[SpellSlot.Q].IsReady();
        }

        public ModuleType GetModuleType()
        {
            return ModuleType.OnUpdate;
        }
        
        public void OnExecute()
        {
            var currentTarget = TargetSelector.GetTarget(Orbwalking.GetRealAutoAttackRange(null) + 240f, DamageType.Physical);
            if (!currentTarget.LSIsValidTarget())
            {
                PortAIO.OrbwalkerManager.ForcedTarget(null);
                return;
            }

            if (currentTarget.ServerPosition.LSDistance(ObjectManager.Player.ServerPosition) <= Orbwalking.GetRealAutoAttackRange(null))
            {
                PortAIO.OrbwalkerManager.ForcedTarget(null);
                return;
            }

            if (HealthPrediction.GetHealthPrediction(currentTarget, (int)(250 + Game.Ping / 2f)) <
                ObjectManager.Player.GetAutoAttackDamage(currentTarget) +
                Variables.spells[SpellSlot.Q].GetDamage(currentTarget)
                && HealthPrediction.GetHealthPrediction(currentTarget, (int)(250 + Game.Ping / 2f)) > 0)
            {
                var extendedPosition = ObjectManager.Player.ServerPosition.LSExtend(
                    currentTarget.ServerPosition, 300f);
                if (extendedPosition.IsSafe())
                {
                    PortAIO.OrbwalkerManager.ResetAutoAttackTimer();
                    Variables.spells[SpellSlot.Q].Cast(extendedPosition);
                    PortAIO.OrbwalkerManager.ForcedTarget(currentTarget);
                }
            }
        }
    }
}
