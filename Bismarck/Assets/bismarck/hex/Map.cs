using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace bismarck.hex
{
    /// <summary>
    /// A hex map.
    /// </summary>
    /// <typeparam name="T">The type of object to store at each hex coordinate.</typeparam>
    public class Map<T>
    {

        /// <summary>
        /// The map containing the items.
        /// </summary>
        private T[,] _map;

        public int RowSize;

        public int ColSize;
        
        /// <summary>
        /// Generate an empty map.
        /// </summary>
        public Map(int rows, int cols)
        {
            _map = new T[rows,cols];

            RowSize = rows;
            ColSize = cols;
        }

        /// <summary>
        /// Return all hexes from this map.
        /// </summary>
        /// <returns></returns>
        public List<(Hex coord, T value)> GetAllHexes()
        {
            List<(Hex coord, T value)> o = new List<(Hex coord, T value)>();

            for (int r = 0; r < _map.GetLength(0); r++)
            {
                for (int c = 0; c < _map.GetLength(1); c++)
                {
                    Hex coord = Hex.FromOffset(r, c);
                    o.Add((coord, _map[r, c]));
                }
            }
            
            return o;
        }

        /// <summary>
        /// Get all hexes in the provided range.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <param name="top"></param>
        /// <param name="pointedTop"></param>
        /// <returns></returns>
        public List<(Hex coord, T value)> GetHexes(int left, int right, int bottom, int top)
        {
            List<(Hex coord, T value)> o = new List<(Hex coord, T value)>();

            for (int r = bottom; r <= top; r++)
            {
                for (int c = left; c <= right; c++)
                {
                    Hex coord = Hex.FromOffset(r, c);
                    o.Add((coord, _map[r, c]));
                }
            }
            
            return o;
        }

        /// <summary>
        /// Get the element at a given index.
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public T Get(Hex coordinate)
        {
            var offset = coordinate.ToOffsetCoord();
            return _map[offset.row, offset.col];
        }

        /// <summary>
        /// Direct map accessor
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c"></param>
        public T this[int r, int c]
        {
            get => _map[r, c];
            set => _map[r, c] = value;
        }

        public T this[Hex h, bool nullOOB = false]
        {
            get
            {
                if (nullOOB)
                {
                    try
                    {
                        var offset = h.ToOffsetCoord();
                        return _map[offset.row, offset.col];
                    }
                    catch (Exception)
                    {
                        return default(T);
                    } 
                }
                else
                {
                    var offset = h.ToOffsetCoord();
                    return _map[offset.row, offset.col];
                }
            }
            set
            {
                var offset = h.ToOffsetCoord();
                _map[offset.row, offset.col] = value;
            }
        }

    }
}