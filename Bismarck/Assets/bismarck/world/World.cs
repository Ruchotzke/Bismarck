using bismarck.hex;
using bismarck.meshing;
using TMPro;
using UnityEngine;

namespace bismarck.world
{
    /// <summary>
    /// A world is the higher-level representation of a map.
    /// </summary>
    public class World
    {
        /// <summary>
        /// The layout/Orientation of the world.
        /// </summary>
        public Layout HexLayout;

        /// <summary>
        /// The map containing world data.
        /// </summary>
        public Map<Cell> Map;
        
        /// <summary>
        /// A reference to the prefab needed for cell labels.
        /// </summary>
        public TextMeshPro pf_Label;

        /// <summary>
        /// Instantiate a new world.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <param name="top"></param>
        public World(int left, int right, int bottom, int top)
        {
            /* Generate the layout and map */
            HexLayout = new Layout(Orientation.layoutPointTop, new Vector3(1, 0, 1), Vector3.zero);
            Map = new Map<Cell>(left, right, bottom, top);
            
            /* Add some initial data */
            foreach (var hex in Map.GetAllHexes())
            {
                int height = Random.Range(0, 4);
                Color c = height > 0 ? Color.green : Color.blue;
                Map.Set(hex.coord, new Cell(c, height));
            }

        }

        /// <summary>
        /// Generate the mesh for this map.
        /// </summary>
        /// <param name="mesher"></param>
        public void GenerateMesh(Mesher mesher)
        {
            foreach (var hex in Map.GetAllHexes())
            {
                GenerateHex(mesher, hex.coord, hex.value);
                GenerateBridges(mesher, hex.coord, hex.value);
                GenerateCorners(mesher, hex.coord, hex.value);
            }
        }

        /// <summary>
        /// Generate the mesh for the flat central portion of a single mesh.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="coord"></param>
        /// <param name="cell"></param>
        private void GenerateHex(Mesher m, Hex coord, Cell cell)
        {
            /* Get the corners for the center hex */
            Vector3[] corners = HexLayout.GenerateCorners(coord, 1f - WorldConfiguration.BLEND_REGION_SCALE);
            Vertex[] c = new Vertex[6];
            for (int i = 0; i < 6; i++)
            {
                corners[i] += Vector3.up * cell.Height * WorldConfiguration.HEIGHT_MULTPLIER;
                c[i] = new Vertex(corners[i], color:cell.Color);
            }
        
            /* Triangulate as a fan */
            m.AddFan(c, false);
        
            /* Add a label */
            var label = Object.Instantiate(pf_Label);
            label.transform.position = HexLayout.HexToWorld(coord) + new Vector3(0f, .05f, 0f) + Vector3.up * cell.Height * WorldConfiguration.HEIGHT_MULTPLIER;
            label.text = "<" + coord.q + "," + coord.r + "," + coord.s + ">";
        }

        /// <summary>
        /// Generate the bridges through the blend regions connecting adjacent cells.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="coord"></param>
        /// <param name="cell"></param>
        private void GenerateBridges(Mesher m, Hex coord, Cell cell)
        {
            /* We need a bridge in left, upper left, and upper directions (0, -1, -2) */
            Vector3 baseHeight = Vector3.up * cell.Height * WorldConfiguration.HEIGHT_MULTPLIER;
            
            /* Direction 0 */
            /* Determine whether a viable neighbor exists */
            Hex nHex = coord.GetNeighbor(0);
            Cell nValue = Map.Get(nHex);
            if (nValue != null)
            {
                Vector3 neighborHeight = Vector3.up * nValue.Height * WorldConfiguration.HEIGHT_MULTPLIER;
                
                /* Neighbor exists, so generate a bridge */
                Vector3 a = HexLayout.HexCornerOffset(0) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(coord) + baseHeight;
                Vector3 b = HexLayout.HexCornerOffset(-1) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(coord) + baseHeight;
                Vector3 c = HexLayout.HexCornerOffset(3) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(nHex) + neighborHeight;
                Vector3 d = HexLayout.HexCornerOffset(2) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(nHex) + neighborHeight;
                
                /* ABC and ACD will be the two triangles generated */
                Vertex va = new Vertex(a, color: cell.Color);
                Vertex vb = new Vertex(b, color: cell.Color);
                Vertex vc = new Vertex(c, color: nValue.Color);
                Vertex vd = new Vertex(d, color: nValue.Color);

                m.AddTriangle(va, vb, vc);
                m.AddTriangle(va, vc, vd);
            }
            
            /* Direction -1 */
            /* Determine whether a viable neighbor exists */
            nHex = coord.GetNeighbor(-1);
            nValue = Map.Get(nHex);
            if (nValue != null)
            {
                Vector3 neighborHeight = Vector3.up * nValue.Height * WorldConfiguration.HEIGHT_MULTPLIER;
                
                /* Neighbor exists, so generate a bridge */
                Vector3 a = HexLayout.HexCornerOffset(-1) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(coord) + baseHeight;
                Vector3 b = HexLayout.HexCornerOffset(-2) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(coord) + baseHeight;
                Vector3 c = HexLayout.HexCornerOffset(2) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(nHex) + neighborHeight;
                Vector3 d = HexLayout.HexCornerOffset(1) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(nHex) + neighborHeight;
                
                /* ABC and ACD will be the two triangles generated */
                Vertex va = new Vertex(a, color: cell.Color);
                Vertex vb = new Vertex(b, color: cell.Color);
                Vertex vc = new Vertex(c, color: nValue.Color);
                Vertex vd = new Vertex(d, color: nValue.Color);
            
                m.AddTriangle(va, vb, vc);
                m.AddTriangle(va, vc, vd);
            }
            
            /* Direction -2 */
            /* Determine whether a viable neighbor exists */
            nHex = coord.GetNeighbor(-2);
            nValue = Map.Get(nHex);
            if (nValue != null)
            {
                Vector3 neighborHeight = Vector3.up * nValue.Height * WorldConfiguration.HEIGHT_MULTPLIER;

                /* Neighbor exists, so generate a bridge */
                Vector3 a = HexLayout.HexCornerOffset(-2) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(coord) + baseHeight;
                Vector3 b = HexLayout.HexCornerOffset(-3) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(coord) + baseHeight;
                Vector3 c = HexLayout.HexCornerOffset(1) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(nHex) + neighborHeight;
                Vector3 d = HexLayout.HexCornerOffset(0) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(nHex) + neighborHeight;
                
                /* ABC and ACD will be the two triangles generated */
                Vertex va = new Vertex(a, color: cell.Color);
                Vertex vb = new Vertex(b, color: cell.Color);
                Vertex vc = new Vertex(c, color: nValue.Color);
                Vertex vd = new Vertex(d, color: nValue.Color);
            
                m.AddTriangle(va, vb, vc);
                m.AddTriangle(va, vc, vd);
            }
        }

