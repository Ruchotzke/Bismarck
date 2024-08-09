using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bismarck.meshing
{
    /// <summary>
    /// A manager for meshing a single chunk.
    /// </summary>
    public class Chunk : MonoBehaviour
    {
        private MeshFilter _filter;
        private MeshRenderer _renderer;
        
        private void Awake()
        {
            /* Generate a filter and renderer for this gameobject */
            _filter = gameObject.AddComponent<MeshFilter>();
            _renderer = gameObject.AddComponent<MeshRenderer>();
        }

        /// <summary>
        /// Initialize this chunk to use a specific mesh and material.
        /// </summary>
        /// <param name="useMaterial"></param>
        /// <param name="useMesh"></param>
        public void Initialize(Material useMaterial, Mesh useMesh)
        {
            _filter.mesh = useMesh;
            _renderer.material = useMaterial;
        }
    }
}

