#region LICENSE

// Copyright 2014 - 2014 Support
// Utils.cs is part of Support.
// Support is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// Support is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with Support. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using EloBuddy;

#endregion

using TargetSelector = PortAIO.TSManager; namespace GosuMechanicsYasuo
{
    public static class Utils
    {
        public static List<Vector2> To2DList(this Vector3[] v)
        {
            var result = new List<Vector2>();
            foreach (var point in v)
            {
                result.Add(point.LSTo2D());
            }
            return result;
        }


        public static Obj_AI_Base Closest(List<Obj_AI_Base> targetList, Vector2 from)
        {
            var dist = float.MaxValue;
            Obj_AI_Base result = null;

            foreach (var target in targetList)
            {
                var distance = Vector2.DistanceSquared(from, target.ServerPosition.LSTo2D());
                if (distance < dist)
                {
                    dist = distance;
                    result = target;
                }
            }

            return result;
        }

        /// <summary>
        ///     Returns when the unit will be able to move again
        /// </summary>
        public static int ImmobileTime(Obj_AI_Base unit)
        {
            var result = 0f;

            foreach (var buff in unit.Buffs)
            {
                if (buff.IsActive && Game.Time <= buff.EndTime &&
                    (buff.Type == BuffType.Charm || buff.Type == BuffType.Knockup || buff.Type == BuffType.Stun ||
                     buff.Type == BuffType.Suppression || buff.Type == BuffType.Snare))
                {
                    result = Math.Max(result, buff.EndTime);
                }
            }

            return (result == 0f) ? -1 : (int) (Environment.TickCount + (result - Game.Time)*1000);
        }


        public static void DrawLineInWorld(Vector3 start, Vector3 end, int width, Color color)
        {
            var from = Drawing.WorldToScreen(start);
            var to = Drawing.WorldToScreen(end);
            Drawing.DrawLine(from[0], from[1], to[0], to[1], width, color);
            //Drawing.DrawLine(from.X, from.Y, to.X, to.Y, width, color);
        }
    }

    public class SpellList<T> : List<T>
    {
        public event EventHandler OnAdd;

        public new void Add(T item)
        {
            if (OnAdd != null)
            {
                OnAdd(this, null);
            }

            base.Add(item);
        }
    }
}