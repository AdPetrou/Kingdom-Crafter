using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobDistribution : MonoBehaviour
{
    public colonist placeholderColonist;
    public GameObject MARKER;

    public Queue<colonist> bedlessColonists = new Queue<colonist>();
    public Queue<colonist> joblessColonists = new Queue<colonist>();

    public List<List<GameObject>> jobs = new List<List<GameObject>>();
    private List<Queue<int>> emptyJobsQueue = new List<Queue<int>>();

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < VoxelData.armountOfJobTypes; i++)
        {
            jobs.Add(new List<GameObject>());
            emptyJobsQueue.Add(new Queue<int>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(bedlessColonists.Count > 0 && emptyJobsQueue[VoxelData.bedJobID].Count > 0)
        {
            //Debug.Log("emptyJobsQueue size " + emptyJobsQueue[Data.bedJobID].Count + " bedlessColonists size " + bedlessColonists.Count);
            lock (emptyJobsQueue)
            {
                lock (bedlessColonists)
                {
                    //Debug.Log("Bed to be filled: " + emptyJobsQueue[Data.bedJobID].Peek());
                    //Debug.Log("Colonist: " + bedlessColonists.Peek().ColonistName);
                    jobs[VoxelData.bedJobID][emptyJobsQueue[VoxelData.bedJobID].Dequeue()].GetComponent<BedJob>().addWorkerToJob(bedlessColonists.Dequeue());
                }
            }
            //Debug.Log("emptyJobsQueue size " + emptyJsobsQueue[Data.bedJobID].Count + " bedlessColonists size " + bedlessColonists.Count);
        }
    }

    //Jobs List//
    //---------------------------------//
    public int getListCount(int baseID)
    {
        return jobs[baseID].Count;
    }

    public void addToList(int baseID, GameObject jobRef)
    {
        jobs[baseID].Add(jobRef);
        //Debug.Log(jobs[baseID].Count);
    }

    public void removeFromList(int baseID, int jobRef)
    {
        jobs[baseID][jobRef] = new GameObject();
    }
    //---------------------------------//



    //Jobs Queue//
    //---------------------------------//
    public int getQueueCount(int baseID)
    {
        return emptyJobsQueue[baseID].Count;
    }

    public void addToQueue(int baseID, int refID)
    {
        emptyJobsQueue[baseID].Enqueue(refID);
    }
    //---------------------------------//
}
