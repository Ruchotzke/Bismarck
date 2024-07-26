using System.Collections.Generic;
using UnityEngine;

namespace bismarck.hex
{
    /// <summary>
    /// A hex map.
    /// </summary>
    /// <typeparam name="T">The type of object to store at each hex coordinate.</typeparam>
    public class Map<T>
    {

        private Dictionary<Hex, T> _map;
        
        /// <summary>
        /// Generate a new map in a hex shape.
        /// </summary>
        public Map(int radius)
        {
            _map = new Dictionary<Hex, T>();

            /* From https://www.redblobgames.com/grids/hexagons/implementation.html */
            for (int q = -radius; q <= radius; q++)
            {
                int r1 = Mathf.Max(-radius, -q - radius);
                int r2 = Mathf.Min(radius, -q + radius);
                for (int r = r1; r <= r2; r++)
                {
                    _map.Add(new Hex(q, r), default(T));
                }
            }
        }

        /// <summary>
        /// Generate a new map in a rectangular shape.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        public Map(int left, int right, int bottom, int top, bool pointedTop = true)
        {
            _map = new Dictionary<Hex, T>();
            
            /* From https://www.redblobgames.com/grids/hexagons/implementation.html */
            if (pointedTop)
            {
                for (int r = bottom; r <= top; r++) 
                { 
                    int rOffset = Mathf.FloorToInt(r / 2.0f);
                    for (int q = left - rOffset; q <= right - rOffset; q++) 
                    {
                        _map.Add(new Hex(q, r), default(T));
                    }
                }
            }
            else
            {
                for (int q = left; q <= right; q++) 
                {
                    int qOffset = Mathf.FloorToInt(q / 2.0f);
                    for (int r = bottom - qOffset; r <= top - qOffset; r++)
                    {
                        _map.Add(new Hex(q, r), default(T));
                    }
                }
            }
        }

        /// <summary>
        /// Return all hexes from this map.
        /// </summary>
        /// <returns></returns>
        public List<(Hex coord, T value)> GetAllHexes()
        {
            List<(Hex coord, T value)> o = new List<(Hex coord, T value)>();

            foreach (var key in _map.Keys)
            {
                o.Add((key, _map[key]));
            }
            
            return o;
        }
        
    }
}