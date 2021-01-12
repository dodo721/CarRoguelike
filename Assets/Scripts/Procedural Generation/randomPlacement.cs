using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randomPlacement : MonoBehaviour
{

    public spawnableItem[] spawnables;
    public float despawnDistance;
    float sqrDespawnDist;
    public int maxSpawned;
    public Transform player;

    public Renderer textureRenderer;
    public MeshFilter mFilter;
    public MeshRenderer mRenderer;


    public LoopingTerrain terrain;
    int chunkSize;
    Dictionary<Vector2, LoopingTerrain.TerrainChunk> terrainDict;
    MapGen mapGen;
    GameObject[] spawnedItems;

    private void Start()
    {
        spawnedItems = new GameObject[maxSpawned];
        for (int i = 0; i < maxSpawned; i++) spawnedItems[i] = null;
        sqrDespawnDist = despawnDistance * despawnDistance;
        
        mapGen = this.GetComponent<MapGen>();
    }

    private void Update()
    {

        chunkSize = terrain.getChunkSize();

        terrainDict = terrain.getCoordsTerrainDict();

        var playerPos = player.position;
        Vector2 chunkCoord = new Vector2(0, 0);
        chunkCoord.x = Mathf.RoundToInt(playerPos.x / chunkSize);
        chunkCoord.y = Mathf.RoundToInt(playerPos.z / chunkSize);

        float scale = mapGen.noiseScale;

        var heightMap = terrainDict[chunkCoord].getHeightMap();
        
        var newNoise = Noise.GenerateNoiseMap(chunkSize + 1, chunkSize + 1, scale, 1, 0.5f, 2f, 0, Vector2.zero);

        var newItem = spawnables[0];

        

        var spawnMask = getPossibleSpawnPoints(heightMap, newNoise, newItem.startRange, newItem.endRange);
        var spawnHeights = applyMask(heightMap, spawnMask);

        var text = TextureGen.TextureFromNoiseMap(spawnHeights);
        
        DrawTexture(text);

        for (int i = 0; i < maxSpawned; i++)
        {
            GameObject item = spawnedItems[i];
            if (item == null) {
                if (terrainDict.ContainsKey(chunkCoord))
                {
                    
                }
            }
            else if (sqrDist(player, item.transform) > sqrDespawnDist)
            {
                // delete gameobject and set item to null
            }

        }
    }
    void DrawTexture(Texture2D texture)
    {
        int w = texture.width;
        int h = texture.height;
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(w, 1, h);
    }

    private float sqrDist(Transform t1, Transform t2)
    {
        return (t1.position - t2.position).sqrMagnitude;
    }

    private bool[,] getPossibleSpawnPoints(float[,] m1, float[,] m2, float min, float max)
    {
        int maxX = m1.GetLength(0);
        int maxY = m1.GetLength(1);
        bool[,] newMap = new bool[maxX, maxY];

        for(int y = 0; y < maxY; y++)
        {
            for(int x = 0; x < maxX; x++)
            {
                float val1 = m1[x, y];
                float val2 = m2[x, y];
                if (val1 >= min && val1 <= max && val2 >= min && val2 <= max) {

                    newMap[x, y] = true;
                } else
                {
                    newMap[x, y] = false;
                }
            }
        }
        return newMap;
    }

    private float[,] applyMask(float[,] map, bool[,] mask)
    {
        int dim = map.GetLength(0);
        float[,] newMap = new float[dim, dim];
        for(int y = 0; y < dim; y++)
        {
            for(int x = 0; x < dim; x++)
            {
                newMap[x, y] = (mask[x, y] ? map[x, y] : 0);
            }
        }
        return newMap;
    }
}

[System.Serializable]
public struct spawnableItem {
    public GameObject item;
    public float startRange;
    public float endRange;
}
