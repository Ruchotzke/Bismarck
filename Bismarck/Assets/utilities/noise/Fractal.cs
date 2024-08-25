using UnityEngine;

namespace utilities.noise
{

    /// <summary>
    /// Fractal noise sampling.
    /// </summary>
    public class Fractal
    {
        private float _lacunarity;

        private float _persistence;

        private int _octaves;

        private FastNoiseLite _noise;
        
        

        public Fractal(float lacunarity, float persistence, int octaves)
        {
            _lacunarity = lacunarity;
            _persistence = persistence;
            _octaves = octaves;
            
            _noise = new FastNoiseLite();
            _noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        }

        /// <summary>
        /// Sample fractal noise from a plane.
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="seedFrequency"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public float Sample(Vector2 coordinate, float seedFrequency = 1, int seed = 0)
        {
            float amplitude = 1;
            float frequency = seedFrequency;
            float totalAmp = 0;

            float sample = 0.0f;

            for (int o = 0; o < _octaves; o++)
            {
                /* Perform the sample */
                float singleSample = _noise.GetNoise(coordinate.x * frequency + seed, coordinate.y * frequency + seed);
                singleSample = (singleSample + 1) / 2.0f;
                singleSample *= amplitude;

                sample += singleSample;
                
                /* Update state */
                seed *= Mathf.FloorToInt(Random.value * 142342.0f - 1231.0f);
                totalAmp += amplitude;
                amplitude *= _persistence;
                frequency *= _lacunarity;
            }

            return sample / totalAmp;
        }

        /// <summary>
        /// Sample fractal noise on the surface of a cylinder.
        /// </summary>
        /// <param name="seedAngularRadius"></param>
        /// <param name="seed"></param>
        /// <param name="y"></param>
        /// <param name="seedVerticalFrequency"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public float SampleCylinder(float angle, float y, float seedVerticalFrequency, float seedAngularRadius, Vector3 seed)
        {
            float amplitude = 1;
            float vertFrequency = seedVerticalFrequency;
            float angFrequency = seedAngularRadius;
            float totalAmp = 0;

            float sample = 0.0f;

            for (int o = 0; o < _octaves; o++)
            {
                /* Compute the x and z coordinate */
                float x = Mathf.Cos(angle) * angFrequency;
                float z = Mathf.Sin(angle) * angFrequency;
                
                /* Perform the sample */
                float singleSample = _noise.GetNoise(x + seed.x, y * vertFrequency + seed.y, z + seed.z);
                singleSample = (singleSample + 1) / 2.0f;
                singleSample *= amplitude;

                sample += singleSample;
                
                /* Update state */
                seed.y += Random.value * 1313 - 500.0f;
                seed.x += Random.value * 1313 - 500.0f;
                seed.z = seed.x;
                totalAmp += amplitude;
                amplitude *= _persistence;
                vertFrequency *= _lacunarity;
                angFrequency *= _lacunarity;
            }

            return sample / totalAmp;
        }
    }
}