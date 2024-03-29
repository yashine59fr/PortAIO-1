﻿using EloBuddy;
using LeagueSharp;
using LeagueSharp.Common;

using SharpDX;
using System.Linq;

using TargetSelector = PortAIO.TSManager; namespace Nechrito_Nidalee.Extras
{
    static class FleeLogic
    {
        public static Vector3 GetFirstWallPoint(Vector3 start, Vector3 end, int step = 1)
        {
            if (start.IsValid() && end.IsValid())
            {
                var distance = start.LSDistance(end);
                for (var i = 0; i < distance; i = i + step)
                {
                    var newPoint = start.LSExtend(end, i);

                    if (NavMesh.GetCollisionFlags(newPoint) == CollisionFlags.Wall || newPoint.LSIsWall())
                    {
                        return newPoint;
                    }
                }
            }
            return Vector3.Zero;
        }
        public static float GetWallWidth(Vector3 start, Vector3 direction, int maxWallWidth = 1000, int step = 1)
        {
            var thickness = 0f;

            if (!start.IsValid() || !direction.IsValid())
            {
                return thickness;
            }

            for (var i = 0; i < maxWallWidth; i = i + step)
            {
                if (NavMesh.GetCollisionFlags(start.LSExtend(direction, i)) == CollisionFlags.Wall
                    || start.LSExtend(direction, i).LSIsWall())
                {
                    thickness += step;
                }
                else
                {
                    return thickness;
                }
            }

            return thickness;
        }
        public static bool IsWallDash(Obj_AI_Base unit, float dashRange, float minWallWidth = 75)
        {
            return IsWallDash(unit.ServerPosition, dashRange, minWallWidth);
        }
        public static bool IsWallDash(Vector3 position, float dashRange, float minWallWidth = 75)
        {
            var dashEndPos = Core.Player.Position.LSExtend(position, dashRange);
            var firstWallPoint = GetFirstWallPoint(ObjectManager.Player.Position, dashEndPos);

            if (firstWallPoint.Equals(Vector3.Zero))
            {
                // No Wall
                return false;
            }

            if (dashEndPos.LSIsWall())
            // End Position is in Wall
            {
                var wallWidth = GetWallWidth(firstWallPoint, dashEndPos);

                if (wallWidth > minWallWidth && wallWidth - firstWallPoint.LSDistance(dashEndPos) < wallWidth * 0.4f)
                {
                    return true;
                }
            }
            else
            // End Position is not a Wall
            {
                return true;
            }

            return false;
        }
    }
}