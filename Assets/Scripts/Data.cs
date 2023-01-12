using UnityEngine;

public static class VoxelData
{
    //----------------------------------//
    //           WORLD DATA             //
    //----------------------------------//

    public static readonly Vector3 x = new Vector3(1, 0, 0);
    public static readonly Vector3 y = new Vector3(0, 1, 0);
    public static readonly Vector3 z = new Vector3(0, 0, 1);

    private static int _worldWidth;

    public static int worldWidth
    {
        get { return _worldWidth; }
    }

    public static readonly int solidGroundHeight = 140;

    public static readonly int waterHeight = 30;
    public static readonly int worldHeight = 2;

    public static readonly int chunkWidth = 16;
    public static readonly int chunkHeight = 128;

    public static int worldWidthInBlocks
    {
        get { return VoxelData.worldWidth * VoxelData.chunkWidth; }
    }
    public static int worldHeightInBlocks
    {
        get { return VoxelData.worldHeight * VoxelData.chunkHeight; }
    }

    public static readonly int TextureAtlasSizeInBlocks = 16;
    public static float NormalisedBlockTextureSize
    {
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }

    public static readonly int viewHeightInChunks = 2;

    public static readonly float timeMultiplier = 1f;

    public static readonly Vector3[] VertexCoords = new Vector3[8] 
    //Storage for all possible coordinates of a corner of a cube
    //Will be retrieved and used later when generating a cube mesh to keep code clean and easy to thread
    {
        new Vector3(0f,0f,0f),

        new Vector3(0f,0f,1f),
        new Vector3(0f,1f,0f),
        new Vector3(1f,0f,0f),

        new Vector3(0f,1f,1f),
        new Vector3(1f,0f,1f),
        new Vector3(1f,1f,0f),

        new Vector3(1f,1f,1f),
    };

    public static readonly int[,] VertexOrder = new int[6, 4]
    //All location for the Coordinates on the quad for each face
    {
        {0, 2, 6, 3}, //Front
        {2, 4, 7, 6}, //Top
        {3, 6, 7, 5}, //Right
        {1, 0, 3, 5}, //Bottom
        {1, 4, 2, 0}, //Left
        {5, 7, 4, 1}, //Back
    };

    public static readonly int[] tris = new int[6] { 0, 1, 3, 1, 2, 3 };

    public static readonly Vector3[] checkVoxel = new Vector3[6]
    {
        new Vector3(0f,0f,-1f),
        new Vector3(0f,1f,0f),
        new Vector3(1f,0f,0f),
        new Vector3(0f,-1f,0f),
        new Vector3(-1f,0f,0f),
        new Vector3(0f,0f,1f)
    };

    public static void setWorldWidth(int width)
    {
        _worldWidth = width;
    }

    public static readonly Vector3[] waterVertices = new Vector3[4] { new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 0) };

    public static readonly Vector2[,] waterUVs = new Vector2[4, 4]
    {
        { new Vector2(0, 0), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0) },
        { new Vector2(0, 0.5f), new Vector2(0, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f)},
        { new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0) },
        { new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0.5f) }
    };

    public static readonly Vector3[,] grassVertices = new Vector3[2, 4]
    {
        //{ new Vector3(0, 0, 0.5f), new Vector3(0, 0.75f, 0.6f), new Vector3(1, 0.75f, 0.6f), new Vector3(1, 0, 0.5f) },
        //{ new Vector3(0.4f, 0, 1), new Vector3(0.4f, 0.75f, 1f), new Vector3(0.4f, 0.75f,0), new Vector3(0.5f, 0, 0) },
        { new Vector3(0, 0, 0), new Vector3(0, 0.6f, 0), new Vector3(1, 0.6f, 1), new Vector3(1, 0, 1) },
        { new Vector3(0, 0, 1), new Vector3(0, 0.6f, 1), new Vector3(1, 0.6f, 0), new Vector3(1, 0, 0) }
    };

    //public static readonly Vector3[,] grassVertices = new Vector3[2, 4]
    //{
    //    { new Vector3(-0.5f, 0, 0.5f), new Vector3(-0.5f, 1, 0.5f), new Vector3(1.5f, 1, 0.5f), new Vector3(1.5f, 0, 0.5f) },
    //    { new Vector3(0.5f, 0, -0.5f), new Vector3(0.5f, 1, -0.5f), new Vector3(0.5f, 1f, 1.5f), new Vector3(0.5f, 0, 1.5f) },
    //};

    public static readonly int smoothArea = 4;  

    public static readonly Vector2[] grassUVs = new Vector2[4]{ new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };

    //----------------------------------//
    //            JOB DATA              //
    //----------------------------------//

    public static readonly int armountOfJobTypes = 1;
    public static readonly float workTime = 0.9f, sleepTime = 0.55f;

    // Job ID's
    public static readonly int bedJobID = 0;
}