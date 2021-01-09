using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGen : MonoBehaviour
{
    public static int mapChunkSize = 241;

    [Min(1)]
    public int
        octaves;

    [Range(0, 6)]
    public int editorLOD;

    [Min(0f)]
    public float
        noiseScale,
        heightCoefficient,
        heightExponent,
        wacky;

    [Range(0f, 2f)]
    public float
        persistance,
        lacunarity;

    [Range(-0.5f, 0.5f)]
    public float heightOffset;

    public int seed;
    public Vector2 offset;

    public MapDisplay display;

    public bool autoUpdate;

    public TerrainTypes[] regions;
    public AnimationCurve meshHeightCurve;

    public Noise.NormaliseMode normaliseMode;

    public enum DrawMode { NoiseMap, ColourMap, Mesh};
    public DrawMode drawMode;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
    public void DrawMapInEditor()
    {
        MapData map = GenerateMapData(Vector2.zero);
        if (drawMode == DrawMode.NoiseMap) display.DrawTexture(TextureGen.TextureFromNoiseMap(map.heightMap));
        else if (drawMode == DrawMode.ColourMap) display.DrawTexture(TextureGen.TextureFromColourMap(map.colMap, mapChunkSize, mapChunkSize));
        else if (drawMode == DrawMode.Mesh) display.DrawMesh(MeshGen.GenerateTerrainMesh(map.heightMap, heightCoefficient, heightExponent, meshHeightCurve, editorLOD), TextureGen.TextureFromColourMap(map.colMap, mapChunkSize, mapChunkSize));
    }

    // Generate random map, return it's noise-map and colour-map
    public MapData GenerateMapData(Vector2 centre)
    {
        float[,] nMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseScale, octaves, persistance, lacunarity, seed, centre + offset, normaliseMode);

        nMap = applyHeightOffset(nMap, mapChunkSize, mapChunkSize);

        Color[] colMap = new Color[mapChunkSize * mapChunkSize];
        for(int y = 0; y < mapChunkSize; y++)
        {
            for(int x = 0; x < mapChunkSize; x++)
            {
                float curHeight = nMap[x, y];
                TerrainTypes curTerrain = heightToTerrain(curHeight);
                colMap[y * mapChunkSize + x] = curTerrain.col;
            }
        }
        return new MapData(nMap, colMap);
    }

    // Add in an height offset to each point in heightMap
    private float[,] applyHeightOffset(float[,] map, int w, int h)
    {
        for(int y = 0; y < h; y++)
        {
            for(int x = 0; x < w; x++)
            {
                map[x, y] = Mathf.Min(map[x, y] + heightOffset, 1f);
            }
        }
        return map;
    }

    public int getSize()
    {
        return mapChunkSize;
    }

    // Takes a height value (0f - 1f) and finds the TerrainType associated with it (regions set in inspector currently)
    private TerrainTypes heightToTerrain(float height)
    {
        TerrainTypes cur = regions[0];
        foreach(TerrainTypes ter in regions)
        {
            if (height <= ter.height) return ter;
        }
        return regions[regions.Length-1];
    }

    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(centre, callback);
        };

        new Thread(threadStart).Start();
    }

    public void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(centre);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, Action<MeshData> callback, int LOD)
    {
        ThreadStart tStart = delegate
        {
            MeshDataThread(mapData, callback, LOD);
        };
        new Thread(tStart).Start();
    }

    public void MeshDataThread(MapData mapData, Action<MeshData> callback, int LOD)
    {
        MeshData meshData = MeshGen.GenerateTerrainMesh(mapData.heightMap, heightCoefficient, heightExponent, meshHeightCurve, LOD);
        lock (meshData)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update()
    {
        if(mapDataThreadInfoQueue.Count > 0)
        {
            for(int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    struct MapThreadInfo<T>{
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }

}

// Store information for coloured maps
[System.Serializable]
public struct TerrainTypes {
    public string name;
    public float height;
    public Color col;
}

// Store information about final colour map
public struct MapData {
    public readonly float[,] heightMap;
    public readonly Color[] colMap;

    public MapData(float[,] heightMap, Color[] colMap)
    {
        this.heightMap = heightMap;
        this.colMap = colMap;
    }
}

 