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
        int bound = 6;
        World world = new World(-bound, bound, -bound, bound);
        world.pf_Label = pf_Label;

        Mesher m = new Mesher();

        world.GenerateMesh(m);

        _filter.mesh = m.GenerateMesh(true);
    }
    
}
