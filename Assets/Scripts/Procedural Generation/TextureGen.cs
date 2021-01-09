using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGen 
{
    public static Texture2D TextureFromColourMap(Color[] colMap, int w, int h)
    {
        Texture2D nTexture = new Texture2D(w, h);
        nTexture.filterMode = FilterMode.Point;
        nTexture.wrapMode = TextureWrapMode.Clamp;
        nTexture.SetPixels(colMap);
        nTexture.Apply();
        return nTexture;
    }

    public static Texture2D TextureFromNoiseMap(float[,] noiseMap)
    {
        int w = noiseMap.GetLength(0);
        int h = noiseMap.GetLength(1);
        Texture2D nTexture = new Texture2D(w, h);
        Color[] colMap = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                colMap[y * w + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        return TextureFromColourMap(colMap, w, h);
    }
}
