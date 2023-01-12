using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public struct pathfindStore
{
    public Queue<Vector3> path;
    public Queue<byte> rotation;
    public Vector3 currentPosition, endingPosition, offset;
}

public struct pathfindStoreList
{
    public List<Vector3> path;
    public List<byte> rotation;
    public Vector3 currentPosition;
    public bool moved;
}

public class mainColonistControl : MonoBehaviour
{    
    Vector3 x = VoxelData.x; Vector3 y = VoxelData.y; Vector3 z = VoxelData.z;
    Vector3[] xArr, zArr;

    public World world;
    public bool placing = false;
    public GameObject currentColonistSelect;

    private GameObject colonistToPlace;
    public Camera mainCam;
    public GameObject[] colonistTemplates;
    public List<GameObject> colonistsInPlay;

    private RaycastHit hit;
    public int NoOfColonists
    {
        get { return noOfColonists; }
        set { noOfColonists = value; }
    }
    protected int noOfColonists;

    //Vector3 currentPosition, endingPosition, offset;
    List<Vector3> blacklistedDirections = new List<Vector3>();
    byte[] rotationRefs = new byte[4] { 1, 3, 0, 2 };

    public Queue<pathfindStore> pathsToFind = new Queue<pathfindStore>();
    public Queue<colonist> colonistRef = new Queue<colonist>();
    private Thread pathfindThread;
    public object threadLock;

