using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChunkMeshData
{
    public GameObject gO;
    public Mesh mesh;
    public List<Vector3> vertices;
    public List<int> tris;
    public List<Vector2> uvs;

    public WaterChunk waterChunk;
    public GrassChunk grassChunk;

    public void chunkGameobject(World _world, Material mat, ChunkCoords chunkCoords)
    {
        gO = new GameObject((chunkCoords.x * VoxelData.chunkWidth).ToString() + " " + (chunkCoords.y * VoxelData.chunkHeight).ToString() + " " + (chunkCoords.z * VoxelData.chunkWidth).ToString());
        gO.transform.parent = _world.transform;
        gO.transform.position = new Vector3(chunkCoords.x * VoxelData.chunkWidth, chunkCoords.y * VoxelData.chunkHeight, chunkCoords.z * VoxelData.chunkWidth);

        gO.AddComponent<MeshFilter>();
        gO.AddComponent<MeshCollider>();
        gO.AddComponent<MeshRenderer>().material = mat;

        mesh = new Mesh();
        vertices = new List<Vector3>();
        tris = new List<int>();
        uvs = new List<Vector2>();

        initWaterChunk(_world);
        initGrassChunk(_world);
    }

    public void initGrassChunk(World _world)
    {
        grassChunk.grassMesh = new Mesh();
        grassChunk.vertices = new List<Vector3>();
        grassChunk.tris = new List<int>();
        grassChunk.uvs = new List<Vector2>();

        grassChunk.grassGO = new GameObject("grassChunk", typeof(MeshFilter), typeof(MeshRenderer));
        grassChunk.grassGO.layer = 2;
        grassChunk.grassGO.transform.SetParent(gO.transform);
        grassChunk.grassGO.transform.localPosition = new Vector3(0f, 0f, 0f);
        MeshRenderer meshR = grassChunk.grassGO.GetComponent<MeshRenderer>();
        meshR.material = _world.grassMat;
        meshR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshR.receiveShadows = false;
    }

    public void initWaterChunk(World _world)
    {
        waterChunk.waterMesh = new Mesh();
        waterChunk.vertices = new List<Vector3>();
        waterChunk.tris = new List<int>();
        waterChunk.uvs = new List<Vector2>();

        waterChunk.waterGO = new GameObject("waterChunk", typeof(MeshFilter), typeof(MeshRenderer));
        waterChunk.waterGO.layer = 4;
        waterChunk.waterGO.transform.SetParent(gO.transform);
        waterChunk.waterGO.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        MeshRenderer meshR = waterChunk.waterGO.GetComponent<MeshRenderer>();
        meshR.material = _world.waterMat;
        meshR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshR.receiveShadows = false;
    }

    public void clearMesh()
    {
        vertices.Clear();
        tris.Clear();
        uvs.Clear();
        clearWater();
        clearGrass();
    }

    public void clearWater()
    {
        waterChunk.vertices.Clear();
        waterChunk.uvs.Clear();
        waterChunk.tris.Clear();
    }

    public void clearGrass()
    {
        grassChunk.vertices.Clear();
        grassChunk.uvs.Clear();
        grassChunk.tris.Clear();
    }

    public void updateChunk()
    {
        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        gO.GetComponent<MeshFilter>().mesh = mesh;
        gO.GetComponent<MeshCollider>().sharedMesh = mesh;

        updateWater();
        updateGrass();
    }


    public void updateWater()
    {
        waterChunk.waterMesh.Clear();
        waterChunk.waterMesh.vertices = waterChunk.vertices.ToArray();
        waterChunk.waterMesh.triangles = waterChunk.tris.ToArray();
        waterChunk.waterMesh.uv = waterChunk.uvs.ToArray();

        waterChunk.waterMesh.RecalculateNormals();

        waterChunk.waterGO.GetComponent<MeshFilter>().mesh = waterChunk.waterMesh;
    }

    public void updateGrass()
    {
        grassChunk.grassMesh.Clear();
        grassChunk.grassMesh.vertices = grassChunk.vertices.ToArray();
        grassChunk.grassMesh.triangles = grassChunk.tris.ToArray();
        grassChunk.grassMesh.uv = grassChunk.uvs.ToArray();

        grassChunk.grassMesh.RecalculateNormals();

        grassChunk.grassGO.GetComponent<MeshFilter>().mesh = grassChunk.grassMesh;
    }
}

public struct WaterChunk
{
    public List<Vector3> vertices;
    public List<int> tris;
    public List<Vector2> uvs;
    public Mesh waterMesh;
    public GameObject waterGO;
}

public struct GrassChunk
{
    public List<Vector3> vertices;
    public List<int> tris;
    public List<Vector2> uvs;
    public Mesh grassMesh;
    public GameObject grassGO;
}
