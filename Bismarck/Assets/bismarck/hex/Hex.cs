using System.Collections.Generic;
using bismarck.meshing;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace bismarck.hex
{
    /// <summary>
    /// A hex coordinate.
    /// </summary>
    public struct Hex
    {
        private readonly Vector3 _v;

        public float q => _v.x;
        public float r => _v.y;
        public float s => _v.z;

        /// <summary>
        /// A set of direction vectors, one for each side of the hexagon.
        /// </summary>
        public static readonly Hex[] Directions = new[]
        {
            new Hex(1, 0, -1),
            new Hex(1, -1, 0),
            new Hex(0, -1, 1),
            new Hex(-1, 0, 1),
            new Hex(-1, 1, 0),
            new Hex(0, 1, -1)
        };

        public Hex(float q, float r, float s)
        {
            Assert.AreEqual(q + r + s, 0);
            _v = new Vector3(q, r, s);
        }

        public Hex(float q, float r)
        {
            _v = new Vector3(q, r, -q-r);
        }

        public override bool Equals(object obj)
        {
            if (obj is Hex other)
            {
                return _v.Equals(other._v);
            }

            return false;
        }

        public static bool operator ==(Hex a, Hex b)
        {
            return a._v == b._v;
        }

        public static bool operator !=(Hex a, Hex b)
        {
            return a._v != b._v;
        }

        public override int GetHashCode()
        {
            return _v.GetHashCode();
        }

        public override string ToString()
        {
            return "HEX<" + q + ", " + r + ", " + s + ">";
        }

        /// <summary>
        /// Addition between two hex vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Hex operator +(Hex a, Hex b)
        {
            return new Hex(a.q + b.q, a.r + b.r, a.s + b.s);
        }
        
        /// <summary>
        /// Subtraction between two hex vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Hex operator -(Hex a, Hex b)
        {
            return new Hex(a.q - b.q, a.r - b.r, a.s - b.s);
        }
        
        /// <summary>
        /// Multiplication between a hex vector and a multiplier.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static Hex operator *(Hex a, float k)
        {
            return new Hex(a.q * k, a.r * k, a.s * k);
        }

        /// <summary>
        /// Get the length of this hex vector.
        /// Manhattan distance, or number of cells between two points.
        /// </summary>
        /// <returns></returns>
        public float Length()
        {
            return (Mathf.Abs(q) + Mathf.Abs(r) + Mathf.Abs(s)) / 2.0f;
        }

        /// <summary>
        /// Get the distance between two hex coordinates.
        /// </summary>
        /// <returns></returns>
        public static float Distance(Hex a, Hex b)
        {
            return (a - b).Length();
        }

        /// <summary>
        /// Get the given direction offset associated with direction.
        /// 0 is +R, 1 is -r+q, etc. moving CCW around the hex.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public readonly Hex GetDirection(int direction)
        {
            return Directions[(6 + (direction % 6)) % 6];
        }

        /// <summary>
        /// Get the neighbor of this hex coordinate, provided a direction.
        /// 0 is +R, 1 is -r+q, etc. moving CCW around the hex.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public readonly Hex GetNeighbor(int direction)
        {
            return GetDirection(direction) + this;
        }

        /// <summary>
        /// Round this hex coordinate to the next center (integer) hex coordinate.
        /// </summary>
        /// <returns></returns>
        public Hex Round()
        {
            int rq = Mathf.RoundToInt(q);
            int rr = Mathf.RoundToInt(r);
            int rs = Mathf.RoundToInt(s);

            float qDiff = Mathf.Abs(rq - q);
            float rDiff = Mathf.Abs(rr - r);
            float sDiff = Mathf.Abs(rs - s);

            if (qDiff > rDiff && qDiff > sDiff)
            {
                rq = -rr - rs;
            }
            else if (rDiff > sDiff)
            {
                rr = -rq - rs;
            }
            else
            {
                rs = -rq - rr;
            }

            return new Hex(rr, rq, rs);
        }

        /// <summary>
        /// Linearly interpolate between two hex values.
        /// </summary>
        /// <param name="a">The starting point</param>
        /// <param name="b">The ending point</param>
        /// <param name="t">The amount to interpolate</param>
        /// <returns>A hex value "t" between a and b</returns>
        public static Hex Lerp(Hex a, Hex b, float t)
        {
            return new Hex(Mathf.Lerp(a.q, b.q, t),
                Mathf.Lerp(a.r, b.r, t),
                Mathf.Lerp(a.s, b.s, t));
        }

        /// <summary>
        /// Get the list of hex coordinates between start and end, inclusive.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static List<Hex> DrawLine(Hex start, Hex end)
        {
            float distance = Hex.Distance(start, end);
            
            /* Nudge the points in a given direction to ensure they're never on an edge and round consistently */
            Hex sNudge = new Hex(start.q + 1e-6f, start.r + 1e-6f, start.s - 2e-6f);
            Hex eNudge = new Hex(end.q + 1e-6f, end.r + 1e-6f, end.s - 2e-6f);

            List<Hex> output = new List<Hex>();
            float step = 1.0f / Mathf.Max(distance, 1);
            for (int i = 0; i < distance; i++)
            {
                output.Add(Hex.Lerp(sNudge, eNudge, step * i));
            }

            return output;
        }
        
        /// <summary>
        /// Convert this hex coordinate to offset form.
        /// </summary>
        /// <returns></returns>
        public (int row, int col) ToOffsetCoord()
        {
            float col = q + (int)((r - 1 * ((int)r & 1)) / 2);
            float row = r;

            return (Mathf.RoundToInt(row), Mathf.RoundToInt(col));
        }

        /// <summary>
        /// Convert an offset coordinate to cube.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public static Hex FromOffset(int row, int col)
        {
            float q = col - (int)((row - 1 * (row & 1)) / 2);
            float r = row;

            return new Hex(q, r);
        }
    }
}