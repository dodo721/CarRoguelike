using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopingTerrain : MonoBehaviour
{
    const float movementForUpdateThresh = 25f;
    const float sqrThresh = movementForUpdateThresh * movementForUpdateThresh;
    Vector2 oldPos;

    public static float maxViewDist;
    public Transform player;
    public static Vector2 playerPos;
    public Material mapMaterial;
    public LODInfo[] detailLevels;
    static MapGen mapGen;
    int chunkSize;
    int chunksVisible;
    Dictionary<Vector2, TerrainChunk> terrainDict = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> prevChunks = new List<TerrainChunk>();

    void Start()
    {
        mapGen = FindObjectOfType<MapGen>();
        chunkSize = MapGen.mapChunkSize - 1;
        
        Debug.Log("Visible : " + maxViewDist + " / " + chunkSize);
        maxViewDist = detailLevels[detailLevels.Length - 1].distFromPlayer;

        chunksVisible = Mathf.RoundToInt(maxViewDist / chunkSize);

        UpdateVisibleChunks();
    }

    void Update()
    {
        playerPos = new Vector2(player.position.x, player.position.z);
        if((oldPos - playerPos).sqrMagnitude > sqrThresh) {
            UpdateVisibleChunks();
            playerPos = oldPos;
        }
            
    }

    void UpdateVisibleChunks()
    {
        foreach (TerrainChunk chunk in prevChunks) chunk.setVisible(false);
        prevChunks.Clear();

        int curChunkX = Mathf.RoundToInt(playerPos.x / chunkSize);
        int curChunkY = Mathf.RoundToInt(playerPos.y / chunkSize);

        Debug.Log("Visible: " + chunksVisible);

        for(int y = -chunksVisible; y <= chunksVisible; y++)
        {
            for(int x = -chunksVisible; x <= chunksVisible; x++)
            {
                Vector2 chunkCoord = new Vector2(curChunkX + x, curChunkY + y);
                Debug.Log(chunkCoord.ToString());
                
                if (terrainDict.ContainsKey(chunkCoord))
                {
                    var foundChunk = terrainDict[chunkCoord];
                    foundChunk.UpdateTerainChunk();
                } 
                else
                {
                    terrainDict.Add(chunkCoord, new TerrainChunk(chunkCoord, chunkSize, transform, mapMaterial, detailLevels));
                }
            }
        }
    }

    public class TerrainChunk
    {
        Vector2 pos;
        GameObject mesh;
        Bounds bounds;

        
        MeshRenderer mRend;
        MeshFilter mFilter;
        MeshCollider mCollider;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        
        MapData mapData;
        bool recievedMapdata;
        int prevLODIndex = -1;

        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material, LODInfo[] detailLevels)
        {
            pos = coord * size;
            Vector3 posV3 = new Vector3(pos.x, 0, pos.y);

            this.detailLevels = detailLevels;

            bounds = new Bounds(pos, Vector2.one * size);

            mesh = new GameObject("Chunk");
            mRend = mesh.AddComponent<MeshRenderer>();
            mRend.material = material;
            mFilter = mesh.AddComponent<MeshFilter>();
            mCollider = mesh.AddComponent<MeshCollider>();
            
            mesh.transform.position = posV3;
            mesh.transform.parent = parent;
            this.setVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for(int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerainChunk);
            }


            mapGen.RequestMapData(pos, OnMapDataRecieved);
        }

        void OnMapDataRecieved(MapData mapData)
        {
            this.mapData = mapData;
            recievedMapdata = true;
            int c = mapGen.getSize();
            Texture2D t = TextureGen.TextureFromColourMap(mapData.colMap, c, c);
            mRend.material.mainTexture = t;
            UpdateTerainChunk();
        }

        public void UpdateTerainChunk()
        {
            if (recievedMapdata)
            {
                float dstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(playerPos));
                bool visible = dstFromNearestEdge <= maxViewDist;
                Debug.Log(visible);
                Debug.Log("1. " + dstFromNearestEdge + ",       2. " + maxViewDist);
                if (visible)
                {
                    int LODIndex = 0;
                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (dstFromNearestEdge > detailLevels[i].distFromPlayer)
                        {
                            LODIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    Debug.Log("!! " + LODIndex);
                    if (LODIndex != prevLODIndex)
                    {
                        LODMesh lodMesh = lodMeshes[LODIndex];
                        if (lodMesh.hasMesh)
                        {
                            mFilter.mesh = lodMesh.mesh;
                            prevLODIndex = LODIndex;
                            mCollider.sharedMesh = lodMesh.mesh;
                        }
                        else if (lodMesh.hasReq == false)
                        {
                            lodMesh.requestMesh(mapData);
                        }
                    }
                    prevChunks.Add(this);
                }
                setVisible(visible);
            }
        }

        public void setVisible(bool visible)
        {
            mesh.SetActive(visible);
        }

        public bool isVisibile()
        {
            return mesh.activeSelf;
        }
    }

    class LODMesh
    {

        public Mesh mesh;
        public bool hasReq;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void onMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.createMesh();
            hasMesh = true;

            updateCallback();
        }

        public void requestMesh(MapData mapData)
        {
            hasReq = true;
            mapGen.RequestMeshData(mapData, onMeshDataReceived, lod);
        }

    }

    [System.Serializable]
    public struct LODInfo {
        public int lod;
        public float distFromPlayer;
    }


}

