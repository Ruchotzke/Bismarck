using System.Collections.Generic;
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
            
            /* Seed the RNG */
            Random.InitState(5);
            
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
            HandleBridge(m, coord, cell, 0);
            HandleBridge(m, coord, cell, -1);
            HandleBridge(m, coord, cell, -2);
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

        /// <summary>
        /// Handle the bridging between hexes for a given direction.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="coord"></param>
        /// <param name="cell"></param>
        /// <param name="direction"></param>
        private void HandleBridge(Mesher m, Hex coord, Cell cell, int direction)
        {
            Hex nHex = coord.GetNeighbor(direction);
            Cell nValue = Map.Get(nHex);
            if (nValue != null)
            {
                Vector3 baseHeight = Vector3.up * cell.Height * WorldConfiguration.HEIGHT_MULTPLIER;
                Vector3 neighborHeight = Vector3.up * nValue.Height * WorldConfiguration.HEIGHT_MULTPLIER;

                /* If height difference is more than one, just straight mesh it */
                if (Mathf.Abs(cell.Height - nValue.Height) != 1)
                {
                 /* Neighbor exists, so generate a bridge */
                 Vector3 a = HexLayout.HexCornerOffset(0 + direction) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(coord) + baseHeight;
                 Vector3 b = HexLayout.HexCornerOffset(5 + direction) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(coord) + baseHeight;
                 Vector3 c = HexLayout.HexCornerOffset(3 + direction) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(nHex) + neighborHeight;
                 Vector3 d = HexLayout.HexCornerOffset(2 + direction) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(nHex) + neighborHeight;
                 
                 /* ABC and ACD will be the two triangles generated */
                 Vertex va = new Vertex(a, color: cell.Color);
                 Vertex vb = new Vertex(b, color: cell.Color);
                 Vertex vc = new Vertex(c, color: nValue.Color);
                 Vertex vd = new Vertex(d, color: nValue.Color);
             
                 m.AddTriangle(va, vb, vc);
                 m.AddTriangle(va, vc, vd);
                }
                else
                {
                    /* This needs to be terraced */
                    
                    /* Identify a direction vector from cell to n */
                    Vector3 terraceDir = HexLayout.HexToWorld(nHex) - HexLayout.HexToWorld(coord);
                    terraceDir.y = neighborHeight.y - baseHeight.y;
                    
                    /* Keep the original offsets to use for lerping */
                    Vector3 origA = HexLayout.HexCornerOffset(0 + direction) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(coord) + baseHeight;
                    Vector3 origB = HexLayout.HexCornerOffset(5 + direction) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(coord) + baseHeight;
                    Vector3 origC = HexLayout.HexCornerOffset(3 + direction) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(nHex) + neighborHeight;
                    Vector3 origD = HexLayout.HexCornerOffset(2 + direction) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(nHex) + neighborHeight;
                                        
                    /* Lerp along this direction, adding a terrace at each step */
                    float step = 1f / (WorldConfiguration.NUM_TERRACES + 1);
                    var aToD = TerraceLerp(origA, origD, cell.Color, nValue.Color);
                    var bToC = TerraceLerp(origB, origC, cell.Color, nValue.Color);
                    
                    /* Generate the mesh */
                    for (int i = 0; i < aToD.Count - 1; i++)
                    {
                        Vertex va = new Vertex(aToD[i].point, color: aToD[i].color);
                        Vertex vb = new Vertex(bToC[i].point, color: bToC[i].color);
                        Vertex vc = new Vertex(bToC[i+1].point, color: bToC[i+1].color);
                        Vertex vd = new Vertex(aToD[i+1].point, color: aToD[i+1].color);

                        m.AddTriangle(va, vb, vc);
                        m.AddTriangle(va, vc, vd);
                    }

                    // for (int i = 1; i <= WorldConfiguration.NUM_TERRACES + 2; i++)
                    // {
                    //    /* Compute the next terrace coordinates */
                    //    Vector3 c = Vector3.Lerp(origB, origC, step * i);
                    //    Vector3 cLower = new Vector3(c.x, b.y, c.z);
                    //    Vector3 d = Vector3.Lerp(origA, origD, step * i);
                    //    Vector3 dLower = new Vector3(d.x, a.y, d.z);
                    //  
                    //    /* Figure out the current color */
                    //    Color curr = Color.Lerp(cell.Color, nValue.Color, step * i);
                    //    
                    //    /* Triangulate this terrace */
                    //    Vertex va = new Vertex(a, color: curr);
                    //    Vertex vb = new Vertex(b, color: curr);
                    //    Vertex vc = new Vertex(c, color: curr);
                    //    Vertex vd = new Vertex(d, color: curr);
                    //    Vertex vcl = new Vertex(cLower, color: curr);
                    //    Vertex vdl = new Vertex(dLower, color: curr);
                    //    m.AddTriangle(va, vb, vcl);
                    //    m.AddTriangle(va, vcl, vdl);
                    //    m.AddTriangle(vcl, vc, vdl);
                    //    m.AddTriangle(vdl, vc, vd);
                    //
                    //    /* Update the previous coordinates */
                    //    a = d;
                    //    b = c;
                    // }
                }
            }
        }

        /// <summary>
        /// Generate all points and colors along a terrace from A to B.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static List<(Vector3 point, Color color)> TerraceLerp(Vector3 a, Vector3 b, Color ca, Color cb)
        {
            List<(Vector3 point, Color color)> points = new List<(Vector3 point, Color color)>();

            float horizDist = Vector3.Distance(new Vector3(a.x, 0, a.z),
                new Vector3(b.x, 0, b.z));
            float horizStep = horizDist / ((float)WorldConfiguration.NUM_TERRACES + 1);
            float vertStep = (b.y - a.y) / WorldConfiguration.NUM_TERRACES;
            Vector3 dir = (b - a);
            dir.y = 0;
            dir = dir.normalized;

            points.Add((a, ca));
            for (int i = 0; i < WorldConfiguration.NUM_TERRACES; i++)
            {
                /* Track forward */
                Vector3 forward = points[2 * i].point + dir * horizStep;
                
                /* Track up */
                Vector3 up = forward + Vector3.up * vertStep;
                
                /* Compute color */
                Color col = Color.Lerp(ca, cb, horizStep * (i + 1) / horizDist);
                
                /* Add the new points */
                points.Add((forward, col));
                points.Add((up, col));
            }

            points.Add((b, cb));
            
            return points;
        }
    }
}