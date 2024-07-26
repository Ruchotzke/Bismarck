using bismarck.hex;
using bismarck.meshing;
using bismarck.world;
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
        
        /* Generate a map */
        Map<Cell> map = new Map<Cell>(-5, 5, -5, 5);

        /* Render some hexagons and generate their cells */
        foreach (var hex in map.GetAllHexes())
        {
            Color c = Random.value > 0.5f ? Color.blue : Color.green;
            map.Set(hex.coord, new Cell(c, Random.Range(0, 3)));
            MeshHex(hex.coord, map.Get(hex.coord), l, m);
        }

        _filter.mesh = m.GenerateMesh(true);
    }

    /// <summary>
    /// Generate the triangles for a given hexagon.
    /// </summary>
    /// <param name="coord"></param>
    /// <param name="layout"></param>
    /// <param name="m"></param>
    private void MeshHex(Hex coord, Cell cell, Layout layout, Mesher m)
    {
        /* Get the corners for the center hex */
        Vector3[] corners = layout.GenerateCorners(coord);
        Color color = cell.Color;
        Vertex[] c = new Vertex[6];
        c[0] = new Vertex(corners[0], color:color);  // flip the order to maintain CW ordering b/c unity
        c[1] = new Vertex(corners[5], color:color);
        c[2] = new Vertex(corners[4], color:color);
        c[3] = new Vertex(corners[3], color:color);
        c[4] = new Vertex(corners[2], color:color);
        c[5] = new Vertex(corners[1], color:color);
        
        /* Triangulate as a fan */
        m.AddFan(c, false);
        
        /* Add a label */
        var label = Instantiate(pf_Label);
        label.transform.position = layout.HexToWorld(coord) + new Vector3(0f, .05f, 0f);
        label.text = "<" + coord.q + "," + coord.r + "," + coord.s + ">";
    }
}
