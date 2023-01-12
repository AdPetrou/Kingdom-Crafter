using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class WorldThread
{
    //private Thread thread;
    private int threadNo;
    private World world;

    public List<Chunk> chunksToGenerate { get; } = new List<Chunk>();
    public List<Chunk> chunksToUpdate { get; } = new List<Chunk>();
    public Queue<Queue<VoxelMod>> modifications { get; set; } = new Queue<Queue<VoxelMod>>();

    bool applyingMods = false, applyingUpdates = false, generating = false;

    private bool exitThread;

    public WorldThread(World _world, int _threadNo)
    {
        world = _world;
        //thread = new Thread(new ThreadStart(threadedUpdate));
        //thread.Name = _threadNo.ToString();
        threadNo = _threadNo;

        chunksToGenerate = new List<Chunk>();
        chunksToUpdate = new List<Chunk>();

        exitThread = false;
        //thread.Start();
    }

    public void threadedUpdate()
    {
        //Task mods = new Task(() => applyModifications());
        //Task update = new Task(() => updateChunks(threadNo));
        //Task generate = new Task(() => generateChunks(threadNo));

        //if (modifications.Count > 0 && !applyingMods)
        //{
        //    applyingMods = true;
        //    applyModifications();
        //}

        Task.Factory.StartNew(() =>
        {
            if (!applyingUpdates)
            {
                if (chunksToUpdate.Count > 0)
                {
                    applyingUpdates = true;
                    updateChunks();
                }
            }  
        });

        Task.Factory.StartNew(() =>
        {
            if (!generating)
            {
                if (chunksToGenerate.Count > 0)
                {
                    generating = true;
                    generateChunks();
                }
            }
        });
    }

    private void updateChunks()
    {
        lock (chunksToUpdate)
        {
            while (chunksToUpdate.Count > 0)
            {
                chunksToUpdate[0].buildChunk();
                chunksToUpdate.RemoveAt(0);
            }
            applyingUpdates = false;
        }
    }

    private void generateChunks()
    {
        lock (chunksToGenerate)
        {
            while (chunksToGenerate.Count > 0)
            {
                if (chunksToGenerate[0] != null)
                {
                    chunksToGenerate[0].populateVoxelArray();
                    chunksToGenerate.RemoveAt(0);
                }
            }
            generating = false;
        }
    }

    private void applyModifications()
    {
        lock (modifications)
        {
            if (modifications.Count > 0)
            {
                //UnityEngine.Debug.Log(modifications.Count);
                Queue<VoxelMod> queue = modifications.Dequeue();

                while (queue.Count > 0)
                {
                    VoxelMod v = queue.Dequeue();

                    ChunkCoords c = world.getChunkCoordsFromVector3(v.position);

                    if (world.chunks[c.x, c.y, c.z] == null)
                    {
                        world.chunks[c.x, c.y, c.z] = new Chunk(c, world, world.mat);
                        world.chunksToCreate.Add(c);
                    }

                    world.chunks[c.x, c.y, c.z].modifications.Enqueue(v);

                    for (int i = 0; i < VoxelData.worldHeight; i++)
                    {
                        if (!chunksToUpdate.Contains(world.chunks[c.x, c.y, c.z]))
                            chunksToUpdate.Add(world.chunks[c.x, c.y, c.z]);
                    }
                }
            }

            applyingMods = false;
        }
    }

    public void OnDisable()
    {
        exitThread = true;
    }
}
