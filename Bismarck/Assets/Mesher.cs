using System;
using System.Collections;
using System.Collections.Generic;
using bismarck.meshing;
using UnityEngine;

public class Mesher : MonoBehaviour
{
    private MeshFilter _filter;
    private MeshRenderer _renderer;

    public Material UseMaterial;
    
    private void Awake()
    {
        /* Generate a filter and renderer for this gameobject */
        _filter = gameObject.AddComponent<MeshFilter>();
        _renderer = gameObject.AddComponent<MeshRenderer>();
        _renderer.material = UseMaterial;
    }

    // Start is called before the first frame update
    void Start()
    {
        bismarck.meshing.Mesher m = new bismarck.meshing.Mesher();

        m.AddTriangle(new Vertex(new Vector3(0.0f, 0.0f, 0.0f)), new Vertex(new Vector3(0.0f, 0.0f, 1.0f)),
            new Vertex(new Vector3(1.0f, 0.0f, 0.0f)));

        _filter.mesh = m.GenerateMesh(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
