﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = SharpDX.Color;
using EloBuddy;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

using TargetSelector = PortAIO.TSManager; namespace OlafxQx.Common
{
    internal class EnemyHeros
    {
        public AIHeroClient Player;
        public int LastSeen;
        public int LastSeenForE;


        public EnemyHeros(AIHeroClient player)
        {
            Player = player;
        }
    }

    internal class AutoBushHelper
    {
        public static List<EnemyHeros> EnemyInfo = new List<EnemyHeros>();

        public AutoBushHelper()
        {
            EnemyInfo = HeroManager.Enemies.Select(e => new EnemyHeros(e)).ToList();

            //foreach (var e in EnemyInfo)
            //{
            //    e.LastSeen = 0;
            //    e.LastSeenForE = 0;
            //}

            Game.OnUpdate += Game_OnGameUpdate;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            foreach (EnemyHeros e in EnemyInfo)
            {
                if (e.Player.IsVisible)
                {
                    e.LastSeen = Environment.TickCount;
                }

                if (!e.Player.IsVisible || e.Player.IsDead)
                {
                    e.LastSeenForE = Environment.TickCount;
                }
            }
        }

        public EnemyHeros GetPlayerInfo(AIHeroClient enemy)
        {
            return EnemyInfo.Find(x => x.Player.NetworkId == enemy.NetworkId);
        }

        public float GetTargetHealth(EnemyHeros playerHeros, int additionalTime)
        {
            if (playerHeros.Player.IsVisible) return playerHeros.Player.Health;

            var predictedhealth = playerHeros.Player.Health
                                  + playerHeros.Player.HPRegenRate
                                  * ((Environment.TickCount - playerHeros.LastSeen + additionalTime) / 1000f);

            return predictedhealth > playerHeros.Player.MaxHealth ? playerHeros.Player.MaxHealth : predictedhealth;
        }
    }

    internal class CommonAutoBush
    {
        private static Spell championSpell;
        private static Menu Config => Modes.ModeConfig.MenuConfig;

        static readonly List<KeyValuePair<int, String>> Wards = new List<KeyValuePair<int, String>> //insertion order
        {
            new KeyValuePair<int, String>(3340, "Warding Totem Trinket"),
            new KeyValuePair<int, String>(3361, "Greater Stealth Totem Trinket"),
            new KeyValuePair<int, String>(3205, "Quill Coat"),
            new KeyValuePair<int, String>(3207, "Spirit Of The Ancient Golem"),
            new KeyValuePair<int, String>(3154, "Wriggle's Lantern"),
            new KeyValuePair<int, String>(2049, "Sight Stone"),
            new KeyValuePair<int, String>(2045, "Ruby Sightstone"),
            new KeyValuePair<int, String>(3160, "Feral Flare"),
            new KeyValuePair<int, String>(2050, "Explorer's Ward"),
            new KeyValuePair<int, String>(2044, "Stealth Ward"),
        };

        static int[] wardIds = { 3340, 3350, 3205, 3207, 2049, 2045, 2044, 3361, 3154, 3362, 3160, 2043 };

        private static int lastTimeWarded;

        private static Menu menu;

        public static void Init(Menu parentMenu)
        {
            menu = parentMenu.AddSubMenu("Auto Bush Revealer", "AutoBushRevealer");
            var useMenuItemName = "Use." + ObjectManager.Player.ChampionName;
            var useMenuItemText = "Use " + ObjectManager.Player.ChampionName;

            foreach (var ward in Wards)
            {
                menu.Add("AutoBush." + ward.Key, new CheckBox(ward.Value));
            }

            switch (ObjectManager.Player.ChampionName)
            {
                case "Corki":
                    {
                        menu.Add(useMenuItemName, new CheckBox(useMenuItemText + " Q"));
                        break;
                    }
                case "Ashe":
                    {
                        menu.Add(useMenuItemName, new CheckBox(useMenuItemText + " E"));
                        break;
                    }
                case "Quinn":
                    {
                        menu.Add(useMenuItemName, new CheckBox(useMenuItemText + " W"));
                        break;
                    }
                case "Kalista":
                    {
                        menu.Add(useMenuItemName, new CheckBox(useMenuItemText + " W"));
                        break;
                    }
                case "Jinx":
                    {
                        menu.Add(useMenuItemName, new CheckBox(useMenuItemText + " E"));
                        break;
                    }
            }
            menu.Add("AutoBushEnabled", new CheckBox("Enabled"));
            menu.Add("AutoBushKey", new KeyBind("Key", false, KeyBind.BindTypes.HoldActive, 32));
            new AutoBushHelper();

            championSpell = GetSpell();

            Game.OnUpdate += Game_OnGameUpdate;
        }

        private static Spell GetSpell()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Corki":
                    {
                        return new Spell(SpellSlot.Q, 700);
                    }
                case "Ashe":
                    {
                        return new Spell(SpellSlot.E, 700);
                    }
                case "Quinn":
                    {
                        return new Spell(SpellSlot.W, 900);
                    }
                case "Kalista":
                    {
                        return new Spell(SpellSlot.W, 700);
                    }
                case "Jinx":
                    {
                        return new Spell(SpellSlot.E, 900);
                    }
            }
            return null;
        }

        private static InventorySlot GetWardSlot
        {
            get
            {
                return
                    wardIds.Select(x => x)
                        .Where(
                            id =>
                                //menu.Item("AutoBush." + id).GetValue<bool>() && 
                                LeagueSharp.Common.Items.HasItem(id) &&
                                LeagueSharp.Common.Items.CanUseItem(id))
                        .Select(
                            wardId =>
                                ObjectManager.Player.InventoryItems.FirstOrDefault(slot => slot.Id == (ItemId)wardId))
                        .FirstOrDefault();
            }
        }

        private static Obj_AI_Base GetNearObject(String name, Vector3 pos, int maxDistance)
        {
            return ObjectManager.Get<Obj_AI_Base>()
                .FirstOrDefault(x => x.Name == name && x.LSDistance(pos) <= maxDistance);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            int time = Environment.TickCount;

            if (menu["AutoBushEnabled"].Cast<CheckBox>().CurrentValue || menu["AutoBushKey"].Cast<KeyBind>().CurrentValue)
            {

                foreach (AIHeroClient enemy in
                    AutoBushHelper.EnemyInfo.Where(
                        x =>
                            x.Player.IsValid && !x.Player.IsVisible && !x.Player.IsDead
                            && x.Player.LSDistance(ObjectManager.Player.ServerPosition) < 1000 && time - x.LastSeen < 2500)
                        .Select(x => x.Player))
                {
                    var wardPosition = GetWardPos(enemy.ServerPosition, 165, 2);

                    if (wardPosition != enemy.ServerPosition && wardPosition != Vector3.Zero &&
                        wardPosition.LSDistance(ObjectManager.Player.ServerPosition) <= 600)
                    {
                        int timedif = Environment.TickCount - lastTimeWarded;

                        if (timedif > 1250 && !(timedif < 2500 && GetNearObject("SightWard", wardPosition, 200) != null))
                        //no near wards
                        {
                            //var myInClause = new string[] { "Corki", "Ashe", "Quinn", "Kalista" };
                            //var results = from x in ObjectManager.Player.ChampionName
                            //              where myInClause.Contains(x.ToString())
                            //              select x;

                            //if (championSpell.IsReady())
                            //{
                            //    championSpell.Cast(wardPosition);
                            //    return;
                            //}

                            if ((ObjectManager.Player.ChampionName == "Corki"
                                 || ObjectManager.Player.ChampionName == "Ashe"
                                 || ObjectManager.Player.ChampionName == "Quinn"
                                 || ObjectManager.Player.ChampionName == "Kalista"
                                 || ObjectManager.Player.ChampionName == "Jinx") && championSpell.IsReady())
                            {
                                championSpell.Cast(wardPosition);
                                lastTimeWarded = Environment.TickCount;
                                return;
                            }

                            var wardSlot = GetWardSlot;
                            if (wardSlot != null && wardSlot.Id != ItemId.Unknown)
                            {
                                ObjectManager.Player.Spellbook.CastSpell(wardSlot.SpellSlot, wardPosition);
                                lastTimeWarded = Environment.TickCount;
                            }
                        }
                    }
                }
            }
        }

        private static Vector3 GetWardPos(Vector3 lastPos, int radius = 165, int precision = 3)
        {
            var count = precision;
            while (count > 0)
            {
                var vertices = radius;

                var wardLocations = new WardLocation[vertices];
                var angle = 2 * Math.PI / vertices;

                for (var i = 0; i < vertices; i++)
                {
                    var th = angle * i;
                    var pos = new Vector3((float)(lastPos.X + radius * Math.Cos(th)),
                        (float)(lastPos.Y + radius * Math.Sin(angle * i)), 0);
                    wardLocations[i] = new WardLocation(pos, NavMesh.IsWallOfGrass(pos, 50));
                }

                var grassLocations = new List<GrassLocation>();

                for (var i = 0; i < wardLocations.Length; i++)
                {
                    if (!wardLocations[i].Grass)
                    {
                        continue;
                    }
                    if (i != 0 && wardLocations[i - 1].Grass)
                    {
                        grassLocations.Last().Count++;
                    }
                    else
                    {
                        grassLocations.Add(new GrassLocation(i, 1));
                    }
                }

                var grassLocation = grassLocations.OrderByDescending(x => x.Count).FirstOrDefault();

                if (grassLocation != null)
                {
                    var midelement = (int)Math.Ceiling(grassLocation.Count / 2f);
                    lastPos = wardLocations[grassLocation.Index + midelement - 1].Pos;
                    radius = (int)Math.Floor(radius / 2f);
                }

                count--;
            }

            return lastPos;
        }

        private class WardLocation
        {
            public readonly Vector3 Pos;

            public readonly bool Grass;

            public WardLocation(Vector3 pos, bool grass)
            {
                Pos = pos;
                Grass = grass;
            }
        }

        private class GrassLocation
        {
            public readonly int Index;

            public int Count;

            public GrassLocation(int index, int count)
            {
                Index = index;
                Count = count;
            }
        }
    }
}