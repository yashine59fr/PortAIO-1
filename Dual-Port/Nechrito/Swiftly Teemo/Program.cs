﻿#region

using System;
using System.Linq;
using EloBuddy;
using LeagueSharp.SDK;
using SharpDX;
using Swiftly_Teemo.Draw;
using Swiftly_Teemo.Handler;
using Swiftly_Teemo.Main;
using EloBuddy.SDK;

#endregion

using TargetSelector = PortAIO.TSManager; namespace Swiftly_Teemo
{
    internal class Program : Core
    {

        public static void Load()
        {
            if (GameObjects.Player.ChampionName != "Teemo")
            {
                return;
            }
            Chat.Print("<b><font color=\"#FFFFFF\">[</font></b><b><font color=\"#00e5e5\">Swiftly Teemo</font></b><b><font color=\"#FFFFFF\">]</font></b><b><font color=\"#FFFFFF\"> Version: 4</font></b>");
            Chat.Print("<b><font color=\"#FFFFFF\">[</font></b><b><font color=\"#00e5e5\">Update</font></b><b><font color=\"#FFFFFF\">]</font></b><b><font color=\"#FFFFFF\"> TowerCheck & R Draw</font></b>");

             Spells.Load();
             MenuConfig.Load();

            Drawing.OnDraw += Drawings.OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            LeagueSharp.Common.LSEvents.AfterAttack += AfterAa.Orbwalker_OnPostAttack;
            Spellbook.OnCastSpell += Mode.OnCastSpell;
            Game.OnUpdate += OnUpdate;
        }
        
        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.LSIsRecalling())
            {
                return;
            }
            Killsteal.KillSteal();

            if (PortAIO.OrbwalkerManager.isComboActive)
            {
                Mode.Combo();
            }
            if (PortAIO.OrbwalkerManager.isLaneClearActive)
            {
                Mode.Lane();
                Mode.Jungle();
            }

            Mode.Flee();
        }


        private static void Drawing_OnEndScene(EventArgs args)
        {
            foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(ene => ene.LSIsValidTarget() && ene.LSIsValidTarget(1000) && !ene.IsZombie))
            {
                if (!MenuConfig.Dind) continue;

                var easyKill = Spells.Q.IsReady() && Dmg.IsLethal(enemy) ? new ColorBGRA(0, 255, 0, 120) : new ColorBGRA(255, 255, 0, 120);

                Drawings.DrawHpBar.unit = enemy;
                Drawings.DrawHpBar.drawDmg(Dmg.ComboDmg(enemy), easyKill);
            }
         }
      }
   }

