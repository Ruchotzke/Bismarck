﻿using System.Collections.Generic;
using UnityEngine;

namespace bismarck.meshing
{
    /// <summary>
    /// An easy interface used for meshing objects.
    /// </summary>
    public class Mesher
    {
        /// <summary>
        /// How many meshes have been generated.
        /// </summary>
        public static int MeshCount = 0;
        
        private List<Vertex> _vertices;
        private List<int> _indices;
        
        /// <summary>
        /// Construct a new mesher.
        /// </summary>
        public Mesher()
        {
            _vertices = new List<Vertex>();
            _indices = new List<int>();
        }

        /// <summary>
        /// Add a new vertex into this mesh.
        /// </summary>
        /// <param name="v"></param>
        public void AddVertex(Vertex v)
        {
            _vertices.Add(v);
        }

        /// <summary>
        /// Add a triangle into the mesh via indices.
        /// Order ABC.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public void AddTriangle(int a, int b, int c)
        {
            _indices.Add(a);
            _indices.Add(b);
            _indices.Add(c);
        }

        /// <summary>
        /// Attempt to add a new triangle directly. Checks for duplicate vertices and merges if possible.
        /// Order: ABC
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="attemptToReuse"></param>
        public void AddTriangle(Vertex a, Vertex b, Vertex c, bool attemptToReuse = false)
        {
            if (attemptToReuse)
            {
                int aInd = -1, bInd = -1, cInd = -1;

                for(int i = 0; i < _vertices.Count; i++)
                {
                    var item = _vertices[i];
                    if (aInd == -1 && item.Equals(a)) aInd = i;
                    if (bInd == -1 && item.Equals(b)) bInd = i;
                    if (cInd == -1 && item.Equals(c)) cInd = i;

                    if (aInd != -1 && bInd != -1 && cInd != -1) break;
                }

                if (aInd == -1)
                {
                    _vertices.Add(a);
                    aInd = _vertices.Count - 1;
                }

                if (bInd == -1)
                {
                    _vertices.Add(b);
                    bInd = _vertices.Count - 1;
                }

                if (cInd == -1)
                {
                    _vertices.Add(c);
                    cInd = _vertices.Count - 1;
                }
            
                _indices.Add(aInd);
                _indices.Add(bInd);
                _indices.Add(cInd);
            }
            else
            {
                _vertices.Add(a);
                _indices.Add(_vertices.Count - 1);
                _vertices.Add(b);
                _indices.Add(_vertices.Count - 1);
                _vertices.Add(c);
                _indices.Add(_vertices.Count - 1);
            }
            
        }

        /// <summary>
        /// Shortcut to add a new triangle directly without vertex objects.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="ca"></param>
        /// <param name="cb"></param>
        /// <param name="cc"></param>
        public void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Color ca, Color cb, Color cc)
        {
            /* Generate three new vertices */
            Vertex va = new Vertex(a, color: ca);
            Vertex vb = new Vertex(b, color: cb);
            Vertex vc = new Vertex(c, color: cc);
            
            /* Add the tri */
            AddTriangle(va, vb, vc);
        }

        /// <summary>
        /// Generate a set of triangles as a fan for this set of vertices.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="mergeVertices"></param>
        public void AddFan(Vertex[] vertices, bool mergeVertices = false)
        {
            for (int i = 1; i < vertices.Length - 1; i++)
            {
                AddTriangle(vertices[0], vertices[i], vertices[i+1], mergeVertices);
            }
        }

        /// <summary>
        /// Generate a mesh from this mesher's state.
        /// </summary>
        /// <param name="calculateNormals"></param>
        /// <returns></returns>
        public Mesh GenerateMesh(bool calculateNormals)
        {
            /* Assign data */
            List<Vector3> v = new List<Vector3>();
            List<Vector3> n = new List<Vector3>();
            List<Vector2> t = new List<Vector2>();
            List<Color> c = new List<Color>();

            foreach (var vert in _vertices)
            {
                v.Add(vert.Position);
                if (!calculateNormals) n.Add(vert.Normal);
                t.Add(vert.Texture);
                c.Add(vert.Color);
            }
            
            /* Generate the mesh */
            Mesh m = new Mesh()
            {
                name = "MESHER MESH " + (MeshCount++)
            };
            m.SetVertices(v);
            m.SetIndices(_indices, MeshTopology.Triangles, 0);
            m.SetColors(c);

            if (calculateNormals)
            {
                m.RecalculateNormals();
            }
            else
            {
                m.SetNormals(n);
            }
            
            /* Return the mesh */
            return m;
        }
    }
}