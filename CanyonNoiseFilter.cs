using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanyonNoiseFilter : INoiseFilter
{
    Noise noise = new Noise();
    NoiseSettings.CanyonNoiseSettings settings;
    public CanyonNoiseFilter(NoiseSettings.CanyonNoiseSettings settings)
    {
        this.settings = settings;
    }
    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float clampNoiseValue = 0;
        float frequency = settings.baseRoughness;
        float amplitude = 1;
        float weight = 1;

        for (int i = 0; i < settings.numLayers; i++)
        {
            float v = noise.Evaluate(point * frequency + settings.centre);
            noiseValue += v*amplitude;
            frequency *= settings.roughness;
            amplitude *= settings.persistence;
        }
        noiseValue = Mathf.Max(0, noiseValue - settings.minValue);

        if (noiseValue > settings.maxHeight)
        {
            noiseValue = settings.maxHeight;
        }
        return noiseValue* settings.strength; ;

    }
    public void Randomize(int seed)
    {
        noise.Randomize(seed);
    }
}
