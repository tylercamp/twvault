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

        public float DistanceTo(int x, int y) => MathF.Sqrt((X - x) * (X - x) + (Y - y) * (Y - y));
        public float DistanceTo(Coordinate other) => DistanceTo(other.X, other.Y);
    }

    public static class ScaffoldExtensions
    {
        public static Coordinate Coordinates(this Scaffold.Village village) => new Coordinate { X = village.X.Value, Y = village.Y.Value };

        public static Coordinate Coordinates(this JSON.Village village) => new Coordinate { X = village.X.Value, Y = village.Y.Value };
    }
}
