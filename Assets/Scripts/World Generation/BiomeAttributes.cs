using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "KingdomCrafter/Biome Attribute")]
public class BiomeAttributes : ScriptableObject
{
    [Header("Biome Data")]
    public string biomeName;
    public int offset;
    public float scale;

    public int terrainHeight;
    public float terrainScale;

    public byte surfaceBlock;
    public byte subSurfaceBlock;

    public veinSpawns[] oreVeins;

    [Header("Major Foliage")]
    public float majFZoneScale = 1f;
    [Range(0.1f, 1f)]
    public float majFZoneThreshold = 0.99f;
    public float majFPlacementScale = 15f;
    [Range(0.1f, 1f)]
    public float majFPlacementThreshold = 0.8f;

    public int maxHeight = 12;
    public int minHeight = 5;

    public bool placeMajorFoliage;

    [Header("Minor Foliage")]
    public float minFZoneScale = 1f;
    [Range(0.1f, 1f)]
    public float minFZoneThreshold = 0.99f;
    public float minFPlacementScale = 15f;
    [Range(0.1f, 1f)]
    public float minFPlacementThreshold = 0.8f;
}

[System.Serializable]
public class veinSpawns
{
    public string nodeName;
    public byte blockID;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float noiseOffset;
}
