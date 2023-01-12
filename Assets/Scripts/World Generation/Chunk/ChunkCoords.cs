using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChunkCoords
{
    public int x, y, z;

    public ChunkCoords(int _x = 0, int _y = 0, int _z = 0)
    {
        x = _x; y = _y; z = _z;
    }

    public ChunkCoords(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x); int yCheck = Mathf.FloorToInt(pos.y); int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.chunkWidth; y = yCheck / VoxelData.chunkHeight; z = zCheck / VoxelData.chunkWidth;
    }

    public string testChunkCoords() { return (x + " " + y + " " + z); }

    public bool Equals(ChunkCoords other)
    {
        if (other.x == x && other.y == y && other.z == z)
            return true;
        else
            return false;
    }

    public Vector3Int convertToLocal(Vector3Int globalPosition)
    {
        return new Vector3Int(globalPosition.x - (x * VoxelData.chunkWidth), globalPosition.y - (y * VoxelData.chunkHeight), globalPosition.z - (z * VoxelData.chunkWidth));
    }
}
