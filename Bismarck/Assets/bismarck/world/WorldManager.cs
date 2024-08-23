using System;
using System.Collections;
using System.Collections.Generic;
using bismarck.meshing;
using UnityEngine;
using Random = UnityEngine.Random;

namespace bismarck.world
{
    /// <summary>
    /// The manager for the game world.
    /// </summary>
    public class WorldManager : MonoBehaviour
    {
        #region SINGLETON

        public static WorldManager Instance;

        #endregion

        #region RENDERING

        [Header("Rendering")] public Material WorldMaterial;

        #endregion
        
        #region WORLD
        
        /// <summary>
        /// The size of the world in chunks (mirrored across axes, so double this).
        /// </summary>
        [Header("World")] public Vector2Int WorldSize = new Vector2Int(5, 5);
        
        /// <summary>
        /// The world data.
        /// </summary>
        private World _world;

        /// <summary>
        /// Chunk objects making up this world.
        /// </summary>
        private Chunk[,] _chunks;

        /// <summary>
        /// The distribution curve for world generation.
        /// </summary>
        [Header("WorldGen")] public AnimationCurve DistributionCurve;

        /// <summary>
        /// The curve shaping the world's height over a range.
        /// </summary>
        public AnimationCurve WorldShapeCurve;

        /// <summary>
        /// The scale used when sampling noise along the latitudes.
        /// </summary>
        public float VerticalSampleScale = 6f;

        /// <summary>
        /// The radius used when sampling along the longitudes.
        /// </summary>
        public float AngularSampleScale = 30f;

        public Vector3 Seed;

        [Range(0, 20)] public float Lacunarity = 1;

        [Range(0, 1)] public float Persistence = 1;

        public int Octaves = 1;

        #endregion
        
        private void Awake()
        {
            /* Initialize the singleton */
            if (Instance != null) Destroy(gameObject);
            Instance = this;
            
            /* Initialize the world seed */
            Random.InitState((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            WorldConfiguration.SEED = DateTime.Now.Second + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year +
                                      DateTime.Now.Millisecond + Random.Range(-50000, 50000);
            Seed = new Vector3(WorldConfiguration.SEED, WorldConfiguration.SEED, WorldConfiguration.SEED);
        }

        private void Start()
        {
            /* Generate a new world */
            _world = new World(WorldSize.x, WorldSize.y);
            _chunks = new Chunk[WorldSize.x,WorldSize.y];

            /* Instantiate the chunks */
            for(int xChunk = 0; xChunk < WorldSize.x; xChunk++)
            {
                for(int yChunk = 0; yChunk < WorldSize.y; yChunk++)
                {
                    var go = new GameObject("chunk " + xChunk + ", " + yChunk);
                    go.transform.parent = transform;
                    _chunks[xChunk, yChunk] = go.AddComponent<Chunk>();

                    UpdateChunk(xChunk, yChunk);
                }
            }
        }

        public void RegenWorld()
        {
            _world = new World(WorldSize.x, WorldSize.y);
            
            UpdateAll();
        } 

        /// <summary>
        /// Force a render update for a given chunk.
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkY"></param>
        public void UpdateChunk(int chunkX, int chunkY)
        {
            Mesher m = new Mesher();
            var bounds = _world.GetChunkBounds(chunkX, chunkY);
            _world.GenerateMesh(m, bounds.left, bounds.right, bounds.bot, bounds.top);
            _chunks[chunkX, chunkY]
                .Initialize(WorldMaterial, m.GenerateMesh(true));
        }

        public void UpdateAll()
        {
            for(int xChunk = 0; xChunk < WorldSize.x; xChunk++)
            {
                for(int yChunk = 0; yChunk < WorldSize.y; yChunk++)
                {
                    UpdateChunk(xChunk, yChunk);
                }
            }
        }
    }
}

