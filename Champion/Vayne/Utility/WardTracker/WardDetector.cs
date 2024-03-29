﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using LeagueSharp.Common;

using TargetSelector = PortAIO.TSManager; namespace WardTracker
{
    internal class WardDetector
    {
        /// <summary>
        ///     The last tick the OnTick cycle performed
        /// </summary>
        public static float lastTick;

        /// <summary>
        ///     Called when the assembly updates.
        /// </summary>
        public static void OnTick()
        {
            if (Environment.TickCount - lastTick < 30)
            {
                return;
            }
            lastTick = Environment.TickCount;

            foreach (var s in WardTrackerVariables.detectedWards)
            {
                if (Environment.TickCount > s.startTick + s.WardTypeW.WardDuration)
                {
                    s.RemoveRenderObjects();
                }
            }

            WardTrackerVariables.detectedWards.RemoveAll(
                s => Environment.TickCount > s.startTick + s.WardTypeW.WardDuration);
        }

        /// <summary>
        ///     Called when an spell is processed
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        public static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsAlly)
            {
                foreach (var wrapperType in WardTrackerVariables.wrapperTypes)
                {
                    if (wrapperType.SpellName.ToLower().Equals(args.SData.Name.ToLower()))
                    {
                        var wardEndPosition = args.End;
                        WardTrackerVariables.detectedWards.Add(new Ward(wrapperType)
                        {
                            Position = wardEndPosition,
                            startTick = Environment.TickCount
                        });
                    }
                }
            }
        }

        /// <summary>
        ///     Called when an object is created.
        /// </summary>
        /// <param name="sender">The object.</param>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender is Obj_AI_Base && !sender.IsAlly)
            {
                var sender_ex = sender as Obj_AI_Base;
                var ward = WardTrackerVariables.wrapperTypes.FirstOrDefault(
                    w => w.ObjectName.ToLower().Equals(sender_ex.CharData.BaseSkinName.ToLower()));
                if (ward != null)
                {
                    var StartTick = Environment.TickCount - (int) ((sender_ex.MaxMana - sender_ex.Mana)*1000);

                    if (WardTrackerVariables.detectedWards.Any())
                    {
                        var AlreadyDetected =
                            WardTrackerVariables.detectedWards.FirstOrDefault(
                                w =>
                                    w.Position.LSDistance(sender_ex.ServerPosition) < 125 &&
                                    (Math.Abs(w.startTick - StartTick) < 800 || w.WardTypeW.WardType != WardType.Green ||
                                     w.WardTypeW.WardType != WardType.Trinket));
                        if (AlreadyDetected != null)
                        {
                            AlreadyDetected.RemoveRenderObjects();
                            WardTrackerVariables.detectedWards.RemoveAll(
                                w =>
                                    w.Position.LSDistance(sender_ex.ServerPosition) < 125 &&
                                    (Math.Abs(w.startTick - StartTick) < 800 || w.WardTypeW.WardType != WardType.Green ||
                                     w.WardTypeW.WardType != WardType.Trinket));
                        }
                    }

                    WardTrackerVariables.detectedWards.Add(new Ward(ward)
                    {
                        Position = sender_ex.ServerPosition,
                        startTick = StartTick
                    });
                }
            }
        }

        /// <summary>
        ///     Called when a game object is deleted.
        /// </summary>
        /// <param name="sender">The game object.</param>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender is Obj_AI_Base && !sender.IsAlly)
            {
                var sender_ex = sender as Obj_AI_Base;

                foreach (
                    var s in
                        WardTrackerVariables.detectedWards.Where(
                            s => s.Position.LSDistance(sender_ex.ServerPosition, true) < 10*10))
                {
                    s.RemoveRenderObjects();
                }

                WardTrackerVariables.detectedWards.RemoveAll(
                    s => s.Position.LSDistance(sender_ex.ServerPosition, true) < 10*10);
            }
        }
    }
}