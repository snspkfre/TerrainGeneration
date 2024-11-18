using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    private Terrain terrain;

    [SerializeField] private int width = 256;
    [SerializeField] private int length = 256;

    public float freq = 16f;
    public float radius = 0.5f;
    public float amplitude = 1.2f;

    private float seed;

    public float grassHeight = 0f;
    public float rockBegin = 0.25f;
    public float rockEnd = 0.35f;
    public float snowHeight = 0.4f;

    [SerializeField] UnityEngine.UI.Slider freqSlider;
    [SerializeField] UnityEngine.UI.Slider ampSlider;
    [SerializeField] UnityEngine.UI.Slider radiusSlider;

    [SerializeField] UnityEngine.UI.Slider grassSlider;
    [SerializeField] UnityEngine.UI.Slider rockBeginSlider;
    [SerializeField] UnityEngine.UI.Slider rockEndSlider;
    [SerializeField] UnityEngine.UI.Slider snowSlider;

    // Start is called before the first frame update
    void Start()
    {
        terrain = GetComponent<Terrain>();
        seed = Random.Range(0f, 1000f);
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
        PaintTerrain();
        
        freqSlider.value = freq;
        ampSlider.value = amplitude;
        radiusSlider.value = radius;

        grassSlider.value = grassHeight;
        rockBeginSlider.value = rockBegin;
        rockEndSlider.value = rockEnd;
        snowSlider.value = snowHeight;
    }

    // Update is called once per frame
    void Update()
    {
        if (freqSlider.value != freq)
        {
            freq = freqSlider.value;
            terrain.terrainData = GenerateTerrain(terrain.terrainData);
            PaintTerrain();
        }

        if (ampSlider.value != amplitude)
        {
            amplitude = ampSlider.value;
            terrain.terrainData = GenerateTerrain(terrain.terrainData);
            PaintTerrain();
        }

        if (radiusSlider.value != radius)
        {
            radius = radiusSlider.value;
            terrain.terrainData = GenerateTerrain(terrain.terrainData);
            PaintTerrain();
        }

        if (grassHeight != grassSlider.value)
        {
            grassHeight = Mathf.Min(grassSlider.value, rockBegin);
            grassSlider.value = grassHeight;
            PaintTerrain();
        }
        if (rockBegin != rockBeginSlider.value)
        {
            rockBegin = Mathf.Min(Mathf.Max(rockBeginSlider.value, grassHeight), rockEnd);
            rockBeginSlider.value = rockBegin;
            PaintTerrain();
        }
        
        if (rockEnd != rockEndSlider.value)
        {
            rockEnd = Mathf.Min(Mathf.Max(rockEndSlider.value, rockBegin), snowHeight);
            rockEndSlider.value = rockEnd;
            PaintTerrain();
        }

        if (snowHeight != snowSlider.value)
        {
            snowHeight = Mathf.Max(snowSlider.value, rockEnd);
            snowSlider.value = snowHeight;
            PaintTerrain();
        }
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.size = new Vector3(width, 50, length);
        terrainData.SetHeights(0, 0, GetHeightArray());
        return terrainData;
    }

    float[,] GetHeightArray()
    {
        TerrainData data = terrain.terrainData;
        int size = data.heightmapResolution;
        float[,] heights = new float[size, size];

        float center = size / 2;

        float maxDistance = Mathf.Min(center, center) * radius;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float xCoord = (i + seed) / size * freq;
                float yCoord = (j + seed) / size * freq;

                float noise = NoiseGenerator(xCoord, yCoord, 10);

                float distance = Mathf.Sqrt(Mathf.Pow(i - center, 2) + Mathf.Pow(j - center, 2));

                float falloff = Mathf.Clamp01((maxDistance - distance) / maxDistance);

                heights[i, j] = Mathf.Pow(noise, amplitude) * falloff;
            }
        }

        return heights;
    }

    float NoiseGenerator(float x, float y, int numPoints)
    {
        List<Vector3> points = new List<Vector3>();
        for(int i = 0; i < numPoints; i++)
        {
        }
        return Mathf.PerlinNoise(x, y);
    }

    void PaintTerrain()
    {
        TerrainData data = terrain.terrainData;
        int width = data.alphamapWidth;
        int height = data.alphamapHeight;

        float[,] heights = data.GetHeights(0, 0, width, height);
        float[,,] splatmapData = new float[width, height, data.alphamapLayers];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float terrainHeight = heights[x, y];

                if (terrainHeight <= grassHeight)
                {
                    splatmapData[x, y, 0] = 1;
                    splatmapData[x, y, 1] = 0;
                    splatmapData[x, y, 2] = 0;
                }
                else if (terrainHeight <= rockBegin)
                {
                    float blend = Mathf.InverseLerp(grassHeight, rockBegin, terrainHeight);
                    splatmapData[x, y, 0] = 1 - blend;
                    splatmapData[x, y, 1] = blend;
                    splatmapData[x, y, 2] = 0;
                }
                else if (height <= rockEnd)
                {
                    splatmapData[x, y, 0] = 0;
                    splatmapData[x, y, 1] = 1;
                    splatmapData[x, y, 2] = 0;
                }
                else if (terrainHeight >= snowHeight)
                {
                    splatmapData[x, y, 0] = 0;
                    splatmapData[x, y, 1] = 0;
                    splatmapData[x, y, 2] = 1;
                }
                else
                {
                    float blend = Mathf.InverseLerp(rockEnd, snowHeight, terrainHeight);
                    splatmapData[x, y, 0] = 0;
                    splatmapData[x, y, 1] = 1 - blend;
                    splatmapData[x, y, 2] = blend;
                }
            }
        }

        data.SetAlphamaps(0, 0, splatmapData);
    }

    public void NewSeed()
    {
        seed = Random.Range(0f, 1000f);
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
        PaintTerrain();
    }
}
