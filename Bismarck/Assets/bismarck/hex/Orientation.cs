using UnityEngine;

namespace bismarck.hex
{
    /// <summary>
    /// An orientation helper used to store hex direction/orientation information.
    /// </summary>
    public struct Orientation
    {
        /// <summary>
        /// The forward (hex->screen) matrix elements.
        /// </summary>
        public readonly float F0, F1, F2, F3;
        
        /// <summary>
        /// The backward (screen->hex) matrix elements.
        /// </summary>
        public readonly float B0, B1, B2, B3;

        /// <summary>
        /// The starting angle of the hexagons (in multiples of 60 degrees).
        /// </summary>
        public readonly float StartAngle;

        public Orientation(float f0, float f1, float f2, float f3, float b0, float b1, float b2, float b3, float angle)
        {
            F0 = f0;
            F1 = f1;
            F2 = f2;
            F3 = f3;
            B0 = b0;
            B1 = b1;
            B2 = b2;
            B3 = b3;
            StartAngle = angle;
        }

        public static Orientation layoutPointTop = new Orientation(
            Mathf.Sqrt(3), Mathf.Sqrt(3) / 2.0f, 0.0f, 3.0f / 2.0f,
            Mathf.Sqrt(3) / 3.0f, -1.0f / 3.0f, 0.0f, 2.0f / 3.0f, 
            0.5f);

        public static Orientation layoutFlatTop = new Orientation(
            3.0f / 2.0f, 0.0f, Mathf.Sqrt(3.0f) / 2.0f, Mathf.Sqrt(3.0f),
            2.0f / 3.0f, 0.0f, -1.0f / 3.0f, Mathf.Sqrt(3.0f) / 3.0f, 
            0.0f);
    }
}