using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{

    public enum NormaliseMode { Local, Global };

    public static float[,] GenerateNoiseMap (int width, int height, float scale, int octaves, float persistance, float lacunarity, int seed, Vector2 offset, NormaliseMode normMode)
    {
        if (scale <= 0) scale = 0.001f;

        
        System.Random prng = new System.Random(seed);

        float amplitude = 1;
        float freq = 1;
        float noiseHeight = 0;

        float maxPossHeight = 0f;
        float minPossHeight = 0f;
        
        // Sample each octave from a new part (increases noise)
        Vector2[] octaveOffsets = new Vector2[octaves];
        for(int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
            maxPossHeight += amplitude;
            amplitude *= persistance;
        }

        // Used to normalise the noisemap at end
        float maxNoiseVal = float.MinValue;
        float minNoiseVal = float.MaxValue;

        // Used to offset the noise to the centre instead of top-left
        float hWidth = width / 2f;
        float hHeight = height / 2f;


        float[,] noiseMap = new float[width, height];
        for(int y = 0; y < height; y++) {
            for(int x = 0; x < width; x++) {

                amplitude = 1;
                freq = 1;
                noiseHeight = 0;

                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - hWidth + octaveOffsets[i].x)  / scale * freq;
                    float sampleY = (y - hHeight + octaveOffsets[i].y) / scale * freq;
                    
                    float perlinVal  = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinVal * amplitude;

                    amplitude *= persistance;
                    freq *= lacunarity;
                }

                if (noiseHeight > maxNoiseVal) maxNoiseVal = noiseHeight;
                else if (noiseHeight < minNoiseVal) minNoiseVal = noiseHeight;

                noiseMap[x, height - 1 - y] = noiseHeight;
            }
        }

        // Normalise noisemap between min and max values to be between 0f - 1f
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                if(normMode == NormaliseMode.Local)
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseVal, maxNoiseVal, noiseMap[x, y]);
                if(normMode == NormaliseMode.Global)
                {
                    noiseMap[x, y] = ((noiseMap[x, y] + 1) / (2f * maxPossHeight / 1.75f)) ;

                }

            }
        }

        return noiseMap;
    }
}
