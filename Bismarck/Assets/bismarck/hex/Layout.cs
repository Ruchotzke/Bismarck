using UnityEngine;

namespace bismarck.hex
{
    /// <summary>
    /// A layout manager for a hex grid.
    /// </summary>
    public class Layout
    {
        /// <summary>
        /// The orientation of this layout.
        /// </summary>
        public Orientation Orientation;

        /// <summary>
        /// The size of each hex cell in world space.
        /// </summary>
        public Vector3 Size;

        /// <summary>
        /// The layout's origin position in world space.
        /// </summary>
        public Vector3 Origin;

        /// <summary>
        /// Generate a new layout manager for the hex grid.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="size"></param>
        /// <param name="origin"></param>
        public Layout(Orientation o, Vector3 size, Vector3 origin)
        {
            Orientation = o;
            Size = size;
            Origin = origin;
        }

        /// <summary>
        /// Convert the given hex coordinate to a worldspace coordinate using this layout's reference.
        /// </summary>
        /// <param name="h">The coordinate to convert.</param>
        /// <returns>A worldspace point.</returns>
        public Vector3 HexToWorld(Hex h)
        {
            /* Apply the size and direction scale to the coordinate */
            float x = (Orientation.F0 * h.q + Orientation.F1 * h.r) * Size.x;
            float z = (Orientation.F2 * h.q + Orientation.F3 * h.r) * Size.y;

            /* Offset by the origin */
            return new Vector3(x, 0, z) + Origin;
        }

        /// <summary>
        /// Convert a given world-space position to a hex position.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Hex WorldToHex(Vector3 p)
        {
            /* First get back to a centered, unit grid */
            p = (p - Origin);
            p.Scale(Size);
            
            /* Multiply by the inverse transform to get back to hex */
            float q = Orientation.B0 * p.x + Orientation.B1 * p.z;
            float r = Orientation.B2 * p.x + Orientation.B3 * p.z;
            return new Hex(q, r);
        }

        /// <summary>
        /// Compute the world-space offset for a given corner of a hexagon.
        /// Takes into account the layout.
        /// </summary>
        /// <param name="corner">The corner in question (CCW from right side).</param>
        /// <returns></returns>
        public Vector3 HexCornerOffset(int corner)
        {
            /* Compute the angle of the corner (a percentage of a full circle sweep) */
            float angle = 2.0f * Mathf.PI * (Orientation.StartAngle + corner) / 6.0f;

            return new Vector3(Size.x * Mathf.Cos(angle), 0f, Size.z * Mathf.Sin(angle));
        }

        /// <summary>
        /// Generate all corners for a given hex.
        /// </summary>
        /// <param name="h"></param>
        /// <returns></returns>
        public Vector3[] GenerateCorners(Hex h)
        {
            Vector3[] corners = new Vector3[6];
            Vector3 center = HexToWorld(h);
            for (int i = 0; i < 6; i++)
            {
                corners[i] = center + HexCornerOffset(i);
            }

            return corners;
        }
    }
}