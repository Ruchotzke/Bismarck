using System;
using System.Collections;
using System.Collections.Generic;
using bismarck.hex;
using bismarck.meshing;
using UnityEngine;

public class MeshTester : MonoBehaviour
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
        /* Generate a hex grid */
        Layout l = new Layout(Orientation.LayoutFlatTop, new Vector3(1, 0, 1), Vector3.zero);
        
        /* Generate a mesher */
        Mesher m = new Mesher();

        /* Get the corners for the center hex */
        Vector3[] corners = l.GenerateCorners(new Hex(0, 0));
        Vertex[] c = new Vertex[6];
        for (int i = 0; i < corners.Length; i++)
        {
            c[i] = new Vertex(corners[i]);
        }
        
        /* Triangulate as a fan */
        m.AddFan(c);

        _filter.mesh = m.GenerateMesh(true);
    }
}
