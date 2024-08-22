using UnityEngine;
using utilities.noise;

namespace bismarck.world.terrain
{
    
    /// <summary>
    /// A generator used to aid in world generation.
    /// </summary>
    public class TerrainGenerator
    {
        /// <summary>
        /// The elevation sampler.
        /// </summary>
        private Fractal _elevation;
        
        /// <summary>
        /// The moisture sampler.
        /// </summary>
        private Fractal _moisture;

        /// <summary>
        /// The maximum elevation generated.
        /// </summary>
        private int _maxElevation;

        /// <summary>
        /// The total scale of the terrain (1 is sampled at real size, 0.5 at half, and so on).
        /// </summary>
        private float _terrainScale;

        /// <summary>
        /// The exponential falloff used for height.
        /// </summary>
        private float _heightfalloff;

        public TerrainGenerator(int maxElevation, float terrainScale, float heightFalloff)
        {
            /* Generate the samplers */
            _elevation = new Fractal(1, 1, 1);
            _moisture = new Fractal(1, 1, 1);

            _maxElevation = maxElevation;
            _terrainScale = terrainScale;
            _heightfalloff = heightFalloff;
        }

        /// <summary>
        /// Sample the terrain generator.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public (int height, Color color) Sample(Vector3 position)
        {
            float elevation = _elevation.Sample(position * _terrainScale);
            elevation = Mathf.Pow(elevation, _heightfalloff) * _maxElevation;

            float moisture = _moisture.Sample(position * _terrainScale);
            moisture = Mathf.Clamp01(Mathf.RoundToInt(moisture * 4) / 5.0f);

            Color c;
            
            if (elevation < 0.5f)
            {
                if (moisture < 0.5f)
                {
                    c = Color.Lerp(Color.gray, Color.green, moisture * 2);
                }
                else
                {
                    c = Color.blue;
                }
            }
            else
            {
                if (moisture < 0.5f)
                {
                    c = Color.Lerp(Color.green, Color.gray, moisture * 2);
                }
                else
                {
                    c = Color.Lerp(Color.green, Color.white, (moisture - 0.5f) * 2);
                }
            }

            return (Mathf.RoundToInt(elevation), c);
        }
    }
}