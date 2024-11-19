using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
    [SerializeField] int size = 128;
    [SerializeField] ComputeShader noiseShader;
    [SerializeField] ComputeShader terrainShader;

    [HideInInspector] public RenderTexture strengthTexture;

    ComputeBuffer vertexBuffer;
    ComputeBuffer indexBuffer;
    ComputeBuffer vertexCountBuffer;
    ComputeBuffer indexCountBuffer;

    const int MaxVertices = 10000;
    const int MaxIndices = 10000;

    Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        Create3DTexture(ref strengthTexture, size, "StrengthTexture");

        vertexBuffer = new ComputeBuffer(MaxVertices, sizeof(float) * 3);
        indexBuffer = new ComputeBuffer(MaxIndices, sizeof(uint));
        vertexCountBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Raw);
        indexCountBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Raw);

        OnGenerate();
    }

    public void OnGenerate()
    {
        noiseShader.SetTexture(0, "StrengthTexture", strengthTexture);
        int threadGroups = Mathf.CeilToInt(size / 8.0f);
        noiseShader.Dispatch(0, threadGroups, threadGroups, threadGroups);

        GenerateMesh();
    }

    void GenerateMesh()
    {
        // Reset counters
        uint[] zeroArray = { 0 };
        vertexCountBuffer.SetData(zeroArray);
        indexCountBuffer.SetData(zeroArray);

        // Set compute shader parameters
        terrainShader.SetTexture(0, "StrengthTexture", strengthTexture);
        terrainShader.SetBuffer(0, "vertexBuffer", vertexBuffer);
        terrainShader.SetBuffer(0, "indexBuffer", indexBuffer);
        terrainShader.SetBuffer(0, "vertexCount", vertexCountBuffer);
        terrainShader.SetBuffer(0, "indexCount", indexCountBuffer);

        int threadGroups = Mathf.CeilToInt(size / 8.0f);
        terrainShader.Dispatch(0, threadGroups, threadGroups, threadGroups);

        // Read the counters to determine how many vertices and indices were written
        uint[] vertexCountArray = new uint[1];
        uint[] indexCountArray = new uint[1];
        vertexCountBuffer.GetData(vertexCountArray);
        indexCountBuffer.GetData(indexCountArray);
        int vertexCount = (int)vertexCountArray[0];
        int indexCount = (int)indexCountArray[0];

        if (vertexCount > 0 && indexCount > 0)
        {
            Vector3[] vertices = new Vector3[vertexCount];
            int[] indices = new int[indexCount];

            vertexBuffer.GetData(vertices, 0, 0, vertexCount);
            indexBuffer.GetData(indices, 0, 0, indexCount);

            if (mesh == null)
            {
                mesh = new Mesh();
            }
            mesh.Clear();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.RecalculateNormals();

            GetComponent<MeshFilter>().mesh = mesh;
        }
    }

    void Create3DTexture(ref RenderTexture texture, int size, string name)
    {
        var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat;
        if (texture == null || !texture.IsCreated() || texture.width != size || texture.height != size || texture.volumeDepth != size || texture.graphicsFormat != format)
        {
            if (texture != null)
            {
                texture.Release();
            }

            const int numBitsInDepthBuffer = 0;
            texture = new RenderTexture(size, size, numBitsInDepthBuffer);
            texture.graphicsFormat = format;
            texture.volumeDepth = size;
            texture.enableRandomWrite = true;
            texture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;

            texture.Create();
        }
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;
        texture.name = name;
    }

    void OnDestroy()
    {
        vertexBuffer.Release();
        indexBuffer.Release();
        vertexCountBuffer.Release();
        indexCountBuffer.Release();
    }
}
