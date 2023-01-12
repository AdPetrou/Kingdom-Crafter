using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobsBase : MonoBehaviour
{
    protected bool isInit;

    public int CurrentRotation
    {
        get { return currentRotation; }
        set { currentRotation = value; }
    }
    protected int currentRotation;

    public bool IsGhost
    {
        get { return isGhost; }
        set { isGhost = value; }
    }
    protected bool isGhost = true;

    public Vector3 ControlPosition
    {
        get { return controlPosition; }
        set { controlPosition = value; }
    }
    protected Vector3 controlPosition;

    public Vector3 JobBlockPosition
    {
        get { return jobBlockPosition; }
        set { jobBlockPosition = value; }
    }
    protected Vector3 jobBlockPosition;

    public Quaternion JobBlockRotation
    {
        get { return jobBlockRotation; }
        set { jobBlockRotation = value; }
    }
    protected Quaternion jobBlockRotation;
    protected JobDistribution jobDist;

    public string JobName
    {
        get { return jobName; }
        set { jobName = value; }
     }
    protected string jobName;

    protected colonist[] assignedWorkers;

    public bool InUse
    {
        get { return inUse; }
        set { inUse = value; }
    }
    protected bool inUse;

    protected void populateWorkerArray(int maxWorkers)
    {
        //Debug.Log("populateWorkerArray Pass");
        assignedWorkers = new colonist[maxWorkers];

        for (int i = 0; i < maxWorkers; i++)
        {
            assignedWorkers[i] = jobDist.placeholderColonist;
            assignedWorkers[i].colonistName = "Placeholder";
            //Debug.Log(assignedWorkers[i].ColonistName);
        }
    }

    public void addWorkerToJob(colonist worker)
    {
        int id = findEmptySlot();
        if(!worksThisJob(worker) && id != -1)
            assignedWorkers[id] = worker;

        //Debug.Log(assignedWorkers.Length);
        //Debug.Log(id);
        //Debug.Log(assignedWorkers[id].ColonistName);

        return;
    }

    public void removeWorkerFromJob(colonist worker)
    {
        if (worksThisJob(worker))
            assignedWorkers[findWorkerID(worker)] = new colonist();

        return;
    }

    public int findWorkerID(colonist worker)
    {
        int id = -1;

        for(int i = 0; i < assignedWorkers.Length; i++)
        {
            if (worker.colonistName == assignedWorkers[i].colonistName)
            {
                id = i;
                break;
            }
        }

        return id;
    }

    public int findEmptySlot()
    {
        int id = -1;

        for(int i = 0; i < assignedWorkers.Length; i++)
        {
            //Debug.Log(assignedWorkers[i].ColonistName);
            if(assignedWorkers[i].colonistName == "Placeholder")
            {
                id = i;
                break;
            }
        }

        return id;
    }

    public bool worksThisJob(colonist worker)
    {
        if (findWorkerID(worker) == -1)
            return false;
        else
            return true;
    }

    public void moveWorkerToControlBlock(colonist worker)
    {
        if (worksThisJob(worker))
        {
            worker.moveWorker(controlPosition);
        }

        return;
    }

    public void moveWorkerToJobBlock(colonist worker)
    {
        if (worksThisJob(worker))
        {
            worker.moveWorker(jobBlockPosition);
        }

        return;
    }

    public void goToJob()
    {
        if (assignedWorkers[0].colonistName != "Placeholder")
        {
            RaycastHit hit;
            if (Physics.Raycast(ControlPosition + (Vector3.up * 10), Vector3.down, out hit))
            {
                //Vector3 position = new Vector3(hit.point.x, Mathf.Round(hit.point.y), hit.point.z);
                assignedWorkers[0].moveWorker(hit.point);
                //FindObjectOfType<World>().getChunkFromVector3(hit.point).editVoxel(hit.point, 2);
            }
            inUse = true;
        }
    }

    public void calculateControlBlock()
    {
        switch (currentRotation)
        {
            case 0:
                controlPosition = jobBlockPosition - VoxelData.x;
                return;

            case 1:
                controlPosition = jobBlockPosition + VoxelData.z;
                return;

            case 2:
                controlPosition = jobBlockPosition + VoxelData.x;
                return;

            case 3:
                controlPosition = jobBlockPosition - VoxelData.z;
                return;
        }
    }
}
