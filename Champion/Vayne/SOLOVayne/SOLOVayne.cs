﻿using System;
using System.Collections.Generic;
using System.Linq;
using ClipperLib;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;
using Color = System.Drawing.Color;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using GamePath = System.Collections.Generic.List<SharpDX.Vector2>;

using TargetSelector = PortAIO.TSManager; namespace SOLOVayne
{
    internal class SOLOPolygon
    {
        /// <summary>
        ///     The points of the polygon
        /// </summary>
        public GamePath Points;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SOLOPolygon" /> class.
        /// </summary>
        /// <param name="p">The p.</param>
        public SOLOPolygon(GamePath p)
        {
            Points = p;
        }

        /// <summary>
        ///     Adds the specified vector to the polygon.
        /// </summary>
        /// <param name="vec">The vector.</param>
        public void Add(Vector2 vec)
        {
            Points.Add(vec);
        }

        /// <summary>
        ///     Counts the points in this instance.
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return Points.Count;
        }

        /// <summary>
        ///     Determines whether the polygon contains the point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        public bool Contains(Vector2 point)
        {
            var result = false;
            var j = Count() - 1;
            for (var i = 0; i < Count(); i++)
            {
                if (Points[i].Y < point.Y && Points[j].Y >= point.Y || Points[j].Y < point.Y && Points[i].Y >= point.Y)
                {
                    if (Points[i].X +
                        (point.Y - Points[i].Y)/(Points[j].Y - Points[i].Y)*(Points[j].X - Points[i].X) < point.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        /// <summary>
        ///     Creates a rectangle with a given start vector.
        /// </summary>
        /// <param name="startVector2">The start vector2.</param>
        /// <param name="endVector2">The end vector2.</param>
        /// <param name="radius">The radius.</param>
        /// <returns></returns>
        public static GamePath Rectangle(Vector2 startVector2, Vector2 endVector2, float radius)
        {
            var points = new List<Vector2>();

            var v1 = endVector2 - startVector2;
            var to1Side = Vector2.Normalize(v1).Perpendicular()*radius;

            points.Add(startVector2 + to1Side);
            points.Add(startVector2 - to1Side);
            points.Add(endVector2 - to1Side);
            points.Add(endVector2 + to1Side);
            return points;
        }
    }

    /// <summary>
    ///     Class that contains the geometry related methods.
    /// </summary>
    public static class SOLOGeometry
    {
        private const int CircleLineSegmentN = 22;

        public static Vector3 SwitchYZ(this Vector3 v)
        {
            return new Vector3(v.X, v.Z, v.Y);
        }

        public static bool IsOverWall(Vector3 start, Vector3 end)
        {
            double distance = Vector3.Distance(start, end);
            for (uint i = 0; i < distance; i += 10)
            {
                var tempPosition = start.Extend(end, i).To3D();
                if (tempPosition.IsWall())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Returns the position on the path after t milliseconds at speed speed.
        /// </summary>
        public static Vector2 PositionAfter(this GamePath self, int t, int speed, int delay = 0)
        {
            var distance = Math.Max(0, t - delay)*speed/1000;
            for (var i = 0; i <= self.Count - 2; i++)
            {
                var from = self[i];
                var to = self[i + 1];
                var d = (int) to.Distance(from);
                if (d > distance)
                {
                    return from + distance*(to - from).Normalized();
                }
                distance -= d;
            }
            return self[self.Count - 1];
        }

        public static Polygon ToPolygon(this Path v)
        {
            var polygon = new Polygon();
            foreach (var point in v)
            {
                polygon.Add(new Vector2(point.X, point.Y));
            }
            return polygon;
        }


        public static Paths ClipPolygons(List<Polygon> polygons)
        {
            var subj = new Paths(polygons.Count);
            var clip = new Paths(polygons.Count);

            foreach (var polygon in polygons)
            {
                subj.Add(polygon.ToClipperPath());
                clip.Add(polygon.ToClipperPath());
            }

            var solution = new Paths();
            var c = new Clipper();
            c.AddPaths(subj, PolyType.ptSubject, true);
            c.AddPaths(clip, PolyType.ptClip, true);
            c.Execute(ClipType.ctUnion, solution, PolyFillType.pftPositive, PolyFillType.pftEvenOdd);

            return solution;
        }


        public class Circle
        {
            public Vector2 Center;
            public float Radius;

            public Circle(Vector2 center, float radius)
            {
                Center = center;
                Radius = radius;
            }

            public Polygon ToPolygon(int offset = 0, float overrideWidth = -1)
            {
                var result = new Polygon();
                var outRadius = overrideWidth > 0
                    ? overrideWidth
                    : (offset + Radius)/(float) Math.Cos(2*Math.PI/CircleLineSegmentN);

                for (var i = 1; i <= CircleLineSegmentN; i++)
                {
                    var angle = i*2*Math.PI/CircleLineSegmentN;
                    var point = new Vector2(
                        Center.X + outRadius*(float) Math.Cos(angle), Center.Y + outRadius*(float) Math.Sin(angle));
                    result.Add(point);
                }

                return result;
            }
        }

        public class Polygon
        {
            public List<Vector2> Points = new List<Vector2>();

            public void Add(Vector2 point)
            {
                Points.Add(point);
            }

            public Path ToClipperPath()
            {
                var result = new Path(Points.Count);
                result.AddRange(Points.Select(point => new IntPoint(point.X, point.Y)));

                return result;
            }

            public bool IsOutside(Vector2 point)
            {
                var p = new IntPoint(point.X, point.Y);
                return Clipper.PointInPolygon(p, ToClipperPath()) != 1;
            }

            public void Draw(Color color, int width = 1)
            {
                for (var i = 0; i <= Points.Count - 1; i++)
                {
                    var nextIndex = Points.Count - 1 == i ? 0 : i + 1;
                    DrawLineInWorld(Points[i].To3D(), Points[nextIndex].To3D(), width, color);
                }
            }

            public static void DrawLineInWorld(Vector3 start, Vector3 end, int width, Color color)
            {
                var from = Drawing.WorldToScreen(start);
                var to = Drawing.WorldToScreen(end);
                Drawing.DrawLine(from[0], from[1], to[0], to[1], width, color);
            }
        }

        public class Rectangle
        {
            public Vector2 Direction;
            public Vector2 Perpendicular;
            public Vector2 REnd;
            public Vector2 RStart;
            public float Width;

            public Rectangle(Vector2 start, Vector2 end, float width)
            {
                RStart = start;
                REnd = end;
                Width = width;
                Direction = (end - start).Normalized();
                Perpendicular = Direction.Perpendicular();
            }

            public Polygon ToPolygon(int offset = 0, float overrideWidth = -1)
            {
                var result = new Polygon();

                result.Add(
                    RStart + (overrideWidth > 0 ? overrideWidth : Width + offset)*Perpendicular - offset*Direction);
                result.Add(
                    RStart - (overrideWidth > 0 ? overrideWidth : Width + offset)*Perpendicular - offset*Direction);
                result.Add(
                    REnd - (overrideWidth > 0 ? overrideWidth : Width + offset)*Perpendicular + offset*Direction);
                result.Add(
                    REnd + (overrideWidth > 0 ? overrideWidth : Width + offset)*Perpendicular + offset*Direction);

                return result;
            }
        }


        public class Ring
        {
            public Vector2 Center;
            public float Radius;
            public float RingRadius; //actually radius width.

            public Ring(Vector2 center, float radius, float ringRadius)
            {
                Center = center;
                Radius = radius;
                RingRadius = ringRadius;
            }

            public Polygon ToPolygon(int offset = 0)
            {
                var result = new Polygon();

                var outRadius = (offset + Radius + RingRadius)/(float) Math.Cos(2*Math.PI/CircleLineSegmentN);
                var innerRadius = Radius - RingRadius - offset;

                for (var i = 0; i <= CircleLineSegmentN; i++)
                {
                    var angle = i*2*Math.PI/CircleLineSegmentN;
                    var point = new Vector2(
                        Center.X - outRadius*(float) Math.Cos(angle), Center.Y - outRadius*(float) Math.Sin(angle));
                    result.Add(point);
                }

                for (var i = 0; i <= CircleLineSegmentN; i++)
                {
                    var angle = i*2*Math.PI/CircleLineSegmentN;
                    var point = new Vector2(
                        Center.X + innerRadius*(float) Math.Cos(angle),
                        Center.Y - innerRadius*(float) Math.Sin(angle));
                    result.Add(point);
                }


                return result;
            }
        }

        public class Sector
        {
            public float Angle;
            public Vector2 Center;
            public Vector2 Direction;
            public float Radius;

            public Sector(Vector2 center, Vector2 direction, float angle, float radius)
            {
                Center = center;
                Direction = direction;
                Angle = angle;
                Radius = radius;
            }

            public Polygon ToPolygon(int offset = 0)
            {
                var result = new Polygon();
                var outRadius = (Radius + offset)/(float) Math.Cos(2*Math.PI/CircleLineSegmentN);

                result.Add(Center);
                var Side1 = Direction.Rotated(-Angle*0.5f);

                for (var i = 0; i <= CircleLineSegmentN; i++)
                {
                    var cDirection = Side1.Rotated(i*Angle/CircleLineSegmentN).Normalized();
                    result.Add(new Vector2(Center.X + outRadius*cDirection.X, Center.Y + outRadius*cDirection.Y));
                }

                return result;
            }
        }
    }
}