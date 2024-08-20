using System;
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
        public void GenerateTiles(Map<Cell> map, int iterations)
        {
            /* Determine helpful landmarks */
            var bounds = map.GetBounds();
            Hex center = new Hex((bounds.lower.q + bounds.upper.q) / 2, (bounds.lower.r + bounds.upper.r) / 2);
            
            /* Make a clone of the map for terrain generation */
            Map<TerrainGenCell> genMap = new Map<TerrainGenCell>();
            foreach (var hex in map.GetAllHexes())
            {
                float initHeight = 0;
                if (Random.value < 0.1f)
                {
                    initHeight = 3;
                }
                genMap.Set(hex.coord, new TerrainGenCell(initHeight));
            }
            
            /* Loop and do cellular automata */
            for (int loop = 0; loop < iterations; loop++)
            {
                /* Make a new working copy */
                Map<TerrainGenCell> nextMap = new Map<TerrainGenCell>();
                
                /* Iterate through each cell coordinate */
                foreach (var curr in genMap.GetAllHexes())
                {
                    /* Get info about this hex */
                    TerrainGenCell currCell = genMap.Get(curr.coord);
                    
                    /* Attempt to account for all neighbors (edges are not processed) */
                    float neighborHeights = 0;
                    for (int dir = 0; dir < 6; dir++)
                    {
                        try
                        {
                            neighborHeights += genMap.Get(curr.coord.GetNeighbor(dir)).height;
                        }
                        catch (Exception)
                        {
                            /* This is an edge. height remains zero */
                            neighborHeights = -1.0f;
                            break;
                        }
                    }

                    if (neighborHeights < 0.0f)
                    {
                        nextMap.Set(curr.coord, new TerrainGenCell(0));
                        continue;
                    }
                    
                    /* Apply automata rules */
                    float diff = currCell.height - (neighborHeights / 6);
                    float nextHeight = 0;
                    if (neighborHeights == 0)
                    {
                        /* Ocean should never become anything else on its own */
                        nextHeight = 0;
                    }
                    else if (neighborHeights < 6)
                    {
                        /* Low lying areas tend to turn into oceans */
                        nextHeight = currCell.height - Random.Range(-0.5f, 2.0f);
                    }
                    else if (Mathf.Abs(diff) < 2)
                    {
                        /* Flatish areas should fluctuate */
                        nextHeight = currCell.height + Random.Range(-1.0f, 2.0f);
                    }
                    else if (-diff > 2)
                    {
                        /* If a cell is lower than its neighbors, it should grow towards its neighbor */
                        nextHeight = currCell.height + Random.Range(0.0f, (neighborHeights / 6));
                    }
                    else
                    {
                        nextHeight = currCell.height;
                    }

                    nextHeight = Mathf.Clamp(nextHeight, 0f, 15f);
                    nextMap.Set(curr.coord, new TerrainGenCell(nextHeight));
                }
                
                /* Finally, iterate the map */
                genMap = nextMap;
            }
            
            /* Convert the map heights to cell heights */
            foreach (var hex in map.GetAllHexes())
            {
                int height = Mathf.RoundToInt(genMap.Get(hex.coord).height);

                Color c;
                if (height <= 0)
                {
                    c = new Color(0.0f, 0.0f, 0.5f);
                }
                else if (height == 1)
                {
                    c = new Color(0.0f, 0.6f, 0.6f);
                }
                else if (height == 2)
                {
                    c = new Color(1.0f, 0.8f, 0.0f);
                }
                else if(height < 4)
                {
                    c = new Color(0.0f, 0.8f, 0.0f);
                }
                else
                {
                    c = new Color(1.0f, 1.0f, 1.0f);
                }
                
                map.Set(hex.coord, new Cell(c, height));
            }
        }

        /// <summary>
        /// A job used to parallelize the cellular automata for world generation.
        /// </summary>
        private struct WorldGenJob : IJobParallelFor
        {
            public NativeArray<TerrainGenCell> input;
            public int stride;
            
            public NativeArray<TerrainGenCell> output;

            public void Execute(int index)
            {
                /* Get a reference to the proper hex for this element */
                Hex curr = IndexToHex(index);
                
                /* Attempt to account for all neighbors (edges are not processed) */
                float neighborHeights = 0;
                for (int dir = 0; dir < 6; dir++)
                {
                    try
                    {
                        /* Get the offset for this neighbor */
                        Hex neighbor = curr.GetNeighbor(dir);
                        int neighborIndex = HexToIndex(neighbor);
                        
                        /* Attempt to index and read the data */
                        neighborHeights += input[neighborIndex].height;
                    }
                    catch (Exception)
                    {
                        /* This is an edge. height remains zero */
                        neighborHeights = -1.0f;
                        break;
                    }
                }
                
                /* Error: if -1.0f height, we know this was an edge */
                if (neighborHeights < 0.0f)
                {
                    output[index] = new TerrainGenCell(0.0f);
                    return;
                }
                
                /* Apply automata rules */
                float diff = input[index].height - (neighborHeights / 6);
                float nextHeight = 0;
                if (neighborHeights == 0)
                {
                    /* Ocean should never become anything else on its own */
                    nextHeight = 0;
                }
                else if (neighborHeights < 6)
                {
                    /* Low lying areas tend to turn into oceans */
                    nextHeight = input[index].height - Random.Range(0.0f, 2.0f);
                }
                else if (Mathf.Abs(diff) < 2)
                {
                    /* Flatish areas should fluctuate */
                    nextHeight = input[index].height + Random.Range(-1.0f, 2.0f);
                }
                else if (-diff > 2)
                {
                    /* If a cell is lower than its neighbors, it should grow towards its neighbor */
                    nextHeight = input[index].height + Random.Range(0.0f, (neighborHeights / 6));
                }
                else
                {
                    nextHeight = input[index].height;
                }
                
                /* Clamp the height within an acceptable range and continue */
                nextHeight = Mathf.Clamp(nextHeight, 0f, 15f);
                input[index] = new TerrainGenCell(nextHeight);
            }

            private Hex IndexToHex(int index)
            {
                int row = index / stride;
                int col = index % stride;

                return Hex.FromOffset(row, col);
            }

            private int HexToIndex(Hex h)
            {
                var offset = h.ToOffsetCoord();

                return offset.row * stride + offset.col;
            }
        }
    }
}