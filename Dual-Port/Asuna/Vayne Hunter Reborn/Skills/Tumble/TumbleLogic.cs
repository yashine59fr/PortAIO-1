﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.Modules.ModuleHelpers;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.Helpers;
using VayneHunter_Reborn.Utility.MenuUtility;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

using TargetSelector = PortAIO.TSManager; namespace VayneHunter_Reborn.Skills.Tumble
{
    class TumbleLogic
    {
        private static float LastCondemnTick = 0f;

        private static LeagueSharp.Common.Spell Q
        {
            get { return Variables.spells[SpellSlot.Q]; }
        }

        public static void OnLoad()
        {
            Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnDoCast;
        }

        private static void Obj_AI_Base_OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && Orbwalking.IsAutoAttack(args.SData.Name) && (args.Target is Obj_AI_Base))
            {
                if (Environment.TickCount - Variables.LastCondemnFlashTime < 250)
                {
                    return;
                }

                ExecuteAALogic(sender, (Obj_AI_Base) args.Target);
            }

            if (sender.IsMe && args.Slot == SpellSlot.E)
            {
                LastCondemnTick = Environment.TickCount;
            }
        }
        
        private static void ExecuteAALogic(Obj_AI_Base sender, Obj_AI_Base target)
        {
            var QEnabled = Q.IsEnabledAndReady(PortAIO.OrbwalkerManager.GetActiveMode());
            
            if (QEnabled)
            {
                if (PortAIO.OrbwalkerManager.isComboActive)
                {
                    TumbleMethods.PreCastTumble(target);
                }

                if (PortAIO.OrbwalkerManager.isHarassActive)
                {
                    if (target is AIHeroClient)
                    {
                        TumbleMethods.PreCastTumble(target);
                    }
                }

                if (PortAIO.OrbwalkerManager.isLaneClearActive || PortAIO.OrbwalkerManager.isLastHitActive)
                {
                    TumbleMethods.HandleFarmTumble(target);
                }
            }

            if (MenuGenerator.harassMenu["dz191.vhr.mixed.ethird"].Cast<CheckBox>().CurrentValue)
            {
                if (target is AIHeroClient)
                {
                    var tg = target as AIHeroClient;
                    if (PortAIO.OrbwalkerManager.isHarassActive && tg.GetWBuff() != null && tg.GetWBuff().Count == 1 && tg.LSIsValidTarget(Variables.spells[SpellSlot.E].Range))
                    {
                        Variables.spells[SpellSlot.E].CastOnUnit(tg);
                    }
                }
            }

            foreach (var module in Variables.moduleList.Where(module => module.GetModuleType() == ModuleType.OnAfterAA
                && module.ShouldGetExecuted()))
            {
                module.OnExecute();
            }
        }

        
    }
}
