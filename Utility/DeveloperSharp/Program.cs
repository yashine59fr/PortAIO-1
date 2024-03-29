﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
using SharpDX;
using EloBuddy;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

using TargetSelector = PortAIO.TSManager; namespace DeveloperSharp
{
    class Program
    {
        private static Menu Config;
        private static int _lastMovementTick = 0;
        public static void Init()
        {
            CustomEvents.Game.OnGameLoad += sD =>
            {
                InitMenu();
                Game.OnUpdate += OnUpdate;
                Drawing.OnDraw += OnDraw;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            };
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                //Game.PrintChat("Detected Spell Name: " + args.SData.Name + " Missile Name: " + args.SData.MissileBoneName + " Issued By: " + sender.CharData.BaseSkinName);
            }
        }

        private static void InitMenu()
        {
            Config = MainMenu.AddMenu("Developer#", "developersharp");
            Config.Add("range", new Slider("Max object dist from cursor", 400, 100, 1000));
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Environment.TickCount - _lastMovementTick > 140000)
            {
                EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, ObjectManager.Player.Position.Randomize(-1000, 1000));
                _lastMovementTick = Environment.TickCount;
            }
        }

        private static void OnDraw(EventArgs args)
        {
            #pragma warning disable 618
            foreach (var obj in ObjectManager.Get<GameObject>().Where(o => o.Position.LSDistance(Game.CursorPos) < Config["range"].Cast<Slider>().CurrentValue && !(o is Obj_Turret) && o.Name != "missile" && !(o is Obj_LampBulb) && !(o is Obj_SpellMissile) && !(o is GrassObject) && !(o is DrawFX) && !(o is LevelPropSpawnerPoint) && !(o is Obj_GeneralParticleEmitter) && !o.Name.Contains("MoveTo")))
            {
                if (!obj.IsValid) return;
                var X = Drawing.WorldToScreen(obj.Position).X;
                var Y = Drawing.WorldToScreen(obj.Position).Y;
                Drawing.DrawText(X, Y, Color.DarkTurquoise, (obj is AIHeroClient) ? ((AIHeroClient)obj).CharData.BaseSkinName : (obj is Obj_AI_Minion) ? (obj as Obj_AI_Minion).CharData.BaseSkinName : (obj is Obj_AI_Turret) ? (obj as Obj_AI_Turret).CharData.BaseSkinName : obj.Name);
                Drawing.DrawText(X, Y + 10, Color.DarkTurquoise, obj.Type.ToString());
                Drawing.DrawText(X, Y + 20, Color.DarkTurquoise, "NetworkID: " + obj.NetworkId);
                Drawing.DrawText(X, Y + 30, Color.DarkTurquoise, obj.Position.ToString());
                if (obj is Obj_AI_Base)
                {
                    var aiobj = obj as Obj_AI_Base;
                    Drawing.DrawText(X, Y + 40, Color.DarkTurquoise, "Health: " + aiobj.Health + "/" + aiobj.MaxHealth + "(" + aiobj.HealthPercent + "%)");
                }
                if (obj is AIHeroClient)
                {
                    var hero = obj as AIHeroClient;
                    Drawing.DrawText(X, Y + 50, Color.DarkTurquoise, "Spells:");
                    Drawing.DrawText(X, Y + 60, Color.DarkTurquoise, "(Q): " + hero.Spellbook.Spells[0].Name);
                    Drawing.DrawText(X, Y + 70, Color.DarkTurquoise, "(W): " + hero.Spellbook.Spells[1].Name);
                    Drawing.DrawText(X, Y + 80, Color.DarkTurquoise, "(E): " + hero.Spellbook.Spells[2].Name);
                    Drawing.DrawText(X, Y + 90, Color.DarkTurquoise, "(R): " + hero.Spellbook.Spells[3].Name);
                    Drawing.DrawText(X, Y + 100, Color.DarkTurquoise, "(D): " + hero.Spellbook.Spells[4].Name);
                    Drawing.DrawText(X, Y + 110, Color.DarkTurquoise, "(F): " + hero.Spellbook.Spells[5].Name);
                    var buffs = hero.Buffs;
                    if (buffs.Any())
                    {
                        Drawing.DrawText(X, Y + 120, Color.DarkTurquoise, "Buffs:");
                    }
                    for (var i = 0; i < buffs.Count() * 10; i += 10)
                    {
                        Drawing.DrawText(X, (Y + 130 + i), Color.DarkTurquoise, buffs[i / 10].Count + "x " + buffs[i / 10].Name);
                    }

                }
                if (obj is Obj_SpellMissile)
                {
                    var missile = obj as Obj_SpellMissile;
                    Drawing.DrawText(X, Y + 40, Color.DarkTurquoise, "Missile Speed: " + missile.SData.MissileSpeed);
                    Drawing.DrawText(X, Y + 50, Color.DarkTurquoise, "Cast Range: " + missile.SData.CastRange);
                }

                if (obj is MissileClient && obj.Name != "missile")
                {
                    var missile = obj as MissileClient;
                    Drawing.DrawText(X, Y + 40, Color.DarkTurquoise, "Missile Speed: " + missile.SData.MissileSpeed);
                    Drawing.DrawText(X, Y + 50, Color.DarkTurquoise, "Cast Range: " + missile.SData.CastRange);
                }
            }
        }
    }
}