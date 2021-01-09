using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGen
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightCoefficient, float heightExponent, AnimationCurve heightCurve, int LOD)
    {
     
        int w = heightMap.GetLength(0);
        int h = heightMap.GetLength(1);
        float topLeftX = (w - 1) / -2f;
        float topLeftZ = (h - 1) / 2f;

        // Setup in case we want to simplify mesh (less detail the further away from skipping occasional vertice)
        int meshSimplificationInc = (LOD == 0 ? 1 : LOD * 2);
        int vertPerLine = (w - 1) / meshSimplificationInc + 1;

        MeshData terMesh = new MeshData(vertPerLine, vertPerLine);

        AnimationCurve pCurve = new AnimationCurve(heightCurve.keys);

        int vertexIndex = 0;     // Point in mesh that we're adding info into at each step
        for (int y = 0; y < h; y += meshSimplificationInc)
        {
            for(int x = 0; x < w; x += meshSimplificationInc)
            {
                float noiseVal = heightMap[x, y];
                terMesh.vertices[vertexIndex] = new Vector3(topLeftX  + x, getMeshHeightFromNoise(noiseVal, heightCoefficient, heightExponent, heightCurve), topLeftZ - y);
                terMesh.uvs[vertexIndex] = new Vector2(x / (float)w, y / (float)h);

                if(x < w - 1 && y < h - 1)
                {
                    terMesh.addTriangle(vertexIndex, vertexIndex + vertPerLine + 1, vertexIndex + vertPerLine);
                    terMesh.addTriangle(vertexIndex + vertPerLine + 1, vertexIndex, vertexIndex + 1);
                }
                
                vertexIndex++;
            }
        }
        return terMesh;
    }
    
    private static float getMeshHeightFromNoise(float height, float coefficient, float exponent, AnimationCurve curve)
    {
        return curve.Evaluate(Mathf.Round(height * exponent) / exponent) * coefficient;
       
    }
}


public class MeshData {
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;
    
    public MeshData(int width, int height)
    {
        vertices = new Vector3[width * height];
        uvs = new Vector2[vertices.Length];
        triangles = new int[(width - 1) * (height - 1) * 6];
        //triangleIndex = 0;
    }

    public void addTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex+2] = c;
        triangleIndex += 3;
    }
    
    Vector3[] calcNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triCount = triangles.Length / 3;
        for(int i = 0; i < triCount; i++)   // For every triangle in mesh
        {
            int normTriangleIndex = i * 3;
            int vertexIndexA = triangles[normTriangleIndex];
            int vertexIndexB = triangles[normTriangleIndex + 1];
            int vertexIndexC = triangles[normTriangleIndex + 2];
            Vector3 triNorm = normVectorFromTriangle(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triNorm;
            vertexNormals[vertexIndexB] += triNorm;
            vertexNormals[vertexIndexC] += triNorm;
        }

        foreach (Vector3 norm in vertexNormals) norm.Normalize();

        return vertexNormals;
    } 

    Vector3 normVectorFromTriangle(int a, int b, int c)
    {
        Vector3 pA = vertices[a];
        Vector3 pB = vertices[b];
        Vector3 pC = vertices[c];
        Vector3 ab = pB - pA;
        Vector3 ac = pC - pA;
        return new Vector3(0, 1, 0);
        return Vector3.Cross(ab, ac).normalized;
    }

    public Mesh createMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = this.vertices;
        mesh.triangles = this.triangles;
        mesh.uv = uvs;
        mesh.normals = calcNormals();
        
        return mesh;
    }
}
