using UnityEngine;

namespace bismarck.world
{
    /// <summary>
    /// An individual cell in the world map.
    /// </summary>
    public class Cell
    {
        /// <summary>
        /// The color for this cell.
        /// </summary>
        public Color Color;

        /// <summary>
        /// The height for this cell.
        /// </summary>
        public int Height;


        /// <summary>
        /// Generate a new cell with height and color.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="h"></param>
        public Cell(Color c, int h)
        {
            Color = c;
            Height = h;
        }
    }
}