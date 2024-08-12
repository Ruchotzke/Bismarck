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
            
            /* Convert into a native array so we can use the job system */
            int rowSize = 40;
            int colSize = 40;
            NativeArray<TerrainGenCell> bufferA = new NativeArray<TerrainGenCell>(rowSize * colSize, Allocator.TempJob,
                NativeArrayOptions.UninitializedMemory);
            NativeArray<TerrainGenCell> bufferB = new NativeArray<TerrainGenCell>(rowSize * colSize, Allocator.TempJob,
                NativeArrayOptions.UninitializedMemory);
            
            /* Insert all cells into the array */
            foreach (var hex in genMap.GetAllHexes())
            {
                var offset = hex.coord.ToOffsetCoord();
                int index = offset.row * rowSize + offset.col;
                bufferA[index] = hex.value;
            }
            
            /* Loop and do cellular automata */
            bool aIsInput = true;
            for (int loop = 0; loop < iterations; loop++)
            {
                /* Generate a new job, swapping input/output */
                WorldGenJob genJob = new WorldGenJob()
                {
                    input = aIsInput ? bufferA : bufferB,
                    output = aIsInput ? bufferB : bufferA,
                    rowCount = rowSize,
                    colCount = colSize
                };
                
                /* Process the job */
                JobHandle handle = genJob.Schedule(rowSize * colSize, 20);
                handle.Complete();
                
                /* Swap the buffers */
                aIsInput = !aIsInput;
            }
            
            /* Write the heights back to the gen map */
            for (int row = 0; row < rowSize; row++)
            {
                for (int col = 0; col < colSize; col++)
                {
                    int index = col + row * rowSize;
                    Hex h = Hex.FromOffset(row, col);
                    
                    genMap.Set(h, new TerrainGenCell((aIsInput ? bufferA : bufferB)[index].height));
                }
            }
            
            /* Let the scheduler know it can free buffers */
            bufferA.Dispose();
            bufferB.Dispose();
            Debug.Log("Iterated");
            
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
            [ReadOnly] public NativeArray<TerrainGenCell> input;
            [ReadOnly] public int rowCount;
            [ReadOnly] public int colCount;
            
            public NativeArray<TerrainGenCell> output;

            public void Execute(int index)
            {
                /* Get a reference to the proper hex for this element */
                Hex curr = IndexToHex(index);
                Debug.Log(curr.ToOffsetCoord() + " " + curr);
                
                /* Attempt to account for all neighbors (edges are not processed) */
                if (index / colCount == 0 || index % colCount == 0 || index / colCount == rowCount - 1 || index % colCount == colCount - 1)
                {
                    /* This is an edge - immediately cancel */
                    output[index] = new TerrainGenCell(0);
                }
                
                /* With all edges removed, we can now work normally without fear of OOB accesses */
                float neighborHeights = 0;
                for (int dir = 0; dir < 6; dir++)
                {
                    /* Get the offset for this neighbor */
                    Hex neighbor = curr.GetNeighbor(dir);
                    int neighborIndex = HexToIndex(neighbor);
                    
                    /* Attempt to index and read the data */
                    neighborHeights += input[neighborIndex].height;
                }
                
                /* Error: if -1.0f height, we know this was an edge */
                if (neighborHeights < 0.0f)
                {
                    output[index] = new TerrainGenCell(3.0f);
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
                    nextHeight = input[index].height - 1;
                }
                else if (Mathf.Abs(diff) < 2)
                {
                    /* Flatish areas should fluctuate */
                    nextHeight = input[index].height + 0.5f;
                }
                else if (-diff > 2)
                {
                    /* If a cell is lower than its neighbors, it should grow towards its neighbor */
                    nextHeight = input[index].height + 2;
                }
                else
                {
                    nextHeight = input[index].height;
                }
                
                /* Clamp the height within an acceptable range and continue */
                nextHeight = Mathf.Clamp(nextHeight, 0f, 15f);
                output[index] = new TerrainGenCell(nextHeight);
            }

            private Hex IndexToHex(int index)
            {
                int row = index / colCount;
                int col = index % colCount;

                return Hex.FromOffset(row, col);
            }

            private int HexToIndex(Hex h)
            {
                var offset = h.ToOffsetCoord();

                return offset.row * colCount + offset.col;
            }
        }
    }
}