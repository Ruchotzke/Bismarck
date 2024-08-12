﻿using System;
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

        private Dictionary<Hex, T> _map;

        private Hex _lowerBounds;
        private Hex _upperBounds;
        
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
                    if (_lowerBounds.q > q) _lowerBounds = new Hex(q, _lowerBounds.r);
                    if (_lowerBounds.r > r) _lowerBounds = new Hex(_lowerBounds.q, r);
                    if (_upperBounds.q < q) _upperBounds = new Hex(q, _upperBounds.r);
                    if (_upperBounds.r < r) _upperBounds = new Hex(_upperBounds.q, r);
                    
                    _map.Add(new Hex(q, r), default(T));
                }
            }
        }

        /// <summary>
        /// Generate an empty map.
        /// </summary>
        public Map()
        {
            _map = new Dictionary<Hex, T>();
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
                        if (_lowerBounds.q > q) _lowerBounds = new Hex(q, _lowerBounds.r);
                        if (_lowerBounds.r > r) _lowerBounds = new Hex(_lowerBounds.q, r);
                        if (_upperBounds.q < q) _upperBounds = new Hex(q, _upperBounds.r);
                        if (_upperBounds.r < r) _upperBounds = new Hex(_upperBounds.q, r);
                        
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
                        if (_lowerBounds.q > q) _lowerBounds = new Hex(q, _lowerBounds.r);
                        if (_lowerBounds.r > r) _lowerBounds = new Hex(_lowerBounds.q, r);
                        if (_upperBounds.q < q) _upperBounds = new Hex(q, _upperBounds.r);
                        if (_upperBounds.r < r) _upperBounds = new Hex(_upperBounds.q, r);
                        
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

        /// <summary>
        /// Get the extents of this map.
        /// </summary>
        /// <returns></returns>
        public (Hex lower, Hex upper) GetBounds()
        {
            return (_lowerBounds, _upperBounds);
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
        public List<(Hex coord, T value)> GetHexes(int left, int right, int bottom, int top, bool pointedTop = true)
        {
            List<(Hex coord, T value)> o = new List<(Hex coord, T value)>();

            /* From https://www.redblobgames.com/grids/hexagons/implementation.html */
            if (pointedTop)
            {
                for (int r = bottom; r <= top; r++) 
                { 
                    int rOffset = Mathf.FloorToInt(r / 2.0f);
                    for (int q = left - rOffset; q <= right - rOffset; q++) 
                    {
                        try
                        {
                            o.Add((new Hex(q, r), _map[new Hex(q, r)]));
                        }
                        catch (Exception)
                        {
                            Debug.LogWarning("Caution - attempting to access hex " + new Hex(q, r) + " which is off the map.");
                        }
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
                        try
                        {
                            o.Add((new Hex(q, r), _map[new Hex(q, r)]));
                        }
                        catch (Exception)
                        {
                            Debug.LogWarning("Caution - attempting to access hex " + new Hex(q, r) + " which is off the map.");
                        }
                    }
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
            return _map[coordinate];
        }

        /// <summary>
        /// Get like the other method, but catch and return nulls/defaults for non-existent values.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        public T GetDefault(Hex coordinates)
        {
            try
            {
                return _map[coordinates];
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Set (or add) the value provided.
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="value"></param>
        public void Set(Hex coordinate, T value)
        {
            if(_map.Keys.Contains(coordinate))
            {
                _map[coordinate] = value;
            }
            else
            {
                if (_lowerBounds.q > coordinate.q) _lowerBounds = new Hex(coordinate.q, _lowerBounds.r);
                if (_lowerBounds.r > coordinate.r) _lowerBounds = new Hex(_lowerBounds.q, coordinate.r);
                if (_upperBounds.q < coordinate.q) _upperBounds = new Hex(coordinate.q, _upperBounds.r);
                if (_upperBounds.r < coordinate.r) _upperBounds = new Hex(_upperBounds.q, coordinate.r);
                _map.Add(coordinate, value);
            }
        }
        
    }
}