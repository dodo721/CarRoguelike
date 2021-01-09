using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter mFilter;
    public MeshRenderer mRenderer;

    public void DrawTexture(Texture2D texture)
    {
        int w = texture.width;
        int h = texture.height;
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(w, 1, h);
    }

    public void DrawMesh(MeshData mesh, Texture2D texture)
    {
        mFilter.sharedMesh = mesh.createMesh();
        mRenderer.sharedMaterial.mainTexture = texture; 
    }

}
