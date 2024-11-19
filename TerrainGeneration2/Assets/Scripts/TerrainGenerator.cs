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

    List<Vector3> noisePoints = new List<Vector3>();

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
        NoiseGenerationSetup(Mathf.FloorToInt(freq));
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
                float xCoord = (i + seed) / size;
                float yCoord = (j + seed) / size;

                float noise = NoiseGenerator(xCoord, yCoord);

                float distance = Mathf.Sqrt(Mathf.Pow(i - center, 2) + Mathf.Pow(j - center, 2));

                float falloff = Mathf.Clamp01((maxDistance - distance) / maxDistance);

                heights[i, j] = Mathf.Pow(noise, amplitude) * falloff;
            }
        }

        return heights;
    }

    void NoiseGenerationSetup(int numPoints)
    {
        noisePoints.Clear();
        for (int i = 0; i < numPoints; i++)
        {
            noisePoints.Add(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
        }
    }
    
    float NoiseGenerator(float x, float y)
    {
        Vector2 position = new Vector2(x % 1f, y % 1f); // Ensure position is within 0-1 bounds
        float closestHeight = 0f;
        int min = 0;
        List<float> distances = new List<float>();

        for (int i = 0; i < noisePoints.Count; i++)
        {
            distances.Add(Vector2.Distance(position, noisePoints[i]));
            if (distances[i] < distances[min]) min = i;
        }

        return noisePoints[min].z;
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
        NoiseGenerationSetup(Mathf.FloorToInt(freq));
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
        PaintTerrain();
    }
}
