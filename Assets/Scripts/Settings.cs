using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Settings
{
    [Header("Mouse Settings")]
    [Range(1f, 20f)]
    public int scrollSpeed;
    [Range(1, 20)]
    public int rotateSpeed;
    [Range(1, 20)]
    public int camSpeed;

    [Header("Performance")]
    public bool enableThreading;
    public bool enableAnimatedChunks;
    public int viewWidthInChunks;

    [Header("World Gen")]
    public int seed;
}