    // Start is called before the first frame update
    void Start()
    {
        xArr = new Vector3[2] { x, -x };
        zArr = new Vector3[2] { z, -z };

        bool enableThreading = world.settings.enableThreading;
        if (enableThreading)
        {
            pathfindThread = new Thread(new ThreadStart(pathfindThreadStart));
            threadLock = new object();
            pathfindThread.Name = "Path Thread";
            pathfindThread.Start();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (placing)
        {
            Ray ray = mainCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z));
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 blockHit = new Vector3(Mathf.FloorToInt(hit.point.x), Mathf.Round(hit.point.y), Mathf.FloorToInt(hit.point.z)) + Vector3.down;
                if (world.checkForVoxel(blockHit))
                    colonistToPlace.transform.position = blockHit + new Vector3(0.5f,1,0.5f); //* 2 + new Vector3(0.5f,0,0.5f);
            }
            if (Input.GetMouseButtonDown(0))
            {
                colonistToPlace.GetComponent<colonist>().hollowCol = false;
                colonistsInPlay.Add(colonistToPlace);
                placing = false;
            }
        }
    }

    public void placeWorker()
    {
        Ray ray = mainCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z));

        System.Random rnd = new System.Random();

        if (Physics.Raycast(ray, out hit))
        {
            int colonistNumber = rnd.Next(0, colonistTemplates.Length);
            noOfColonists += 1;
            colonistToPlace = Instantiate(colonistTemplates[colonistNumber], hit.point, colonistTemplates[colonistNumber].transform.rotation);
            colonistToPlace.transform.parent = this.gameObject.transform;
            colonistToPlace.name = "colonist " + noOfColonists;       
        }
        placing = true;
    }

    public GameObject findClosestColonistToVector3(Vector3 start)
    {
        float closestDistance = -1;
        GameObject closestColonist = null;

        for(int i = 0; i < colonistsInPlay.Count; i++)
        {
            float currentDistance = Vector3.Distance(colonistsInPlay[i].transform.position, start);

            if (currentDistance < closestDistance || closestDistance < 0)
            {
                closestDistance = currentDistance;
                closestColonist = colonistsInPlay[i];
            }
        }

        return closestColonist;
    }

    private void pathfindThreadStart()
    {
        while (true)
        {
            if(pathsToFind.Count > 0 && colonistRef.Count > 0)
                colonistRef.Dequeue().setPath(pathfind(pathsToFind.Dequeue()));
        }
    }

    public pathfindStore pathfind(pathfindStore pathfindInformation)
    {
        pathfindStore path = pathfindInformation;

        path.path = new Queue<Vector3>(); path.rotation = new Queue<byte>();
        //path.offset = pathfindInformation.offset; path.currentPosition = pathfindInformation.currentPosition; path.endingPosition = pathfindInformation.endingPosition;
        int xDistance = 0; int zDistance = 0;

        lock (threadLock)
        {
            while (path.currentPosition != path.endingPosition)
            {
                xDistance = calculateMagnitude(path.currentPosition.x, path.endingPosition.x);
                zDistance = calculateMagnitude(path.currentPosition.z, path.endingPosition.z);

                if (xDistance >= zDistance)
                {
                    if (path.currentPosition.x < path.endingPosition.x)
                        path = xPosCheck(path);
                    else if (path.currentPosition.x > path.endingPosition.x)
                        path = xNegCheck(path);
                    else break;
                }

                else if (xDistance < zDistance)
                {
                    if (path.currentPosition.z < path.endingPosition.z)
                        path = zPosCheck(path);
                    else if (path.currentPosition.z > path.endingPosition.z)
                        path = zNegCheck(path);
                    else break;
                }

                else break;
            }

            if (path.currentPosition == path.endingPosition)
            {
                //Debug.Log("Successful");
                blacklistedDirections.Clear();
                return path;
            }
            else
                return path;
        }
    }

    public int calculateMagnitude(float a, float b)
    {
        int magnitude = 0;

        magnitude = Mathf.Abs(Mathf.RoundToInt(a - b));

        return magnitude;
    }

    private bool checkFakeBlocks(Vector3 currentPosition, Vector3 pos)
    {
        if ((world.checkForVoxelBlockType(currentPosition + pos - y).byteID == 1) ||
            (world.checkForVoxelBlockType(currentPosition + pos).byteID == 1) ||
            (world.checkForVoxelBlockType(currentPosition + pos + y).byteID == 1))
            return true;

        else
            return false;
    }

    private pathfindStore xPosCheck(pathfindStore path)
    {
        byte xPosRotate = rotationRefs[0];
        //X POSITIVE

        //check 1 block above straight x-axis
        if (!world.checkForVoxel(path.currentPosition + x + 2 * y) && !checkFakeBlocks(path.currentPosition, x))
        {
            if (world.checkForVoxel(path.currentPosition + x + y))
            {
                path.currentPosition += (x + y);
                path.path.Enqueue(path.currentPosition + path.offset);
                path.rotation.Enqueue(xPosRotate);

                return path;
            }
            //check on straight x-axis
            else if (world.checkForVoxel(path.currentPosition + x))
            {
                path.currentPosition += x;
                path.path.Enqueue(path.currentPosition + path.offset);
                path.rotation.Enqueue(xPosRotate);

                return path;
            }
            //check 1 block below straight x-axis
            else if (world.checkForVoxel(path.currentPosition + x - y))
            {
                path.currentPosition += (x - y);
                path.path.Enqueue(path.currentPosition + path.offset);
                path.rotation.Enqueue(xPosRotate);

                return path;
            }

            else
                return path;
        }

        else
        {
            pathfindStoreList pathA = obstacleInPath(path.currentPosition, path.endingPosition, 0, 0);
            pathfindStoreList pathB = obstacleInPath(path.currentPosition, path.endingPosition, 0, 1);

            return queueQuickerPath(path, pathA, pathB);
        }
    }

    private pathfindStore xNegCheck(pathfindStore path)
    {
        byte xNegRotate = rotationRefs[1];
        //X NEGATIVE

        //check 1 block above straight negative x-axis
        if (!world.checkForVoxel(path.currentPosition - x + 2 * y) && !checkFakeBlocks(path.currentPosition, -x))
        {
            if (world.checkForVoxel(path.currentPosition - x + y))
            {
                path.currentPosition -= (x - y);
                path.path.Enqueue(path.currentPosition + path.offset);
                path.rotation.Enqueue(xNegRotate);

                return path;
            }
            //check on straight negative x-axis
            else if (world.checkForVoxel(path.currentPosition - x))
            {
                path.currentPosition -= x;
                path.path.Enqueue(path.currentPosition + path.offset);
                path.rotation.Enqueue(xNegRotate);

                return path;
            }
            //check 1 block below straight negative x-axis
            else if (world.checkForVoxel(path.currentPosition - x - y) )
            {
                path.currentPosition -= (x + y);
                path.path.Enqueue(path.currentPosition + path.offset);
                path.rotation.Enqueue(xNegRotate);

                return path;
            }

            else
                return path;
        }

        //Checks Left, Right and Behind if there is an obstacle in the way
        else
        {
            pathfindStoreList pathA = obstacleInPath(path.currentPosition, path.endingPosition, 1, 0);
            pathfindStoreList pathB = obstacleInPath(path.currentPosition, path.endingPosition, 1, 1);

            return queueQuickerPath(path, pathA, pathB);
        }
    }

    private pathfindStore zPosCheck(pathfindStore path)
    {
        byte zPosRotate = rotationRefs[2];
        //Z POSITIVE

        //check 1 block above straight z-axis
        if (!world.checkForVoxel(path.currentPosition + z + 2 * y) && !checkFakeBlocks(path.currentPosition, z))
        {
            if (world.checkForVoxel(path.currentPosition + z + y))
            {
                path.currentPosition += (z + y);
                path.path.Enqueue(path.currentPosition + path.offset);
                path.rotation.Enqueue(zPosRotate);

                return path;
            }
            //check on straight z-axis
            else if (world.checkForVoxel(path.currentPosition + z))
            {
                path.currentPosition += z;
                path.path.Enqueue(path.currentPosition + path.offset);
                path.rotation.Enqueue(zPosRotate);

                return path;
            }
            //check 1 block below straight z-axis
            else if (world.checkForVoxel(path.currentPosition + z - y))
            {
                path.currentPosition += (z - y);
                path.path.Enqueue(path.currentPosition + path.offset);
                path.rotation.Enqueue(zPosRotate);

                return path;
            }
            
            else
                return path;
        }

        //Checks Left, Right and Behind if there is an obstacle in the way
        else
        {
            pathfindStoreList pathA = obstacleInPath(path.currentPosition, path.endingPosition, 2, 0);
            pathfindStoreList pathB = obstacleInPath(path.currentPosition, path.endingPosition, 2, 1);

            return queueQuickerPath(path, pathA, pathB);          
        }
    }

    private pathfindStore zNegCheck(pathfindStore path)
    {
        byte zNegRotate = rotationRefs[3];
        //Z NEGATIVE

        //check 1 block above straight negative z-axis
        if (!world.checkForVoxel(path.currentPosition - z + 2 * y) && !checkFakeBlocks(path.currentPosition, -z))
        {
            if (world.checkForVoxel(path.currentPosition - z + y))
            {
                path.currentPosition -= (z - y);
                path.path.Enqueue(path.currentPosition + path.offset);
                path.rotation.Enqueue(zNegRotate);

                return path;
            }
            //check on straight negative z-axis
            else if (world.checkForVoxel(path.currentPosition - z))
            {
                path.currentPosition -= z;
                path.path.Enqueue(path.currentPosition + path.offset);
                path.rotation.Enqueue(zNegRotate);

                return path;
            }
            //check 1 block below straight negative z-axis
            else if (world.checkForVoxel(path.currentPosition - z - y))
            {
                path.currentPosition -= (z + y);
                path.path.Enqueue(path.currentPosition + path.offset);
                path.rotation.Enqueue(zNegRotate);

                return path;
            }

            else
                return path;
        }

        //obstacle check
        else
        {
            pathfindStoreList pathA = obstacleInPath(path.currentPosition, path.endingPosition, 3, 0);
            pathfindStoreList pathB = obstacleInPath(path.currentPosition, path.endingPosition, 3, 1);

            return queueQuickerPath(path, pathA, pathB);
        }
    }

    private pathfindStoreList obstacleInPath(Vector3 currentPosition, Vector3 endingPosition, int index, int bit)
    {
        pathfindStoreList path = new pathfindStoreList();
        path.rotation = new List<byte>();
        path.path = new List<Vector3>();
        path.moved = false;
        path.currentPosition = currentPosition;

        if (index < 2)
        {
            path.path.Add(currentPosition); path.path.Add(currentPosition);
            path.rotation.Add(rotationRefs[index]); path.rotation.Add(rotationRefs[index]);

            pathfindStoreList tempPath = path;
            tempPath.currentPosition += y;
            tempPath = pathXCheck(tempPath, index, bit, true);
            if (!tempPath.moved)
            {
                tempPath.currentPosition -= y;
                tempPath = pathXCheck(tempPath, index, bit, true);
                if (!tempPath.moved)
                {
                    tempPath.currentPosition -= y;
                    tempPath = pathXCheck(tempPath, index, bit, true);
                    if (!tempPath.moved)
                        return new pathfindStoreList();
                    else
                        path = tempPath;
                }
                else
                    path = tempPath;
            }
            else
                path = tempPath;

            while (path.currentPosition.x != endingPosition.x)
            {
                tempPath.currentPosition += y;
                tempPath = pathXCheck(tempPath, index, bit);
                if (!tempPath.moved)
                {
                    tempPath.currentPosition -= y;
                    tempPath = pathXCheck(tempPath, index, bit);
                    if (!tempPath.moved)
                    {
                        tempPath.currentPosition -= y;
                        tempPath = pathXCheck(tempPath, index, bit);
                        if (!tempPath.moved)
                            return new pathfindStoreList();
                        else
                            path = tempPath;
                    }
                    else
                        path = tempPath;
                }
                else
                    path = tempPath;               
            }
            return path;
        }

        if (index > 1)
        {
            index -= 2;
            path.path.Add(currentPosition); path.path.Add(currentPosition);
            path.rotation.Add(rotationRefs[index + 2]); path.rotation.Add(rotationRefs[index + 2]);

            pathfindStoreList tempPath = path;
            tempPath.currentPosition += y;
            tempPath = pathZCheck(tempPath, index, bit, true);
            if (!tempPath.moved)
            {
                tempPath.currentPosition -= y;
                tempPath = pathZCheck(tempPath, index, bit, true);
                if (!tempPath.moved)
                {
                    tempPath.currentPosition -= y;
                    tempPath = pathZCheck(tempPath, index, bit, true);
                    if (!tempPath.moved)
                        return new pathfindStoreList();
                    else
                        path = tempPath;
                }
                else
                    path = tempPath;
            }
            else
                path = tempPath;

            while (path.currentPosition.z != endingPosition.z)
            {
                tempPath.currentPosition += y;
                tempPath = pathZCheck(tempPath, index, bit);
                if (!tempPath.moved)
                {
                    tempPath.currentPosition -= y;
                    tempPath = pathZCheck(tempPath, index, bit);
                    if (!tempPath.moved)
                    {
                        tempPath.currentPosition -= y;
                        tempPath = pathZCheck(tempPath, index, bit);
                        if (!tempPath.moved)
                            return new pathfindStoreList();
                        else
                            path = tempPath;
                    }
                    else
                        path = tempPath;
                }
                else
                    path = tempPath;
              
            }
            return path;
        }

        return new pathfindStoreList();
    }

    private pathfindStoreList pathXCheck(pathfindStoreList path, int index, int bit, bool skipFirst = false)
    {
        path.moved = true;

        if (!skipFirst && (world.checkForVoxel(path.currentPosition + xArr[index]) && !world.checkForVoxel(path.currentPosition + xArr[index] + y) && !world.checkForVoxel(path.currentPosition + xArr[index] + 2 * y)
        && !alreadyVisited(path.path, path.currentPosition + xArr[index])) && !checkFakeBlocks(path.currentPosition, xArr[index]))
        {
            path.currentPosition += xArr[index];
            path.path.Add(path.currentPosition);
            path.rotation.Add(rotationRefs[index]);
            return path;
        }

        else if (world.checkForVoxel(path.currentPosition + zArr[bit]) && !world.checkForVoxel(path.currentPosition + zArr[bit] + y) && !world.checkForVoxel(path.currentPosition + zArr[bit] + 2 * y)
            && !alreadyVisited(path.path, path.currentPosition + zArr[bit]) && !checkFakeBlocks(path.currentPosition, zArr[bit]))
        {
            path.currentPosition += zArr[bit];
            path.path.Add(path.currentPosition);
            path.rotation.Add(rotationRefs[bit + 2]);
            return path;
        }

        else if (world.checkForVoxel(path.currentPosition - xArr[index]) && !world.checkForVoxel(path.currentPosition - xArr[index] + y) && !world.checkForVoxel(path.currentPosition - xArr[index] + 2 * y)
                && !alreadyVisited(path.path, path.currentPosition - xArr[index]) && !checkFakeBlocks(path.currentPosition, -xArr[index]))
        {
            path.currentPosition -= xArr[index];
            path.path.Add(path.currentPosition);
            path.rotation.Add(rotationRefs[index ^ 1]);
            return path;
        }

        else if (world.checkForVoxel(path.currentPosition - zArr[bit]) && !world.checkForVoxel(path.currentPosition - zArr[bit] + y) && !world.checkForVoxel(path.currentPosition - zArr[bit] + 2 * y)
        && !alreadyVisited(path.path, path.currentPosition - zArr[bit]) && !checkFakeBlocks(path.currentPosition, -zArr[bit]))
        {
            path.currentPosition -= zArr[bit];
            path.path.Add(path.currentPosition);
            path.rotation.Add(rotationRefs[(bit ^ 1) + 2]);
            return path;
        }

        else
        {
            path.moved = false;
            return path;
        }
    }

    private pathfindStoreList pathZCheck(pathfindStoreList path, int index, int bit, bool skipFirst = false)
    {
        path.moved = true;

        if (!skipFirst && (world.checkForVoxel(path.currentPosition + zArr[index]) && !world.checkForVoxel(path.currentPosition + zArr[index] + y) && !world.checkForVoxel(path.currentPosition + zArr[index] + 2 * y)
        && !alreadyVisited(path.path, path.currentPosition + zArr[index])) && !checkFakeBlocks(path.currentPosition, zArr[index]))
        {
            path.currentPosition += zArr[index];
            path.path.Add(path.currentPosition);
            path.rotation.Add(rotationRefs[index + 2]);
            return path;
        }

        else if (world.checkForVoxel(path.currentPosition + xArr[bit]) && !world.checkForVoxel(path.currentPosition + xArr[bit] + y) && !world.checkForVoxel(path.currentPosition + xArr[bit] + 2 * y)
            && !alreadyVisited(path.path, path.currentPosition + xArr[bit]) && !checkFakeBlocks(path.currentPosition, xArr[bit]))
        {
            path.currentPosition += xArr[bit];
            path.path.Add(path.currentPosition);
            path.rotation.Add(rotationRefs[bit]);
            return path;
        }

        else if (world.checkForVoxel(path.currentPosition - zArr[index]) && !world.checkForVoxel(path.currentPosition - zArr[index] + y) && !world.checkForVoxel(path.currentPosition - zArr[index] + 2 * y)
                && !alreadyVisited(path.path, path.currentPosition - zArr[index]) && !checkFakeBlocks(path.currentPosition, -zArr[index]))
        {
            path.currentPosition -= zArr[index];
            path.path.Add(path.currentPosition);
            path.rotation.Add(rotationRefs[(index ^ 1) + 2]);
            return path;
        }

        else if (world.checkForVoxel(path.currentPosition - xArr[bit]) && !world.checkForVoxel(path.currentPosition - xArr[bit] + y) && !world.checkForVoxel(path.currentPosition - xArr[bit] + 2 * y)
        && !alreadyVisited(path.path, path.currentPosition - xArr[bit]) && !checkFakeBlocks(path.currentPosition, -xArr[bit]))
        {
            path.currentPosition -= xArr[bit];
            path.path.Add(path.currentPosition);
            path.rotation.Add(rotationRefs[(bit ^ 1)]);
            return path;
        }

        else
        {
            path.moved = false;
            return path;
        }
    }

    private pathfindStore queueQuickerPath(pathfindStore path, pathfindStoreList pathA, pathfindStoreList pathB)
    {
        if (pathA.path.Count <= pathB.path.Count)
        {
            for (int i = 0; i < pathA.path.Count; i++)
            {
                path.path.Enqueue(pathA.path[i] + path.offset);
                path.rotation.Enqueue(pathA.rotation[i]);
            }

            path.currentPosition = pathA.path[pathA.path.Count - 1];
            return path;
        }
        else
        {
            for (int i = 0; i < pathB.path.Count; i++)
            {
                path.path.Enqueue(pathB.path[i] + path.offset);
                path.rotation.Enqueue(pathB.rotation[i]);
            }

            path.currentPosition = pathB.path[pathB.path.Count - 1];
            return path;
        }
    }

    private bool alreadyVisited(List<Vector3> path, Vector3 currentPos)
    {
        for (int i = 0; i < path.Count; i++)
            if (path[i] == currentPos)
                return true;

        return false;
    }

    private void OnDisable()
    {
        if (world.settings.enableThreading)
        {
            pathfindThread.Abort();
        }
    }
}