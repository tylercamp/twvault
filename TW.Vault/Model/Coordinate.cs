using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model
{
    public struct Coordinate
    {
        public int X;
        public int Y;

        public override string ToString() => $"{X}|{Y}";

        public float DistanceTo(int x, int y) => MathF.Sqrt((X - x) * (X - x) + (Y - y) * (Y - y));
        public float DistanceTo(Coordinate other) => DistanceTo(other.X, other.Y);

        public static float Distance(int x1, int y1, int x2, int y2) => MathF.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
    }

    public static class ScaffoldExtensions
    {
        public static Coordinate Coordinates(this Scaffold.Village village) => new Coordinate { X = village.X.Value, Y = village.Y.Value };

        public static Coordinate Coordinates(this JSON.Village village) => new Coordinate { X = village.X.Value, Y = village.Y.Value };
    }
}
