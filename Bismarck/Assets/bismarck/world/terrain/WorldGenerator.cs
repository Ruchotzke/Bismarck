using System;
using bismarck.hex;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using utilities;
using utilities.noise;
using Random = UnityEngine.Random;

namespace bismarck.world.terrain
{
    /// <summary>
    /// A more primitive world generator, used to generate functional worlds.
    /// This is used without typical perlin-noise generation.
    /// </summary>
    public class WorldGenerator
    {
        /// <summary>
        /// Sample the generator for a given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public void GenerateTiles(Map<Cell> map, Layout hexLayout)
        {
            /* Determine helpful landmarks */
            Hex center = Hex.FromOffset(map.RowSize / 2, map.ColSize / 2);
            Vector3 centerWorldCoord = hexLayout.HexToWorld(center);
            float zScale = Vector3.Distance(centerWorldCoord, hexLayout.HexToWorld(Hex.FromOffset(map.RowSize / 2, 0)));
            
            /* Initialize noise */
            Fractal noise = new Fractal(WorldManager.Instance.Lacunarity, WorldManager.Instance.Persistence, WorldManager.Instance.Octaves);
            
            /* Generate a circle */
            foreach (var hex in map.GetAllHexes())
            {
                /* Sample from a cylinder for horizontal wrapping */
                float angle = Mathf.PI * 2 * (hex.coord.ToOffsetCoord().col / (float) map.ColSize);
                
                /* Sample this wrt world coordinate for elevation*/
                Vector3 hexWorldCoord = hexLayout.HexToWorld(hex.coord);
                Vector3 sampleCoord = WorldManager.Instance.VerticalSampleScale * hexWorldCoord;
                float elevationSample = noise.SampleCylinder(angle, sampleCoord.z, WorldManager.Instance.VerticalSampleScale,
                    WorldManager.Instance.AngularSampleScale, WorldManager.Instance.Seed);
                elevationSample = WorldManager.Instance.DistributionCurve.Evaluate(elevationSample);
                elevationSample *= 6;
                
                /* Get the distance from the equator */
                float distanceToEquator =
                    Mathf.Clamp01(Mathf.Abs(centerWorldCoord.z - hexWorldCoord.z) / zScale * 1.5f);
                
                /* Force more oceans further south */
                elevationSample -= Mathf.Lerp(0.0f, 3.0f, Easing.EaseOutExpo(distanceToEquator));
                
                /* Sample moisture */
                float moistureSample = noise.SampleCylinder(angle, sampleCoord.z, WorldManager.Instance.VerticalSampleScale,
                    WorldManager.Instance.AngularSampleScale, WorldManager.Instance.Seed * 4);
                if (elevationSample < 0.5f) moistureSample = 1.0f;
                
                /* Sample heat */
                float heatSample = 1f - distanceToEquator;
                
                /* Convert */
                elevationSample = Mathf.Clamp(elevationSample, 0.0f, 10.0f);
                int conv = Mathf.RoundToInt(elevationSample);
                Cell ncell = new Cell(Color.magenta, conv);
                switch (conv)
                {
                    case 0:
                        ncell.Color = new Color(0.0f, 0.2f, 0.8f);
                        map[hex.coord] = ncell;
                        break;
                    case 1:
                        ncell.Color = new Color(0.8f, 0.8f, 0.0f);
                        map[hex.coord] = ncell;
                        break;
                    case 2:
                    case 3:
                    case 4:
                        ncell.Color = new Color(0.0f, 0.7f, 0.2f);
                        map[hex.coord] = ncell;
                        break;
                    default:
                        ncell.Color = new Color(1.0f, 1.0f, 1.0f);
                        map[hex.coord] = ncell;
                        break;
                }

                if (heatSample < 0.005f)
                {
                    ncell.Color = Color.white;
                    ncell.Height = 5;
                }
                else
                {
                    // ncell.Color = Color.HSVToRGB(heatSample, 1, 1);
                }
                
                
            }
        }
    }
}