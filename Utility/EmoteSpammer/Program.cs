﻿#region
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;
using EloBuddy.SDK.Menu;
using EloBuddy;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK;
#endregion

using TargetSelector = PortAIO.TSManager; namespace EmoteSpammer
{
    internal class Program
    {
        private static Menu Config;
        public static int tick;

        public static void Game_OnGameLoad()
        {
            Config = MainMenu.AddMenu("EmoteSpammer", "EmoteSpammer");
            Config.Add("EmotePress", new KeyBind("Emote On Key press", false, KeyBind.BindTypes.HoldActive, 32));
            Config.Add("EmoteToggable", new KeyBind("Toggleable Emote", false, KeyBind.BindTypes.PressToggle, 'H'));
            Config.Add("Type", new ComboBox("Which Emote to spam?", 0, "Laugh", "Taunt", "Joke", "Dance"));
            Config.Add("delay", new Slider("Delay", 0, 0, 1000));

            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.HasBuff("Recall")) return;
            {
                if (Core.GameTickCount - tick >= Config["delay"].Cast<Slider>().CurrentValue)
                {
                    if (Config["EmotePress"].Cast<KeyBind>().CurrentValue)
                    {
                        SPAM();
                    }
                    if (Config["EmoteToggable"].Cast<KeyBind>().CurrentValue)
                    {
                        SPAM();
                    }
                }
            }
        }

        private static void SPAM()
        {
            if (Config["Type"].Cast<ComboBox>().CurrentValue == 0)
            {
                tick = Core.GameTickCount;
                Player.DoEmote(Emote.Laugh);
            }
            if (Config["Type"].Cast<ComboBox>().CurrentValue == 1)
            {
                tick = Core.GameTickCount;
                Player.DoEmote(Emote.Taunt);
            }
            if (Config["Type"].Cast<ComboBox>().CurrentValue == 2)
            {
                tick = Core.GameTickCount;
                Player.DoEmote(Emote.Joke);
            }
            if (Config["Type"].Cast<ComboBox>().CurrentValue == 3)
            {
                tick = Core.GameTickCount;
                Player.DoEmote(Emote.Dance);
            }
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos, false);
        }
    }
}
