using System.Collections.Generic;
using bismarck.hex;
using bismarck.meshing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using utilities.noise;

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
            
            /* Create a noise generator for map generation */
            Fractal noise = new Fractal(5, 0.75f);
            
            /* Add some initial data */
            foreach (var hex in Map.GetAllHexes())
            {
                /* Get the world-space coordinate of this hex */
                Vector3 pos = HexLayout.HexToWorld(hex.coord);
                pos.y = 0;  //mantain a 2D sample

                int height = 0;
                Color c = Color.Lerp(Color.black, Color.white, 0.1f * noise.Sample(pos));
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
            HandleCorner(m, coord, cell, 0);

            /* Work on the top corner next */
            HandleCorner(m, coord, cell, -1);
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
                    
                    /* Keep the original offsets to use for lerping */
                    Vector3 origA = HexLayout.HexCornerOffset(0 + direction) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(coord) + baseHeight;
                    Vector3 origB = HexLayout.HexCornerOffset(5 + direction) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(coord) + baseHeight;
                    Vector3 origC = HexLayout.HexCornerOffset(3 + direction) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(nHex) + neighborHeight;
                    Vector3 origD = HexLayout.HexCornerOffset(2 + direction) * (1f - WorldConfiguration.BLEND_REGION_SCALE) + HexLayout.HexToWorld(nHex) + neighborHeight;
                                        
                    /* Lerp along this direction, adding a terrace at each step */
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
                }
            }
        }

        /// <summary>
        /// Handle meshing a corner of a hex connecting three adjacent hexes.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="coord"></param>
        /// <param name="cell"></param>
        /// <param name="direction"></param>
        private void HandleCorner(Mesher m, Hex coord, Cell cell, int direction)
        {
            /* Determine the neighbors and a height-based ordering */
            Hex botHex = coord;
            Hex rightHex = coord.GetNeighbor(direction);
            Hex leftHex = coord.GetNeighbor(direction - 1);
            Cell bCell = cell;
            Cell rCell = Map.Get(rightHex);
            Cell lCell = Map.Get(leftHex);
            
            if (lCell == null || rCell == null) return;
            
            Vector3 bvec = HexLayout.HexCornerOffset(-1 + direction) * (1f - WorldConfiguration.BLEND_REGION_SCALE) 
                           + HexLayout.HexToWorld(botHex) 
                           + bCell.Height * Vector3.up * WorldConfiguration.HEIGHT_MULTPLIER;
            Vector3 lvec = HexLayout.HexCornerOffset(1 + direction) * (1f - WorldConfiguration.BLEND_REGION_SCALE) 
                           + HexLayout.HexToWorld(leftHex) 
                           + lCell.Height * Vector3.up * WorldConfiguration.HEIGHT_MULTPLIER;
            Vector3 rvec = HexLayout.HexCornerOffset(3 + direction) * (1f - WorldConfiguration.BLEND_REGION_SCALE) 
                           + HexLayout.HexToWorld(rightHex) 
                           + rCell.Height * Vector3.up * WorldConfiguration.HEIGHT_MULTPLIER;

            
            
            /* Find the lowest cell, and reorder left/right accordingly */
            if (lCell.Height < cell.Height && lCell.Height < rCell.Height)
            {
                /* lcell is the lowest */
                (botHex, bCell, bvec, leftHex, lCell, lvec, rightHex, rCell, rvec) = 
                    (leftHex, lCell, lvec, rightHex, rCell, rvec, botHex, bCell, bvec);
            }
            else if (rCell.Height < cell.Height && rCell.Height < lCell.Height)
            {
                /* rcell is the lowest */
                (botHex, bCell, bvec, leftHex, lCell, lvec, rightHex, rCell, rvec) = 
                    (rightHex, rCell, rvec, botHex, bCell, bvec, leftHex, lCell, lvec);
            }
            
            /* Determine the type of the two bottom-adjacent edges */
            HexEdgeType leftEdgeType = GetEdgeType(bCell, lCell);
            HexEdgeType rightEdgeType = GetEdgeType(bCell, rCell);

            /* Check for a slope-slope-flat condition */
            if (leftEdgeType == HexEdgeType.SLOPE)
            {
                if (rightEdgeType == HexEdgeType.SLOPE)
                {
                    /* SSF */
                    TriangulateCornerTerraces(m, bvec, bCell, lvec, lCell, rvec, rCell);
                }
                else if (rightEdgeType == HexEdgeType.FLAT)
                {
                    /* SFS */
                    TriangulateCornerTerraces(m, lvec, lCell, rvec, rCell, bvec, bCell);
                }
                else
                {
                    /* SC? */
                    TriangulateCornerTerracesCliff(m, bvec, bCell, lvec, lCell, rvec, rCell);
                }
            }
            else if (rightEdgeType == HexEdgeType.SLOPE)
            {
                if (leftEdgeType == HexEdgeType.FLAT)
                {
                    /* FSS */
                    TriangulateCornerTerraces(m, rvec, rCell, bvec, bCell, lvec, lCell);
                }
                else
                {
                    /* CS? */
                    TriangulateCornerCliffTerraces(m, bvec, bCell, lvec, lCell, rvec, rCell);
                }
            }
            else if (GetEdgeType(lCell, rCell) == HexEdgeType.SLOPE)
            {
                if (lCell.Height < rCell.Height)
                {
                    /* CCSL */
                    TriangulateCornerCliffTerraces(m, rvec, rCell, bvec, bCell, lvec, lCell);
                }
                else
                {
                    /* CCSR */
                    TriangulateCornerTerracesCliff(m, lvec, lCell, rvec, rCell, bvec, bCell);
                }
            }
            else
            {
                /* No terracing - just triangulate */
                m.AddTriangle(bvec, lvec, rvec, bCell.Color, lCell.Color, rCell.Color);
            }
        }

        /// <summary>
        /// Triangulate a dual-terraced corner.
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="beginCell"></param>
        /// <param name="left"></param>
        /// <param name="leftCell"></param>
        /// <param name="right"></param>
        /// <param name="rightCell"></param>
        private void TriangulateCornerTerraces(Mesher m, Vector3 begin, Cell beginCell, Vector3 left, Cell leftCell,
            Vector3 right, Cell rightCell)
        {
            /* Work waay begin to top */
            var leftSide = TerraceLerp(begin, left, beginCell.Color, leftCell.Color);
            var rightSide = TerraceLerp(begin, right, beginCell.Color, rightCell.Color);

            /* First is a triangle */
            Vertex bot = new Vertex(begin, color: beginCell.Color);
            Vertex l = new Vertex(leftSide[1].point, color: leftSide[1].color);
            Vertex r = new Vertex(rightSide[1].point, color: rightSide[1].color);
            m.AddTriangle(bot, l, r);
            
            /* Now use quads to finish the rest of the terraces */
            for (int i = 2; i < leftSide.Count; i++)
            {
                Vertex nl = new Vertex(leftSide[i].point, color: leftSide[i].color);
                Vertex nr = new Vertex(rightSide[i].point, color: rightSide[i].color);

                m.AddTriangle(l, nl, nr);
                m.AddTriangle(l, nr, r);

                l = nl;
                r = nr;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <param name="begin"></param>
        /// <param name="beginCell"></param>
        /// <param name="left"></param>
        /// <param name="leftCell"></param>
        /// <param name="right"></param>
        /// <param name="rightCell"></param>
        private void TriangulateCornerTerracesCliff(Mesher m, Vector3 begin, Cell beginCell, Vector3 left,
            Cell leftCell, Vector3 right, Cell rightCell)
        {
            /* Split into two operations: bottom and top */
            
            /* Bottom - merge vertices at height of slope */
            float b = 1f / (rightCell.Height - beginCell.Height);
            if (b < 0) b = -b;
            Vector3 boundary = Vector3.Lerp(begin, right, b);
            Color bColor = Color.Lerp(beginCell.Color, rightCell.Color, b);
            TriangulateBoundaryTriangle(m, begin, beginCell, left, leftCell, boundary, bColor);
            
            /* Top, if sloped terrace, otherwise just use a triangle */
            if (GetEdgeType(leftCell, rightCell) == HexEdgeType.SLOPE)
            {
                TriangulateBoundaryTriangle(m, left, leftCell, right, rightCell, boundary, bColor);
            }
            else
            {
                m.AddTriangle(left, right, boundary, leftCell.Color, rightCell.Color, bColor);
            }
        }
        
        private void TriangulateCornerCliffTerraces(Mesher m, Vector3 begin, Cell beginCell, Vector3 left,
            Cell leftCell, Vector3 right, Cell rightCell)
        {
            /* Split into two operations: bottom and top */
            
            /* Bottom - merge vertices at height of slope */
            float b = 1f / (leftCell.Height - beginCell.Height);
            if (b < 0) b = -b;
            Vector3 boundary = Vector3.Lerp(begin, left, b);
            Color bColor = Color.Lerp(beginCell.Color, leftCell.Color, b);
            TriangulateBoundaryTriangle(m, right, rightCell, begin, beginCell, boundary, bColor);
            
            /* Top, if sloped terrace, otherwise just use a triangle */
            if (GetEdgeType(leftCell, rightCell) == HexEdgeType.SLOPE)
            {
                TriangulateBoundaryTriangle(m, left, leftCell, right, rightCell, boundary, bColor);
            }
            else
            {
                m.AddTriangle(left, right, boundary, leftCell.Color, rightCell.Color, bColor);
            }
        }

        /// <summary>
        /// Triangulate a corner triangle with terracing.
        /// </summary>
        private void TriangulateBoundaryTriangle(Mesher m, Vector3 begin, Cell beginCell, Vector3 left,
            Cell leftCell, Vector3 boundary, Color bColor)
        {
            /* Grab all terrace points from the left and connect them to the point */
            var sideTerrace = TerraceLerp(begin, left, beginCell.Color, leftCell.Color);
            for (int i = 0; i < sideTerrace.Count - 1; i++)
            {
                m.AddTriangle(sideTerrace[i].point, sideTerrace[i + 1].point, boundary, sideTerrace[i].color,
                    sideTerrace[i + 1].color, bColor);
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

        private enum HexEdgeType
        {
            FLAT, SLOPE, CLIFF
        }

        /// <summary>
        /// Get the edge type between two cells.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private HexEdgeType GetEdgeType(Cell a, Cell b)
        {
            var diff = Mathf.Abs(a.Height - b.Height);

            if (diff == 0) return HexEdgeType.FLAT;
            if (diff == 1) return HexEdgeType.SLOPE;
            return HexEdgeType.CLIFF;
        }
    }
}