using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class WaterBehavior : MonoBehaviour
{
    private const int size = 256;

    [SerializeField] private int depth = 5;
    [SerializeField] private float scale = 20f;

    public float offsetX = 100f;
    public float offsetY = 100f;

    private float waterHeight = 0f;

    [SerializeField] Slider waterHeightSlider;

    // Start is called before the first frame update
    void Start()
    {
        waterHeightSlider.value = waterHeight;
    }

    // Update is called once per frame
    void Update()
    {
        if(waterHeight != waterHeightSlider.value)
        {
            waterHeight = waterHeightSlider.value;
            transform.position = new Vector3(transform.position.x, waterHeight, transform.position.z);
        }

        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);

        offsetX += Time.deltaTime / 2f;
        offsetY += Mathf.Sin(Time.time) / 100f;
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = size + 1;
        terrainData.size = new Vector3(size, depth, size);

        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[size, size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float xCoord = (float)x / size * scale + offsetX;
                float yCoord = (float)y / size * scale + offsetY;
                heights[x, y] = (1 - Mathf.Abs(Mathf.Sin(xCoord))) * (Mathf.Cos(yCoord) * Mathf.Cos(yCoord) * 0.5f + 0.5f);
            }
        }

        return heights;
    }
}
