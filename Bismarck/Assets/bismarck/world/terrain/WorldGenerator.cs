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
        public void GenerateTiles(Map map, Layout hexLayout)
        {
            /* Determine helpful landmarks */
            Hex center = Hex.FromOffset(map.RowSize / 2, map.ColSize / 2);
            Vector3 centerWorldCoord = hexLayout.HexToWorld(center);
            float zScale = Vector3.Distance(centerWorldCoord, hexLayout.HexToWorld(Hex.FromOffset(map.RowSize / 2, 0)));
            
            /* Initialize noise */
            Fractal noise = new Fractal(WorldManager.Instance.Lacunarity, WorldManager.Instance.Persistence, WorldManager.Instance.Octaves);
            
            /* Generate the initial map */
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
            }
            
            /* Now that an initial landmass set has been generated */
            /* Smooth out very rough features */
            /* First make everything parallel friendly */
            var pFriendly = map.ToNativeArray();
            NativeArray<(Hex coord, Cell.CellValueStruct data)> output =
                new NativeArray<(Hex coord, Cell.CellValueStruct data)>(map.ColSize * map.RowSize, Allocator.TempJob);

            var job = new MapKernelParallel()
            {
                Map = pFriendly.arr,
                ColSize = pFriendly.colSize,
                RowSize = pFriendly.rowSize,
                Radius = 1,
                Output = output
            };
            
            /* Execute the job */
            var handle = job.Schedule(pFriendly.arr.Length, 10);
            handle.Complete();  //block
            
            /* Apply the values back to the original map */
            map.UpdateFromNativeArray(output);
            
            /* Free the arrays */
            pFriendly.arr.Dispose();
            output.Dispose();
            
            /* Update Colors */
            foreach (var i in map.GetAllHexes())
            {
                switch (i.value.Height)
                {
                    case 0:
                        i.value.Color = new Color(0.0f, 0.2f, 0.8f);
                        break;
                    case 1:
                        i.value.Color = new Color(0.8f, 0.8f, 0.0f);
                        break;
                    case 2:
                    case 3:
                    case 4:
                        i.value.Color = new Color(0.0f, 0.7f, 0.2f);
                        break;
                    default:
                        i.value.Color = new Color(1.0f, 1.0f, 1.0f);
                        break;
                }
            }
        }

        struct MapKernelParallel : IJobParallelFor
        {
            [ReadOnly] public NativeArray<(Hex coord, Cell.CellValueStruct data)> Map;

            [ReadOnly] public int ColSize;

            [ReadOnly] public int RowSize;

            [ReadOnly] public int Radius;

            public NativeArray<(Hex coord, Cell.CellValueStruct data)> Output;
            
            public void Execute(int index)
            {
                /* Collect the information for this cell */
                Hex coord = Map[index].coord;
                Cell.CellValueStruct cell = Map[index].data;
                
                /* Smooth the world. Average heights */
                float height = 0;
                int count = 0;
                foreach (var hex in coord.GetRange(Radius))
                {
                    if (hex == coord) continue; //inclusive is annoying
                    int hidx = HexToIndex(hex);

                    if (hidx >= 0)
                    {
                        height += Map[hidx].data.Height;
                        count += 1;
                    }
                }

                height /= count;
                
                /* Set the output */
                Cell.CellValueStruct oCell = new Cell.CellValueStruct()
                {
                    Color = cell.Color,
                    Height = Mathf.FloorToInt(height)
                };
                Output[index] = (coord, oCell);
            }

            /// <summary>
            /// Convert a hex to an index, respecting boundaries.
            /// </summary>
            /// <param name="h"></param>
            /// <returns></returns>
            private int HexToIndex(Hex h)
            {
                /* Convert to offset */
                var offset = h.ToOffsetCoord();

                /* Determine if in bounds */
                if (offset.col < 0 || offset.col >= ColSize)
                {
                    return -1;
                }

                if (offset.row < 0 || offset.row >= RowSize)
                {
                    return -1;
                }

                /* Return */
                return offset.row * ColSize + offset.col;
            }
        }
    }
}