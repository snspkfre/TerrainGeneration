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
        GameObject[,,] objs = new GameObject[size, size, size];

        RenderTexture voxelData = new RenderTexture(size, size, 0, RenderTextureFormat.ARGB32);
        voxelData.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        voxelData.volumeDepth = size;
        voxelData.enableRandomWrite = true;
        voxelData.Create();
        ComputeShader cs = Resources.Load<ComputeShader>("NoiseCreator");
        int kernelHandle = cs.FindKernel("CSMain");

        cs.SetTexture(kernelHandle, "result", voxelData);
        cs.Dispatch(kernelHandle, size / 8, size / 8, size / 8);
        
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
                    if (strength[x,y,z] > 0.5) objs[x, y, z] = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
                }
            }
        }
    }

    public static float Perlin3D(float x, float y, float z)
    {
        x *= 0.1f;
        y *= 0.1f;
        z *= 0.1f;

        float xy = Mathf.PerlinNoise(x, y);
        float xz = Mathf.PerlinNoise(x, z);
        float yx = Mathf.PerlinNoise(y, x);
        float yz = Mathf.PerlinNoise(y, z);
        float zx = Mathf.PerlinNoise(z, x);
        float zy = Mathf.PerlinNoise(z, y);

        return (xy + xz + yx + yz + zx + zy) / 6;
    }
}
