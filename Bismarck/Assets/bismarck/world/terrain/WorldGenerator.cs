﻿using System;
using bismarck.hex;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
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
            var bounds = map.GetBounds();
            Hex center = new Hex((bounds.lower.q + bounds.upper.q) / 2, (bounds.lower.r + bounds.upper.r) / 2);
            
            /* Determine seed */
            Vector3 seedPoint = Random.insideUnitCircle * Random.Range(0f, 100f);
            
            /* Initialize noise */
            FastNoiseLite noise = new FastNoiseLite();
            noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            
            /* Generate a circle */
            foreach (var hex in map.GetAllHexes())
            {
                /* Sample this wrt world coordinate */
                Vector3 worldCoord = hexLayout.HexToWorld(hex.coord);
                worldCoord = seedPoint + (WorldManager.Instance.SampleScale * worldCoord);
                float sample = noise.GetNoise(worldCoord.x, worldCoord.z) + 1.0f;
                sample /= 2;
                sample = WorldManager.Instance.DistributionCurve.Evaluate(sample);
                sample *= 6;
                
                /* Modulate the coordinate based on distance */
                // float dist = Hex.Distance(hex.coord, center);
                // sample = WorldManager.Instance.WorldShapeCurve.Evaluate(dist / 50.0f) * sample;
                
                /* Convert */
                sample = Mathf.Clamp(sample, 0.0f, 10.0f);
                int conv = Mathf.RoundToInt(sample);
                Cell ncell = new Cell(Color.magenta, conv);
                switch (conv)
                {
                    case 0:
                        ncell.Color = new Color(0.0f, 0.2f, 0.8f);
                        map.Set(hex.coord, ncell);
                        break;
                    case 1:
                        ncell.Color = new Color(0.8f, 0.8f, 0.0f);
                        map.Set(hex.coord, ncell);
                        break;
                    case 2:
                    case 3:
                    case 4:
                        ncell.Color = new Color(0.0f, 0.7f, 0.2f);
                        map.Set(hex.coord, ncell);
                        break;
                    default:
                        ncell.Color = new Color(1.0f, 1.0f, 1.0f);
                        map.Set(hex.coord, ncell);
                        break;
                }
            }
        }
    }
}