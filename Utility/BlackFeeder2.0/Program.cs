﻿using System;
using LeagueSharp.Common;

using TargetSelector = PortAIO.TSManager; namespace BlackFeeder
{
    internal class Program
    {
        public static void Init()
        {
            try
            {
                CustomEvents.Game.OnGameLoad += Entry.OnLoad;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }
    }
}