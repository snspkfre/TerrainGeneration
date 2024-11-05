using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{

    [SerializeField] int size = 128;
    [SerializeField] GameObject prefab;


    // Start is called before the first frame update
    void Start()
    {
        float[,,] strength = new float[size, size, size];
        for(int z = 0; z < size; z++)
        {
            for(int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    strength[x, y, z] = Perlin3D(x, y, z);
                }
            }
        }

        for (int z = 0; z < size; z++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    GameObject sphere = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
                    sphere.transform.localScale = Vector3.one / 2;
                    sphere.GetComponent<MeshRenderer>().materials[0].SetColor("_Color", new UnityEngine.Color(strength[x, y, z], strength[x, y, z], strength[x, y, z], 1));
                }
            }
        }
    }

    public static float Perlin3D(float x, float y, float z)
    {
        return (Mathf.PerlinNoise1D(x * 0.1f) + Mathf.PerlinNoise1D(y * 0.1f) + Mathf.PerlinNoise1D(z * 0.1f)) / 3;
    }
}
