﻿    using TargetSelector = PortAIO.TSManager; namespace ElUtilitySuite.Items
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Security.Permissions;

    using LeagueSharp;
    using LeagueSharp.Common;
    using EloBuddy.SDK.Menu;
    using EloBuddy;
    using EloBuddy.SDK;
    internal class Offensive2 : IPlugin
    {
        #region Fields

        private readonly List<Item> offensiveItems;

        #endregion

        #region Constructors and Destructors

        public Offensive2()
        {
            this.offensiveItems =
                Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(
                        x =>
                        x.Namespace != null && x.Namespace.Contains("OffensiveItems") && x.IsClass
                        && typeof(Item).IsAssignableFrom(x))
                    .Select(x => (Item)Activator.CreateInstance(x))
                    .ToList();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        public Menu Menu { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        /// <param name="rootMenu">The root menu.</param>
        /// <returns></returns>
        public void CreateMenu(Menu rootMenu)
        {
            Menu = rootMenu.AddSubMenu("Offensive", "omenu2");

            foreach (var item in offensiveItems)
            {
                item.Menu = Menu;
                item.CreateMenu();
            }
        }

        /// <summary>
        ///     Loads this instance.
        /// </summary>
        public void Load()
        {
            Game.OnUpdate += this.Game_OnUpdate;
            if (PortAIO.OrbwalkerManager.isEBActive)
            {
                Orbwalker.OnPostAttack += Orbwalker_OnPostAttackEB;
            }
            else
            {
                LSEvents.AfterAttack += Orbwalker_OnPostAttack;
            }
        }

        private void Orbwalker_OnPostAttackEB(AttackableUnit target, EventArgs args)
        {
            foreach (var item in this.offensiveItems.Where(x => x.AfterOrb() && EloBuddy.SDK.Item.CanUseItem((int)x.Id) && EloBuddy.SDK.Item.HasItem((int)x.Id)))
            {
                item.UseItem();
            }
        }

        private void Orbwalker_OnPostAttack(AfterAttackArgs args)
        {
            foreach (var item in this.offensiveItems.Where(x => x.AfterOrb() && EloBuddy.SDK.Item.CanUseItem((int)x.Id) && EloBuddy.SDK.Item.HasItem((int)x.Id)))
            {
                item.UseItem();
            }
        }

        #endregion

        #region Methods

        private void Game_OnUpdate(EventArgs args)
        {
            foreach (var item in this.offensiveItems.Where(x => x.ShouldUseItem() && EloBuddy.SDK.Item.CanUseItem((int)x.Id) && EloBuddy.SDK.Item.HasItem((int)x.Id)))
            {
                item.UseItem();
            }
        }

        #endregion
    }
}