using System;
using System.Collections;
using System.Collections.Generic;
using bismarck.meshing;
using UnityEngine;
using utilities.noise;

public class Heightmap : MonoBehaviour
{
    private MeshFilter _filter;
    private MeshRenderer _renderer;

    public Material UseMaterial;

    public float Scale = 0.15f;

    public float Amplitude = 4.0f;

    public int Octaves = 4;

    public float Persistence = 0.5f;

    public float Exponent = 4.0f;
    private void Awake()
    {
        /* Generate a filter and renderer for this gameobject */
        _filter = gameObject.AddComponent<MeshFilter>();
        _renderer = gameObject.AddComponent<MeshRenderer>();
        _renderer.material = UseMaterial;
    }

    private void Update()
    {
        /* Noise generator */
        Fractal noise = new Fractal(1, 1, 1);
        
        /* Generate a map */
        float[,] map = new float[100, 100];
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                map[x, y] = noise.Sample(Scale * new Vector3(x, 0, y));
                map[x, y] = Mathf.Pow(map[x, y], Exponent);
            }
        }
        
        /* Generate the heightmap */
        Mesher m = new Mesher();
        for (int x = 0; x < map.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < map.GetLength(1) - 1; y++)
            {
                Vertex a = new Vertex(Scale * new Vector3(x, Amplitude * map[x, y], y), color: Color.Lerp(Color.green, Color.red, map[x, y]));
                Vertex b = new Vertex(Scale * new Vector3(x, Amplitude * map[x, y+1], y+1), color: Color.Lerp(Color.green, Color.red, map[x, y+1]));
                Vertex c = new Vertex(Scale * new Vector3(x+1, Amplitude * map[x+1, y+1], y+1), color: Color.Lerp(Color.green, Color.red, map[x+1, y+1]));
                Vertex d = new Vertex(Scale * new Vector3(x+1, Amplitude * map[x+1, y], y), color: Color.Lerp(Color.green, Color.red, map[x+1, y]));
                
                
                m.AddTriangle(a, b, c);
                m.AddTriangle(a, c, d);
            }
        }

        _filter.mesh = m.GenerateMesh(true);
    }
}
