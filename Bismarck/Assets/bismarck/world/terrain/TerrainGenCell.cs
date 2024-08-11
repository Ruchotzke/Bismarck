namespace bismarck.world.terrain
{
    /// <summary>
    /// A cell used to store information during terrain generation.
    /// </summary>
    public struct TerrainGenCell
    {
        public readonly float height;

        public TerrainGenCell(float h)
        {
            height = h;
        }
    }
}