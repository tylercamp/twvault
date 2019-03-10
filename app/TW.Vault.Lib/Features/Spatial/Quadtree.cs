using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Model;

namespace TW.Vault.Features.Spatial
{
    public class Quadtree
    {
        private class Node
        {
            public int Width, Height, X, Y, Count = 0;

            public Coordinate Center => new Coordinate { X = X + Width / 2, Y = Y + Height / 2 };

            public bool Contains(Coordinate coord) =>
                coord.X >= X && coord.X <= X + Width &&
                coord.Y >= Y && coord.Y <= Y + Height;

            public bool Contains(Coordinate start, Coordinate end) => !(
                start.X > this.X + this.Width ||
                end.X < this.X ||
                start.Y > this.Y + this.Height ||
                end.Y < this.Y
            );

            public bool Contains(Coordinate center, int maxDist) => Contains(
                new Coordinate { X = center.X - maxDist, Y = center.Y - maxDist },
                new Coordinate { X = center.X + maxDist, Y = center.Y + maxDist }
            );

            public List<Node> Subnodes;
            public List<Coordinate> Coords;

            public override string ToString() => $"{X}-{X+Width}|{Y}-{Y+Height}";
        }

        Node root;

        public Quadtree(IEnumerable<Coordinate> contents, int nodeDepth = 3)
        {
            void PopulateNode(Node node, int levels)
            {
                int subWidth = node.Width / 2;
                int subHeight = node.Height / 2;

                if (subWidth == 0 || subHeight == 0)
                {
                    node.Coords = new List<Coordinate>();
                    return;
                }

                var subCenter = node.Center;

                node.Subnodes = new List<Node>
                {
                    // top-left
                    new Node { X = node.X, Y = node.Y, Width = subWidth, Height = subHeight },
                    // top-right
                    new Node { X = subCenter.X, Y = node.Y, Width = subWidth, Height = subHeight },
                    // bottom-left
                    new Node { X = node.X, Y = subCenter.Y, Width = subWidth, Height = subHeight },
                    // bottom-right
                    new Node { X = subCenter.X, Y = subCenter.Y, Width = subWidth, Height = subHeight }
                };

                foreach (var subNode in node.Subnodes)
                {
                    if (levels > 1)
                        PopulateNode(subNode, levels - 1);
                    else
                        subNode.Coords = new List<Coordinate>();
                }
            }

            void InsertCoord(Coordinate coord)
            {
                var currentNode = root;
                while (currentNode.Subnodes != null)
                {
                    currentNode.Count += 1;
                    currentNode = currentNode.Subnodes.First(n => n.Contains(coord));
                }

                currentNode.Coords.Add(coord);
                currentNode.Count += 1;
            }

            if (nodeDepth < 2)
                throw new InvalidOperationException("nodeDepth must be > 1");

            var minCoord = new Coordinate { X =  10000, Y =  10000 };
            var maxCoord = new Coordinate { X = -10000, Y = -10000 };

            var coords = contents.ToList();
            foreach (var coord in coords)
            {
                minCoord.X = Math.Min(minCoord.X, coord.X);
                minCoord.Y = Math.Min(minCoord.Y, coord.Y);
                maxCoord.X = Math.Max(maxCoord.X, coord.X);
                maxCoord.Y = Math.Max(maxCoord.Y, coord.Y);
            }

            int padding = 50;

            //  Nearest power of 2 (so we can keep evenly subdividing)
            var width = (int)Math.Pow(2, Math.Ceiling(Math.Log(maxCoord.X - minCoord.X + padding, 2)));
            var height = (int)Math.Pow(2, Math.Ceiling(Math.Log(maxCoord.Y - minCoord.Y + padding, 2)));
            var center = new Coordinate { X = (maxCoord.X + minCoord.X) / 2, Y = (maxCoord.Y + minCoord.Y) / 2 };

            root = new Node
            {
                X = center.X - width / 2,
                Y = center.Y - height / 2,
                Width = width,
                Height = height
            };

            PopulateNode(root, nodeDepth);

            foreach (var coord in coords)
                InsertCoord(coord);
        }

        public bool ContainsInRange(int x, int y, float maxDistance) => ContainsInRange(new Coordinate { X = x, Y = y }, maxDistance);

        public bool ContainsInRange(Coordinate coord, float maxDistance)
        {
            var relevantCoords = Enumerable.Empty<Coordinate>();
            var pendingNodes = new Queue<Node>();
            pendingNodes.Enqueue(root);

            while (pendingNodes.Count > 0)
            {
                var node = pendingNodes.Dequeue();
                if (node.Contains(coord, (int)Math.Ceiling(maxDistance)))
                {
                    if (node.Subnodes != null)
                    {
                        foreach (var subnode in node.Subnodes.Where(sn => sn.Count > 0))
                            pendingNodes.Enqueue(subnode);
                    }
                    else
                    {
                        if (node.Coords.Any(c => c.DistanceTo(coord) <= maxDistance))
                            return true;
                    }
                }
            }

            return false;
        }
    }
}
