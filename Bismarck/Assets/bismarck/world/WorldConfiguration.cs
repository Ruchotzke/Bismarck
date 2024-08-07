﻿namespace bismarck.world
{
    /// <summary>
    /// A static class used to contain configuration values accessible anywhere.
    /// </summary>
    public class WorldConfiguration
    {

        /// <summary>
        /// What percent of each hex should be dedicated to edges/blend areas.
        /// </summary>
        public const float BLEND_REGION_SCALE = 0.2f;

        /// <summary>
        /// How much height should be added per integer height.
        /// </summary>
        public const float HEIGHT_MULTPLIER = 0.5f;

        /// <summary>
        /// The number of terraces between adjacent heights.
        /// </summary>
        public const int NUM_TERRACES = 5;
    }
}