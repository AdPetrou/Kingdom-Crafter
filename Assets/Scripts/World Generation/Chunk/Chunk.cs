using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    private World world;

    public ChunkMeshData chunkMesh;
    public ChunkCoords chunkCoords;
    public Vector3Int position;

    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();

    private int[,,] voxels;

    private bool isVoxelsPopulated = false;
    private bool _isActive;
    public bool isActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            if (chunkMesh.gO != null)
                chunkMesh.gO.SetActive(value);
        }
    }
    public bool isEditable
    {
        get
        {
            if (!isVoxelsPopulated)
                return false;
            else
                return true;
        }
    }

    private int threadNo;

    //Constructor for the Chunk, variables are defined and prepared for use
    public Chunk(ChunkCoords _chunkCoords, World _world, Material mat)
    { 
        world = _world;
        chunkCoords = _chunkCoords;
    }

    public void init(Material mat)
    {
        chunkMesh.chunkGameobject(world, mat, chunkCoords);
        voxels = new int[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];

        position = new Vector3Int(Mathf.FloorToInt(chunkMesh.gO.transform.position.x), Mathf.FloorToInt(chunkMesh.gO.transform.position.y), Mathf.FloorToInt(chunkMesh.gO.transform.position.z));
        threadNo = position.y / VoxelData.chunkHeight;
        //UnityEngine.Debug.Log(threadNo);
        world.chunkUpdateThread[threadNo].chunksToGenerate.Add(this);
    }

    public int getVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x); int yCheck = Mathf.FloorToInt(pos.y); int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(position.x);
        yCheck -= Mathf.FloorToInt(position.y);
        zCheck -= Mathf.FloorToInt(position.z);

        return voxels[xCheck, yCheck, zCheck];
    }

    public void populateVoxelArray()
    {
        int[,] heightmap = generateHeightMap();
        lock (voxels)
        {
            for (int x = 0; x < VoxelData.chunkWidth; x++)
            {
                for (int y = 0; y < VoxelData.chunkHeight; y++)
                {
                    for (int z = 0; z < VoxelData.chunkWidth; z++)
                    {
                        Vector3Int pos = new Vector3Int(x, y, z);
                        voxels[x, y, z] = world.getVoxel(pos + position, heightmap[x,z]);
                    }
                }
            }

            isVoxelsPopulated = true;
            world.chunkUpdateThread[threadNo].chunksToUpdate.Add(this);
        }
    }

    public void buildChunk()
    {
        while(modifications.Count > 0)
        {
            VoxelMod v = modifications.Dequeue();
            Vector3 pos = v.position -= position;
            voxels[(int)pos.x, (int)pos.y, (int)pos.z] = v.id;
        }

        chunkMesh.clearMesh();

        int cubeIndex = 0;
        for (int x = 0; x < VoxelData.chunkWidth; x++)
        {
            for (int y = 0; y < VoxelData.chunkHeight; y++)
            {
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    if (world.blockType[voxels[x, y, z]].isSolid)
                    {
                        Vector3 pos = new Vector3(x, y, z);
                        for (int i = 0; i < 6; i++)
                        {
                            if (!checkVoxelArray(pos + VoxelData.checkVoxel[i]))
                            {
                                addVoxelData(pos, i, cubeIndex);
                                addTexture((world.blockType[voxels[x, y, z]].GetTextureId(i)));
                                cubeIndex += 4;
                            }
                        }
                    }

                }
            }
        }

        buildWater();
        buildGrass();
        world.chunksToDraw.Enqueue(this);
    }

    private void addVoxelData(Vector3 coords, int faceIndex, int cubeIndex)
    {
        chunkMesh.vertices.Add(VoxelData.VertexCoords[VoxelData.VertexOrder[faceIndex, 0]] + coords);
        chunkMesh.vertices.Add(VoxelData.VertexCoords[VoxelData.VertexOrder[faceIndex, 1]] + coords);
        chunkMesh.vertices.Add(VoxelData.VertexCoords[VoxelData.VertexOrder[faceIndex, 2]] + coords);
        chunkMesh.vertices.Add(VoxelData.VertexCoords[VoxelData.VertexOrder[faceIndex, 3]] + coords);

        for (int i = 0; i < 6; i++)
        {
            chunkMesh.tris.Add(VoxelData.tris[i] + cubeIndex);
        }
    }

    private void addTexture(int textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalisedBlockTextureSize;
        y *= VoxelData.NormalisedBlockTextureSize;

        y = 1f - y - VoxelData.NormalisedBlockTextureSize;

        chunkMesh.uvs.Add(new Vector2(x, y));
        chunkMesh.uvs.Add(new Vector2(x, y + VoxelData.NormalisedBlockTextureSize));
        chunkMesh.uvs.Add(new Vector2(x + VoxelData.NormalisedBlockTextureSize, y + VoxelData.NormalisedBlockTextureSize));
        chunkMesh.uvs.Add(new Vector2(x + VoxelData.NormalisedBlockTextureSize, y));
    }

    private void updateSurroundingChunks(Vector3 pos)
    {
        Vector3 thisVoxel = pos;

        for (int i = 0; i < 6; i++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.checkVoxel[i];

            if (!isVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                world.chunkUpdateThread[threadNo].chunksToUpdate.Insert(0, world.getChunkFromVector3(currentVoxel + position));
            }
        }
    }

    public void editVoxel(Vector3 pos, int newID, ObjectGhost objGhost = null)
    {
        int xCheck = Mathf.FloorToInt(pos.x); int yCheck = Mathf.FloorToInt(pos.y); int zCheck = Mathf.FloorToInt(pos.z);

        Vector3 globalPos = new Vector3(xCheck, yCheck, zCheck);

         xCheck -= Mathf.FloorToInt(chunkMesh.gO.transform.position.x);
         yCheck -= Mathf.FloorToInt(chunkMesh.gO.transform.position.y);
         zCheck -= Mathf.FloorToInt(chunkMesh.gO.transform.position.z);

        if (world.blockType[newID].isFurniture)
        {
            BlockType furniture = world.blockType[newID];

            world.placeFurniture(furniture, objGhost, pos, chunkMesh.gO.transform);

            if (furniture.xSize > 1 || furniture.zSize > 1)
            {
                int furnPlaceholder = 1;
                //UnityEngine.Debug.Log(objGhost.CurrentRotation);
                switch (objGhost.CurrentRotation)
                {                  
                    case 0:
                        pos = new Vector3(globalPos.x + 1, globalPos.y, globalPos.z + 1);
                        generateFakeBlocks(furniture.fill, furniture.xSize, furniture.zSize, pos, furnPlaceholder);
                        return;

                    case 1:
                        pos = new Vector3(globalPos.x + 1, globalPos.y, globalPos.z - 1);
                        generateFakeBlocks(furniture.fill, furniture.zSize, -furniture.xSize, pos, furnPlaceholder);
                        return;

                    case 2:
                        pos = new Vector3(globalPos.x - 1, globalPos.y, globalPos.z - 1);
                        generateFakeBlocks(furniture.fill, -furniture.xSize, -furniture.zSize, pos, furnPlaceholder);
                        return;

                    case 3:
                        pos = new Vector3(globalPos.x - 1, globalPos.y, globalPos.z + 1);
                        generateFakeBlocks(furniture.fill, -furniture.zSize, furniture.xSize, pos, furnPlaceholder);
                        return;
                }
            }
        }

        else
            voxels[xCheck, yCheck, zCheck] = newID;

        lock (world.chunkUpdateThread[threadNo].chunksToUpdate)
        {
            world.chunkUpdateThread[threadNo].chunksToUpdate.Insert(0, this);
            //Update Surrounding Chunks
            updateSurroundingChunks(new Vector3(xCheck, yCheck, zCheck));
        }
    }

    public bool isVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.chunkWidth - 1 || y < 0 || y > VoxelData.chunkHeight - 1 || z < 0 || z > VoxelData.chunkWidth - 1)
            return false;
        else
            return true;
    }

    public bool checkVoxelArray(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x); int y = Mathf.FloorToInt(pos.y); int z = Mathf.FloorToInt(pos.z);

        if (!isVoxelInChunk(x, y, z))
            return world.checkForVoxel(pos + position);
        else
            return world.blockType[voxels[x, y, z]].isSolid;
    }

    public void generateFakeBlocks(bool fill, int xSize, int zSize, Vector3 pos, int voxel)
    {
        //pos = new Vector3(pos.x + 1, pos.y, pos.z + 1);        

        if (!fill)
        {
            for(int i = xSize; i != 0;)
            {
                //editVoxel(new Vector3(pos.x - i, pos.y, pos.z), 1);
                world.getChunkFromVector3(new Vector3(pos.x - i, pos.y, pos.z)).editVoxel(new Vector3(pos.x - i, pos.y, pos.z), voxel);
                //UnityEngine.Debug.Log(new Vector3(pos.x - i, pos.y, pos.z));

                if (i < 0)
                    i++;
                else
                    i--;
            }

            for (int i = zSize; i != 0;)
            {
                world.getChunkFromVector3(new Vector3(pos.x, pos.y, pos.z - i)).editVoxel(new Vector3(pos.x, pos.y, pos.z - i), voxel);
                //UnityEngine.Debug.Log(new Vector3(pos.x, pos.y, pos.z - i));

                if (i < 0)
                    i++;
                else
                    i--;
            }
        }

        else
        {
            for (int i = xSize; i != 0;)
            {
                for (int n = zSize; n != 0;)
                {
                    world.getChunkFromVector3(new Vector3(pos.x - i, pos.y, pos.z - n)).editVoxel(new Vector3(pos.x - i, pos.y, pos.z - n), voxel);
                    //UnityEngine.Debug.Log(new Vector3(pos.x - i, pos.y, pos.z - n));

                    if (n < 0)
                        n++;
                    else
                        n--;
                }

                if (i < 0)
                    i++;
                else
                    i--;
            }
        }
    }

    public void buildWater()
    {
        chunkMesh.clearWater();
        int faceIndex = 0; 
        int xfaceOffset = 2, zfaceOffset = 0;
        for (int x = 0; x < VoxelData.chunkWidth; x++)
        {
            if (xfaceOffset == 0)
                xfaceOffset = 2;
            else
                xfaceOffset = 0;

            for (int y = 0; y < VoxelData.chunkHeight; y++)
            {
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    int voxel = voxels[x, y, z];
                    if(voxel == 13)
                    {
                        //UnityEngine.Debug.Log("Water Found");
                        if(y == VoxelData.waterHeight)
                        { 
                            Vector3 worldPos = new Vector3(x, y, z);
                            for (int i = 0; i < VoxelData.waterVertices.Length; i++)
                            {
                                chunkMesh.waterChunk.vertices.Add(VoxelData.waterVertices[i] + worldPos);
                                chunkMesh.waterChunk.uvs.Add(VoxelData.waterUVs[xfaceOffset + zfaceOffset, i]);
                            }

                            for (int i = 0; i < VoxelData.tris.Length; i++)
                                chunkMesh.waterChunk.tris.Add(VoxelData.tris[i] + faceIndex);

                            faceIndex += 4;
                            zfaceOffset++;

                            if (zfaceOffset > 1)
                                zfaceOffset = 0;
                        }
                    }
                }
            }
        }
    }

    public void buildGrass()
    {
        chunkMesh.clearGrass();
        int faceIndex = 0;
        for (int x = 0; x < VoxelData.chunkWidth; x++)
        {
            for (int y = 0; y < VoxelData.chunkHeight; y++)
            {
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    int voxel = voxels[x, y, z];
                    Vector3 worldPos = new Vector3(x, y, z);
                    if (voxel == 14 && world.checkForVoxel(worldPos - VoxelData.y))
                    {
                        for (int i = 0; i < VoxelData.grassVertices.GetLength(0); i++)
                        {
                            for (int u = 0; u < VoxelData.grassVertices.GetLength(1); u++)
                            {
                                chunkMesh.grassChunk.vertices.Add(VoxelData.grassVertices[i, VoxelData.grassVertices.GetLength(1) - 1 - u] + worldPos);
                                chunkMesh.grassChunk.uvs.Add(VoxelData.grassUVs[u]);
                            }

                            for (int u = 0; u < VoxelData.tris.Length; u++)
                                chunkMesh.grassChunk.tris.Add(VoxelData.tris[u] + faceIndex);

                            faceIndex += 4;
                        }
                    }
                }
            }
        }
    }

    public int[,] generateHeightMap()
    {
        int[,] heightMap = new int[VoxelData.chunkWidth,VoxelData.chunkWidth];

        //BASE
        for(int x = 0; x < VoxelData.chunkWidth; x++)
            for(int z = 0; z < VoxelData.chunkWidth; z++)
            {
                BiomeAttributes biome = world.biomes[world.getBiome(new Vector2Int(x + position.x, z + position.z))];
                float currentTerrainHeight = world.getTerrainHeight(new Vector2Int(x + position.x, z + position.z), biome);
                heightMap[x,z] = (int)currentTerrainHeight;
            }



        return heightMap;
    }

}