        /// <summary>
        /// Generate corner meshes.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="coord"></param>
        /// <param name="cell"></param>
        private void GenerateCorners(Mesher m, Hex coord, Cell cell)
        {
            /* Work on the top right corner first */
            Hex rHex = coord.GetNeighbor(0);
            Cell rValue = Map.Get(rHex);
            Hex trHex = coord.GetNeighbor(-1);
            Cell trValue = Map.Get(trHex);
            if (rValue != null && trValue != null)
            {
                Vector3 a = HexLayout.HexCornerOffset(-1) * (1f - WorldConfiguration.BLEND_REGION_SCALE)
                            + HexLayout.HexToWorld(coord) +
                            cell.Height * Vector3.up * WorldConfiguration.HEIGHT_MULTPLIER;
                
                Vector3 b = HexLayout.HexCornerOffset(3) * (1f - WorldConfiguration.BLEND_REGION_SCALE)
                            + HexLayout.HexToWorld(rHex) +
                            rValue.Height * Vector3.up * WorldConfiguration.HEIGHT_MULTPLIER;
                
                Vector3 c = HexLayout.HexCornerOffset(1) * (1f - WorldConfiguration.BLEND_REGION_SCALE)
                            + HexLayout.HexToWorld(trHex) +
                            trValue.Height * Vector3.up * WorldConfiguration.HEIGHT_MULTPLIER;

                Vertex va = new Vertex(a, color: cell.Color);
                Vertex vb = new Vertex(b, color: rValue.Color);
                Vertex vc = new Vertex(c, color: trValue.Color);

                m.AddTriangle(va, vc, vb);
            }
            
            /* Work on the top corner next */
            Hex tHex = coord.GetNeighbor(-2);
            Cell tValue = Map.Get(tHex);
            if (trValue != null && tValue != null)
            {
                Vector3 a = HexLayout.HexCornerOffset(-2) * (1f - WorldConfiguration.BLEND_REGION_SCALE)
                            + HexLayout.HexToWorld(coord) +
                            cell.Height * Vector3.up * WorldConfiguration.HEIGHT_MULTPLIER;
                
                Vector3 b = HexLayout.HexCornerOffset(2) * (1f - WorldConfiguration.BLEND_REGION_SCALE)
                            + HexLayout.HexToWorld(trHex) +
                            trValue.Height * Vector3.up * WorldConfiguration.HEIGHT_MULTPLIER;
                
                Vector3 c = HexLayout.HexCornerOffset(0) * (1f - WorldConfiguration.BLEND_REGION_SCALE)
                            + HexLayout.HexToWorld(tHex) +
                            tValue.Height * Vector3.up * WorldConfiguration.HEIGHT_MULTPLIER;

                Vertex va = new Vertex(a, color: cell.Color);
                Vertex vb = new Vertex(b, color: trValue.Color);
                Vertex vc = new Vertex(c, color: tValue.Color);

                m.AddTriangle(va, vc, vb);
            }
        }
    }
}