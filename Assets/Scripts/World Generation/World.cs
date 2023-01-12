using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;
using System.IO;

public class World : MonoBehaviour
{
    public Settings settings;

    public int seed;
    public bool inUI;

    public Transform player;
    public Material mat, waterMat, grassMat;

    public Chunk[,,] chunks { get; } = new Chunk[VoxelData.worldWidth + 1, VoxelData.worldHeight + VoxelData.worldHeight, VoxelData.worldWidth + 1];

    public BlockType[] blockType;
    public BiomeAttributes[] biomes;
    public GameObject loadingScreen;

    public List<ChunkCoords> chunksToCreate { get; } = new List<ChunkCoords>();
    public List<ChunkCoords> activeChunks { get; set; } = new List<ChunkCoords>();
    public List<ChunkCoords> previouslyActiveChunks { get; set; } = new List<ChunkCoords>();

    public ChunkCoords playerLastChunkCoords { get; set; } = new ChunkCoords(0, 0, 0);
    public ChunkCoords playerChunkCoords { get; set; } = new ChunkCoords(0, 0, 0);

    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    public WorldThread[] chunkUpdateThread{ get; } = new WorldThread[VoxelData.worldHeight];

    private bool isFirstLoad = true;
    private bool viewDistanceCheck = false;
    private bool drawing = false;
    //public bool[] threadUpdate { get; set; } = new bool[VoxelData.worldHeight];

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(VoxelData.worldWidth);

        //string jsonExport = JsonUtility.ToJson(settings);
        //Debug.Log(jsonExport);

        //File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);

        string jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
        settings = JsonUtility.FromJson<Settings>(jsonImport);

        seed = settings.seed;

        Camera.main.GetComponent<CameraController>().enableCamera = false;
        loadingScreen.SetActive(true);
        Random.InitState(seed);

        for (int i = 0; i < VoxelData.worldHeight; i++)
        {
            //threadUpdate[i] = false;
            chunkUpdateThread[i] = new WorldThread(this, i);
            chunkUpdateThread[i].threadedUpdate();
        }

