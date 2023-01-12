using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BedJob : JobsBase
{
    private Cycle sunCycle;
    private Vector3 sleepPos;

    public int BedID
    {
        get { return bedID; }
        set { bedID = value; }
    }
    protected int bedID;

    public bool LyingDown
    {
        get { return lyingDown; }
        set { lyingDown = value; }
    }
    protected bool lyingDown;

    void Start()
    {
        isInit = false;
    }

    void Update()
    {
        if (!isInit && !isGhost)
            init();

        if (!isGhost && !inUse && sunCycle.returnTimeOfDay() >= VoxelData.sleepTime && sunCycle.returnTimeOfDay() < VoxelData.workTime)
        {
            goToJob();
        }
        else if (!isGhost && inUse && sunCycle.returnTimeOfDay() < VoxelData.sleepTime && sunCycle.returnTimeOfDay() >= VoxelData.workTime)
        {
            inUse = false;
        }

        if (inUse && assignedWorkers[0].isPathEmpty() && !lyingDown && assignedWorkers[0].transform.position  == controlPosition + new Vector3(0.5f, 1f, 0.5f))
        {
            //Debug.Log("Col Pos: " + assignedWorkers[0].transform.position); Debug.Log("Control Pos: " + controlPosition);
            Transform workerTransform = assignedWorkers[0].gameObject.transform;
            workerTransform.position = sleepPos;
            workerTransform.eulerAngles = new Vector3(workerTransform.eulerAngles.x - 90f, this.transform.eulerAngles.y - 90f, this.gameObject.transform.eulerAngles.z);
            lyingDown = true;
        }

        if (!inUse && lyingDown)
        {
            Transform workerTransform = assignedWorkers[0].gameObject.transform;
            workerTransform.position = controlPosition + new Vector3(0.5f, 1f, 0.5f);
            workerTransform.eulerAngles = new Vector3(workerTransform.eulerAngles.x + 90f, this.transform.eulerAngles.y + 90f, this.gameObject.transform.eulerAngles.z);
            lyingDown = false;
        }

        //Debug.Log("Worker pos " + (assignedWorkers[0].transform.position - new Vector3(0.5f, 0f, 0.5f)).ToString());
        //Debug.Log("Control pos " + controlPosition.ToString());
    }

    public void init()
    {
        isInit = true;
        inUse = false;
        lyingDown = false;
        jobDist = FindObjectOfType<JobDistribution>();
        bedID = jobDist.getListCount(VoxelData.bedJobID);       
        jobDist.addToList(VoxelData.bedJobID, this.gameObject);
        jobDist.addToQueue(VoxelData.bedJobID, bedID);

        calculateBlockPositions();

        jobName = "Bed";

        sunCycle = FindObjectOfType<Cycle>();

        //Instantiate(jobDist.MARKER, controlPosition + Data.x/2 + Data.z/2, Quaternion.identity);
        //Instantiate(jobDist.MARKER, jobBlockPosition + Data.x / 2 + Data.z / 2, Quaternion.identity);

        populateWorkerArray(1);

        //Debug.Log("Col Pos: " + assignedWorkers[0].transform.position); Debug.Log("Control Pos: " + controlPosition);
    }

        //if (FindObjectOfType<World>().checkForVoxel(this.transform.position - this.transform.up - Data.y))
            //calculateControlBlock();
        //else
            //calculateControlBlock();

    public void calculateBlockPositions()
    {
        World world = FindObjectOfType<World>();

        switch (currentRotation)
        {
            case 0:
                JobBlockPosition = this.transform.position;
                controlPosition = jobBlockPosition + VoxelData.z - VoxelData.y;
                sleepPos = jobBlockPosition + VoxelData.y - (new Vector3(this.transform.right.x, 0f, this.transform.right.z) * 2.5f);
                if (!world.checkForVoxel(controlPosition))
                    controlPosition -= VoxelData.z * 3;
                return;

            case 1:
                JobBlockPosition = this.transform.position - VoxelData.z;
                controlPosition = jobBlockPosition + VoxelData.x - VoxelData.y;
                sleepPos = jobBlockPosition + VoxelData.y - (new Vector3(this.transform.right.x, 0f, this.transform.right.z) * 2.5f);
                sleepPos += VoxelData.z;
                if (!world.checkForVoxel(controlPosition))
                    controlPosition -= VoxelData.x * 3;
                return;

            case 2:
                JobBlockPosition = this.transform.position - VoxelData.z - VoxelData.x;
                controlPosition = jobBlockPosition - VoxelData.z - VoxelData.y;
                sleepPos = jobBlockPosition + VoxelData.y - (new Vector3(this.transform.right.x, 0f, this.transform.right.z) * 2.5f);
                sleepPos += VoxelData.x; sleepPos += VoxelData.z;
                if (!world.checkForVoxel(controlPosition))
                    controlPosition += VoxelData.z * 3;
                return;

            case 3:
                JobBlockPosition = this.transform.position - VoxelData.x;
                controlPosition = jobBlockPosition - VoxelData.x - VoxelData.y;
                sleepPos = jobBlockPosition + VoxelData.y - (new Vector3(this.transform.right.x, 0f, this.transform.right.z) * 2.5f);
                sleepPos += VoxelData.x;
                if (!world.checkForVoxel(controlPosition))
                    controlPosition += VoxelData.x * 3;
                return;
        }
    }
}
