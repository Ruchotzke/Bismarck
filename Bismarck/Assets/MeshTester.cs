using System;
using System.Collections;
using System.Collections.Generic;
using bismarck.hex;
using bismarck.meshing;
using TMPro;
using UnityEngine;

public class MeshTester : MonoBehaviour
{
    private MeshFilter _filter;
    private MeshRenderer _renderer;

    public Material UseMaterial;

    public TextMeshPro pf_Label;
    
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
        Layout l = new Layout(Orientation.layoutPointTop, new Vector3(1, 0, 1), Vector3.zero);
        
        /* Generate a mesher */
        Mesher m = new Mesher();

        /* Render some hexagons */
        Hex center = new Hex(0, 0);
        MeshHex(center, l, m);
        for(int i = -2; i < 3; i++)
        {
            if (i != 0)
            {
                MeshHex(new Hex(i, 0), l, m);
                MeshHex(new Hex(0, i), l, m);
            }
        }

        _filter.mesh = m.GenerateMesh(true);
    }

    /// <summary>
    /// Generate the triangles for a given hexagon.
    /// </summary>
    /// <param name="coord"></param>
    /// <param name="layout"></param>
    /// <param name="m"></param>
    private void MeshHex(Hex coord, Layout layout, Mesher m)
    {
        /* Get the corners for the center hex */
        Vector3[] corners = layout.GenerateCorners(coord);
        Vertex[] c = new Vertex[6];
        c[0] = new Vertex(corners[0]);  // flip the order to maintain CW ordering b/c unity
        c[1] = new Vertex(corners[5]);
        c[2] = new Vertex(corners[4]);
        c[3] = new Vertex(corners[3]);
        c[4] = new Vertex(corners[2]);
        c[5] = new Vertex(corners[1]);
        
        /* Triangulate as a fan */
        m.AddFan(c);
        
        /* Add a label */
        var label = Instantiate(pf_Label);
        label.transform.position = layout.HexToWorld(coord) + new Vector3(0f, .2f, 0f);
        label.text = "<" + coord.q + "," + coord.r + "," + coord.s + ">";
    }
}
