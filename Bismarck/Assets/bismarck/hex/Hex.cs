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
        /// Get the distance between this hex coordinate and another.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public float Distance(Hex other)
        {
            return (this - other).Length();
        }

        /// <summary>
        /// Get the given direction offset associated with direction.
        /// 0 is +R, 1 is -r+q, etc. moving CCW around the hex.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Hex GetDirection(int direction)
        {
            return Directions[(6 + (direction % 6)) % 6];
        }

        /// <summary>
        /// Get the neighbor of this hex coordinate, provided a direction.
        /// 0 is +R, 1 is -r+q, etc. moving CCW around the hex.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Hex GetNeighbor(int direction)
        {
            return GetDirection(direction) + this;
        }
    }
}