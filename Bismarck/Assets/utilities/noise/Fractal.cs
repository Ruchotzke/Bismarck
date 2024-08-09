using System.Collections.Generic;
using UnityEngine;

namespace utilities.noise
{
    /// <summary>
    /// Generate fractal noise from successive noise.
    /// </summary>
    public class Fractal
    {
        /// <summary>
        /// How many octaves there are.
        /// </summary>
        public readonly int Octaves;

        /// <summary>
        /// How persistent each octave is.
        /// </summary>
        public readonly float Persistence;

        /// <summary>
        /// The perlin noise generator.
        /// </summary>
        private Perlin _perlin;

        /// <summary>
        /// Octave parameters for easier computation.
        /// </summary>
        private List<(float amplitude, float frequency)> _octaves;

        /// <summary>
        /// The max value of the noise (used for normalizing).
        /// </summary>
        private float _maxValue;

        /// <summary>
        /// Generate a new fractal noise generator.
        /// </summary>
        /// <param name="octaves"></param>
        /// <param name="persistence"></param>
        public Fractal(int octaves, float persistence)
        {
            /* Parms */
            Octaves = octaves;
            Persistence = persistence;
            
            /* Noise generator */
            _perlin = new Perlin();
            
            /* Octave information */
            _octaves = new List<(float amplitude, float frequency)>();
            _maxValue = 0;
            for (int i = 0; i < octaves; i++)
            {
                _maxValue += Mathf.Pow(persistence, i);
                _octaves.Add((Mathf.Pow(persistence, i), Mathf.Pow(2, i)));
            }
        }

        /// <summary>
        /// Sample the noise at a given point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public float Sample(Vector3 point)
        {
            float total = 0;
            for (int i = 0; i < Octaves; i++)
            {
                total += _perlin.Sample(point * _octaves[i].frequency) * _octaves[i].amplitude;
            }

            return total / _maxValue;
        }
    }
}