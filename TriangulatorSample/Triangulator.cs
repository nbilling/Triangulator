using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Triangulation
{
    public static class Triangulator
    {
        private class PolygonVertex
        {
            public PolygonVertex Prev { get; set; }
            public PolygonVertex Next { get; set; }
            public int Index { get; set; }
            public float WindingValue { get; set; }
            public bool IsReflex { get; set; }
        };

        /// <summary>
        /// Splits a polygon into triangles.
        /// </summary>
        /// <param name="polygon">
        /// Polygon to be split.
        /// </param>
        /// <returns>
        /// List of vertices in input polygon that form triangles (will either
        /// be empty or have count that is a factor of 3). Triangles will be clockwise if
        /// polygon is clockwise and counter-clockwise if polygon is counter-clockwise.
        /// </returns>
        public static IEnumerable<int> Triangulate(IList<Vector> polygon)
        {
            int N = polygon.Count;

            // Not a polygon.
            if (N <= 2)
            {
                return new int[] { };
            }

            IList<int> triangles = new List<int>();

            // Initialize leftmost and vertices for polygon
            IList<PolygonVertex> vertices = polygon.Select(v => new PolygonVertex()).ToList();
            int iLeftMost = 0;
            for (int i = 0; i < N; i++)
            {
                int iPrev = Mod(i - 1, N);
                int iNext = Mod(i + 1, N);

                // Init polygon vertex
                vertices[i].Index = i;
                vertices[i].Prev = vertices[iPrev];
                vertices[i].Next = vertices[iNext];
                vertices[i].Prev.Index = iPrev;
                vertices[i].Next.Index = iNext;
                vertices[i].WindingValue = WindingValue(polygon, vertices[i]);
                vertices[i].IsReflex = false;

                // Update leftmost for polygon
                Vector p = polygon[i];
                Vector lm = polygon[iLeftMost];

                if (p.X < lm.X || (p.X == lm.X && p.Y < lm.Y))
                {
                    iLeftMost = i;
                }
            }

            // Check if polygon is counter-clockwise
            bool isCcw = vertices[iLeftMost].WindingValue > 0.0f;

            // Initialize list of reflex vertices
            IList<PolygonVertex> reflexVertices = new List<PolygonVertex>();

            foreach (var vertex in vertices)
            {
                if (IsReflex(isCcw, vertex))
                {
                    vertex.IsReflex = true;
                    reflexVertices.Add(vertex);
                }
            }

            // Perform triangulation
            int skipped = 0; // Number of consecutive vertices skipped
            int nVertices = vertices.Count; // Number of vertices left in polygon

            PolygonVertex current = vertices[0];

            // While polygon not a triangle
            while (nVertices > 3)
            {
                PolygonVertex prev = current.Prev;
                PolygonVertex next = current.Next;

                if (IsEarTip(polygon, current, reflexVertices))
                {
                    // Add this ear to list of triangles
                    triangles.Add(prev.Index);
                    triangles.Add(current.Index);
                    triangles.Add(next.Index);

                    // Remove this ear from polygon
                    prev.Next = next;
                    next.Prev = prev;

                    // Re-calculate reflexivity of adjacent vertices
                    PolygonVertex[] adjacent = { prev, next };
                    foreach (var vertex in adjacent)
                    {
                        if (vertex.IsReflex)
                        {
                            vertex.WindingValue = WindingValue(polygon, vertex);
                            vertex.IsReflex = IsReflex(isCcw, vertex);

                            if (!vertex.IsReflex)
                            {
                                reflexVertices.Remove(vertex);
                            }
                        }
                    }

                    nVertices--;
                    skipped = 0;
                }
                else if (++skipped > nVertices)
                {
                    // If we have gone through all remaining vertices and not found ear, then fail.
                    return new int[] { };
                }

                current = next;
            }

            // Remaining polygon _is_ a triangle.
            triangles.Add(current.Prev.Index);
            triangles.Add(current.Index);
            triangles.Add(current.Next.Index);

            return triangles;
        }

        private static bool TriangleContains(Vector a, Vector b, Vector c, Vector point)
        {

            if ((point.X == a.X && point.Y == a.Y) ||
                (point.X == b.X && point.Y == b.Y) ||
                (point.X == c.X && point.Y == c.Y))
            {
                return false;
            }

            float A = 0.5f * (float)(-b.Y * c.X + a.Y * (-b.X + c.X) + a.X * (b.Y - c.Y) + b.X * c.Y);
            float sign = A < 0.0f ? -1.0f : 1.0f;

            float s = (float)(a.Y * c.X - a.X * c.Y + (c.Y - a.Y) * point.X + (a.X - c.X) * point.Y) * sign;
            float t = (float)(a.X * b.Y - a.Y * b.X + (a.Y - b.Y) * point.X + (b.X - a.X) * point.Y) * sign;

            return s >= 0.0f && t >= 0.0f && (s + t) <= (2.0f * A * sign);
        }

        private static float WindingValue(IList<Vector> polygon, PolygonVertex vertex)
        {
            Vector a = polygon[vertex.Prev.Index];
            Vector b = polygon[vertex.Index];
            Vector c = polygon[vertex.Next.Index];

            return (float)((b.X - a.X) * (c.Y - b.Y) - (c.X - b.X) * (b.Y - a.Y));
        }

        private static bool IsEarTip(IList<Vector> polygon, PolygonVertex vertex, IList<PolygonVertex> reflexVertices)
        {
            if (vertex.IsReflex)
            {
                return false;
            }

            Vector a = polygon[vertex.Prev.Index];
            Vector b = polygon[vertex.Index];
            Vector c = polygon[vertex.Next.Index];

            foreach (var reflexVertex in reflexVertices)
            {
                int index = reflexVertex.Index;

                if (index == vertex.Prev.Index || index == vertex.Next.Index)
                {
                    continue;
                }

                if (TriangleContains(a, b, c, polygon[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsReflex(bool isCcw, PolygonVertex v)
        {
            return isCcw ? v.WindingValue <= 0.0f : v.WindingValue >= 0.0f;
        }

        private static int Mod(int n, int m)
        {
            return ((n % m) + m) % m;
        }
    }
}
