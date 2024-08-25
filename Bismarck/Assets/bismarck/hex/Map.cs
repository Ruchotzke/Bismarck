using System;
using System.Collections.Generic;
using System.Linq;
using bismarck.world;
using Unity.Collections;
using UnityEngine;

namespace bismarck.hex
{
    /// <summary>
    /// A hex map.
    /// </summary>
    public class Map
    {

        /// <summary>
        /// The map containing the items.
        /// </summary>
        private Cell[,] _map;

        public int RowSize;

        public int ColSize;
        
        /// <summary>
        /// Generate an empty map.
        /// </summary>
        public Map(int rows, int cols)
        {
            _map = new Cell[rows,cols];

            RowSize = rows;
            ColSize = cols;
        }

        /// <summary>
        /// Return all hexes from this map.
        /// </summary>
        /// <returns></returns>
        public List<(Hex coord, Cell value)> GetAllHexes()
        {
            List<(Hex coord, Cell value)> o = new List<(Hex coord, Cell value)>();

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
        public List<(Hex coord, Cell value)> GetHexes(int left, int right, int bottom, int top)
        {
            List<(Hex coord, Cell value)> o = new List<(Hex coord, Cell value)>();

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
        public Cell Get(Hex coordinate)
        {
            var offset = coordinate.ToOffsetCoord();
            return _map[offset.row, offset.col];
        }

        /// <summary>
        /// Direct map accessor
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c"></param>
        public Cell this[int r, int c]
        {
            get => _map[r, c];
            set => _map[r, c] = value;
        }

        public Cell this[Hex h, bool nullOOB = false]
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
                        return default(Cell);
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

        /// <summary>
        /// Convert this map into a parallel-friendly native array.
        /// </summary>
        /// <returns></returns>
        public (NativeArray<(Hex coord, Cell.CellValueStruct data)> arr, int colSize, int rowSize) ToNativeArray()
        {
            NativeArray<(Hex coord, Cell.CellValueStruct data)> arr =
                new NativeArray<(Hex coord, Cell.CellValueStruct data)>(ColSize * RowSize, Allocator.TempJob);

            /* Add all entries into the array using row-major layout */
            for (int r = 0; r < RowSize; r++)
            {
                for (int c = 0; c < ColSize; c++)
                {
                    int index = r * ColSize + c;
                    Cell curr = this[r, c];
                    arr[index] = (Hex.FromOffset(r, c), new Cell.CellValueStruct()
                    {
                        Color = curr.Color,
                        Height = curr.Height
                    });
                }
            }
            
            /* Return useful information */
            return (arr, ColSize, RowSize);
        }

        /// <summary>
        /// Update the contents of this map from a correctly generated native array.
        /// </summary>
        /// <param name="arr"></param>
        public void UpdateFromNativeArray(NativeArray<(Hex coord, Cell.CellValueStruct data)> arr)
        {
            foreach (var item in arr)
            {
                this[item.coord] = new Cell(item.data.Color, item.data.Height);
            }
        }

    }
}