        generateWorld();
        playerChunkCoords = getChunkCoordsFromVector3(new Vector3(player.position.x, player.position.y, player.position.z));
        playerLastChunkCoords = playerChunkCoords;
        loadScreen();
    }

    void Update()
    {
        playerChunkCoords = getChunkCoordsFromVector3(new Vector3(player.position.x, player.position.y, player.position.z));

        for (int i = 0; i < VoxelData.worldHeight; i++)
            chunkUpdateThread[i].threadedUpdate();

        if (chunksToCreate.Count > 0)
            for (int i = 0; i < 5; i++)
            {
                if (chunksToCreate.Count > 0)
                    createChunk();
                else
                    break;
            }

        if (chunksToDraw.Count > 0 && !drawing)
        {
            drawing = true;
            for (int i = 0; i < 5; i++)
            {
                if (chunksToDraw.Count > 0)
                {
                    if (chunksToDraw.Peek().isEditable)
                    {
                        chunksToDraw.Peek().chunkMesh.updateChunk();
                        chunksToDraw.Dequeue();
                    }
                }
                else
                    break;
            }
            drawing = false;
        }


        if (chunksToDraw.Count == 0 && isFirstLoad && chunksToCreate.Count == 0 && !viewDistanceCheck)
        {
            viewDistanceCheck = true;
            //loadScreen();
            Task viewDistance = new Task(() => checkViewDistance(playerChunkCoords));
            viewDistance.Start();
            //checkViewDistance(playerChunkCoords);
        }

        if (!playerChunkCoords.Equals(playerLastChunkCoords) && !viewDistanceCheck)
        {
            viewDistanceCheck = true;
            Task viewDistance = new Task(() => checkViewDistance(playerChunkCoords));
            viewDistance.Start();
            //checkViewDistance(playerChunkCoords);
        }
    }

    private void generateWorld()
    {
        for (int x = (VoxelData.worldWidth / 2) - settings.viewWidthInChunks; x < (VoxelData.worldWidth / 2) + settings.viewWidthInChunks; x++)
        {
            //int y = (Data.worldHeight / 2 + Data.viewHeightInChunks - 1); y > Data.worldHeight / 2 - Data.viewHeightInChunks; y--
            for (int y = 0; y < VoxelData.worldHeight; y++)
            {
                for (int z = (VoxelData.worldWidth / 2) - settings.viewWidthInChunks; z < (VoxelData.worldWidth / 2) + settings.viewWidthInChunks; z++)
                {
                    ChunkCoords currentChunkCoords = new ChunkCoords(x, y, z);
                    chunks[x, y, z] = new Chunk(currentChunkCoords, this, mat);
                    chunksToCreate.Add(currentChunkCoords);
                }
            }
        }
    }

    public void checkViewDistance(ChunkCoords coords)
    {
        lock (activeChunks)
        {
            playerLastChunkCoords = coords;

            previouslyActiveChunks = new List<ChunkCoords>(activeChunks);
            activeChunks.Clear();

            //Loop through all the Chunks in current view of the player
            for (int x = (coords.x - settings.viewWidthInChunks); x < (coords.x + settings.viewWidthInChunks); x++)
            {
                //int y = (coords.y + Data.viewHeightInChunks - 1); y > (coords.y - Data.viewHeightInChunks); y--
                for (int y = 0; y < VoxelData.viewHeightInChunks; y++)
                {
                    for (int z = (coords.z - settings.viewWidthInChunks); z < (coords.z + settings.viewWidthInChunks); z++)
                    {
                        //Check if the chunk is in the world
                        ChunkCoords currentChunkCoords = new ChunkCoords(x, y, z);
                        if (isChunkInWorld(currentChunkCoords))
                        {
                            //Check if it hasn't been created yet and queue it to be created
                            if (chunks[x, y, z] == null)
                            {
                                chunks[x, y, z] = new Chunk(currentChunkCoords, this, mat);
                                chunksToCreate.Add(currentChunkCoords);
                            }

                            activeChunks.Add(currentChunkCoords);
                        }

                        //Check through the previously active chunks to see if any chunks are still active, if they are then remove them for the list
                        for (int i = 0; i < previouslyActiveChunks.Count; i++)
                            if (previouslyActiveChunks[i].Equals(currentChunkCoords))
                                previouslyActiveChunks.RemoveAt(i);
                    }
                }
            }

            //Check if it was previously active and then reactivate it
            foreach (ChunkCoords c in activeChunks)
                if (!chunks[c.x, c.y, c.z].isActive)
                {
                    chunks[c.x, c.y, c.z].isActive = true;
                }

            //Any chunks left in the previousActiveChunks list are no longer in the player's view distance, so loop through and disable them
            foreach (ChunkCoords c in previouslyActiveChunks)
                chunks[c.x, c.y, c.z].isActive = false;
        }
        viewDistanceCheck = false;
    }

    public ChunkCoords getChunkCoordsFromVector3(Vector3 pos)
    {
        ChunkCoords chunkCoords = new ChunkCoords(Mathf.FloorToInt(pos.x / VoxelData.chunkWidth), Mathf.FloorToInt(pos.y / VoxelData.chunkHeight), Mathf.FloorToInt(pos.z / VoxelData.chunkWidth));
        return chunkCoords;
    }

    public Chunk getChunkFromVector3(Vector3 pos)
    {
        ChunkCoords chunkCoords = getChunkCoordsFromVector3(pos);
        return chunks[chunkCoords.x, chunkCoords.y, chunkCoords.z];
    }

    private void loadScreen()
    {
        loadingScreen.SetActive(false);
        Camera.main.GetComponent<CameraController>().enableCamera = true;
        isFirstLoad = false;
    }

    public int getBiome(Vector2Int pos)
    {
        double strongestWeight = 0f;
        int strongestBiomeIndex = 0;

        for (int i = 0; i < biomes.Length; i++)
        {
            double weight = Utils.Get2DPerlin(new Vector2(pos.x, pos.y), biomes[i].offset, biomes[i].scale);

            //Which weight is higher
            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }
        }

        //Set biome to the one with the strongest weight       

        return strongestBiomeIndex;
    }

    public int getTerrainHeight(Vector2Int pos, BiomeAttributes biome)
    {
        int terrainHeight = (int)(Utils.Evaluate2D(pos, 0, biome.terrainScale) * biome.terrainHeight + VoxelData.solidGroundHeight);
        return terrainHeight;
    }

    public int getVoxel(Vector3Int pos, int terrainHeight)
    {
        //Vector3Int pos = data.pos + data.globalPos;
        //int yPos = Mathf.FloorToInt(pos.y);

        int yPos = pos.y;

        //IMMUTABLE PASS

        if (!isVoxelInWorld(pos)) // if outside the world return air block
            return 0;

        if (yPos == 0) // If bottom block of chunk, return bedrock
            return 2;


        //BIOME SELECTION PASS
        BiomeAttributes biome = biomes[getBiome(new Vector2Int(pos.x, pos.z))];
        //float currentTerrainHeight = getTerrainHeight(new Vector2Int(pos.x, pos.z), biome);

        //int terrainHeight = (int)currentTerrainHeight;

        //BASIC TERRAIN PASS

        int voxelValue = 0;

        if (yPos == terrainHeight)
            voxelValue = biome.surfaceBlock;
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = biome.subSurfaceBlock;
        else if (yPos < terrainHeight - 3)
            voxelValue = 4;
        else
            voxelValue = 0;

        //SECOND PASS
        if (voxelValue == 4)
        {
            foreach (veinSpawns ores in biome.oreVeins)
            {
                if (yPos > ores.minHeight && yPos < ores.maxHeight)
                    if (Utils.Get3DPerlin(pos, ores.noiseOffset, ores.scale, ores.threshold))
                        voxelValue = ores.blockID;
            }
        }

        //WATER PASS
        if (yPos <= VoxelData.waterHeight + VoxelData.chunkHeight)
            if (voxelValue == 0)
                voxelValue = 13;

        //TREE PASS
        if (yPos == terrainHeight + 1 && voxelValue == 0 && biome.placeMajorFoliage)
        {
            if (Utils.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.majFZoneScale) > biome.majFZoneThreshold)
            {
                if (Utils.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.majFPlacementScale) > biome.majFPlacementThreshold)
                {
                    voxelValue = 9;
                    chunkUpdateThread[pos.y/VoxelData.chunkHeight].modifications.Enqueue(Structure.makeTree(pos, biome.minHeight, biome.maxHeight));
                }
                else
                    voxelValue = 0;
            }

            else
                voxelValue = 0;
        }

        //FOLIAGE PASS
        if (yPos == terrainHeight + 1 && voxelValue == 0)
        {
            if (Utils.Evaluate2D(new Vector2(pos.x, pos.z), 0, biome.minFZoneScale) > biome.minFZoneThreshold)
            {
                //if (Utils.Evaluate2D(new Vector2(pos.x, pos.z), 0, biome.minFZoneScale) > biome.minFZoneThreshold)
                //{
                    voxelValue = 14;
                //}
            }
        }

        return voxelValue;
    }

    private void createChunk()
    {
        ChunkCoords c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        chunks[c.x, c.y, c.z].init(mat);
    }

    public bool checkForVoxel(Vector3 pos)
    {
        ChunkCoords thisChunkPos = new ChunkCoords(pos);
        Vector3Int posInt = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
        Chunk thisChunkObject = chunks[thisChunkPos.x, thisChunkPos.y, thisChunkPos.z];

        if (!isVoxelInWorld(pos))
            return false;

        if (thisChunkObject != null && thisChunkObject.isEditable)
        {
            if (thisChunkObject.getVoxelFromGlobalVector3(pos) == 1)
                return true;
            else
                return blockType[(thisChunkObject.getVoxelFromGlobalVector3(pos))].isSolid;
        }

        BiomeAttributes biome = biomes[getBiome(new Vector2Int(posInt.x, posInt.z))];
        int currentTerrainHeight = (int)getTerrainHeight(new Vector2Int(posInt.x, posInt.z), biome);

        return blockType[getVoxel(posInt, currentTerrainHeight)].isSolid;
    }


    public bool isChunkInWorld(ChunkCoords coords)
    {
        if (coords.x >= 0 && coords.x < VoxelData.worldWidth && coords.y >= 0 && coords.y <= VoxelData.worldHeight * 2 && coords.z >= 0 && coords.z < VoxelData.worldWidth)
            return true;
        else
            return false;
    }
       

    bool isVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.worldWidthInBlocks && pos.y >= 0 && pos.y <= VoxelData.worldHeightInBlocks * 2 && pos.z >= 0 && pos.z < VoxelData.worldWidthInBlocks)
            return true;
        else
            return false;
    }


    public BlockType checkForVoxelBlockType(Vector3 pos)
    {
        ChunkCoords thisChunkPos = new ChunkCoords(pos);
        Vector3Int posInt = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
        Chunk thisChunkObject = chunks[thisChunkPos.x, thisChunkPos.y, thisChunkPos.z];
        if (!isVoxelInWorld(pos))
            return null;

        if (thisChunkObject != null && thisChunkObject.isEditable)
            return blockType[(thisChunkObject.getVoxelFromGlobalVector3(pos))];

        BiomeAttributes biome = biomes[getBiome(new Vector2Int(posInt.x, posInt.z))];
        int currentTerrainHeight = (int)getTerrainHeight(new Vector2Int(posInt.x, posInt.z), biome);

        return blockType[getVoxel(posInt, currentTerrainHeight)];
    }

    public GameObject placeFurniture(BlockType furniture, ObjectGhost objGhost, Vector3 pos, Transform chunk)
    {
        GameObject furnitureObj = objGhost.getgameObj();
        GameObject furnitureFinal = Instantiate(furnitureObj, furnitureObj.transform.position, furnitureObj.transform.rotation);

        MeshRenderer furnObjectRenderer = furnitureFinal.GetComponent<MeshRenderer>();
        objGhost.changeShader(furnObjectRenderer.materials, FindObjectOfType<CameraController>().ghostMat, true);
        furnObjectRenderer.materials = furniture.furniture.GetComponent<MeshRenderer>().sharedMaterials;
        furnitureFinal.name = furniture.name;
        furnitureFinal.transform.SetParent(chunk);

        if (furnitureFinal.GetComponent<JobsBase>())
        {
            JobsBase jobRef = furnitureFinal.GetComponent<JobsBase>();
            jobRef.IsGhost = false;
            jobRef.CurrentRotation = objGhost.CurrentRotation;
        }

        return furnitureFinal;
        //Debug.Log(pos);
        //Debug.Log("Done");
    }

    private void OnDisable()
    {
        for (int i = 0; i < chunkUpdateThread.Length; i++)
            chunkUpdateThread[i].OnDisable();
    }
}