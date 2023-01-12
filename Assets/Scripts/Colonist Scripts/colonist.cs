using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class colonist : MonoBehaviour
{
    public int reach { get; } = 10;
    bool breakBlockStarted = false;

    private static bool firstPress = true;
    private static bool selectedInsteadOfMoving = false;
    private bool colonistBreakingBlock = false;

    Vector3 offset;
    private Vector3 currentPosition, endingPosition;

    private Queue<Vector3> blocksBreaking = new Queue<Vector3>();
    private Queue<Vector3> path = new Queue<Vector3>();
    private Queue<byte> rotation = new Queue<byte>();

    private World world;
    private Animator anim;
    private mainColonistControl mCC;
    private inventory inv;
    private InterfaceManager interMang;

    private float colonistSpeed = 2;

    Ray ray;
    private RaycastHit hit;

    public string colonistName { get; set; }

    public bool isPathEmpty()
    {
        if (path.Count == 0)
            return true;
        else
            return false;
    }

    public void setBlockToBreak(Vector3 block)
    {
        colonistBreakingBlock = true;
        blocksBreaking.Enqueue(block);
    }

    public bool hollowCol = true;

    public bool IsInit
    {
        get { return isInit; }
        set { isInit = value; }
    }
    protected bool isInit;

    // Start is called before the first frame update
    void Start()
    {
        isInit = false;
    }

    public void init()
    {
        mCC = FindObjectOfType<mainColonistControl>();
        colonistName = "Colonist " + (mCC.NoOfColonists);
        //Debug.Log("Colonist Name: " + colonistName);
        world = mCC.world;
        anim = this.gameObject.GetComponent<Animator>();
        inv = FindObjectOfType<inventory>();
        interMang = FindObjectOfType<InterfaceManager>();

        JobDistribution jobDist = FindObjectOfType<JobDistribution>();
        lock (jobDist.bedlessColonists)
        {
            jobDist.bedlessColonists.Enqueue(this);
        }
        lock (jobDist.joblessColonists)
        {
            jobDist.joblessColonists.Enqueue(this);
        }
        isInit = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(!hollowCol)
        {
            if (!isInit)
                init();

            selectedInsteadOfMoving = false;

            if (path.Count > 0)
            {
                updateMovement();
            }
            else
            {
                anim.SetBool("isWalking", false);
                anim.SetBool("isIdle", true);
            }

            randomIdleAnim();

            if (path.Count == 0 && colonistBreakingBlock && !breakBlockStarted)
            {
                StartCoroutine("breakBlock");
            }

            if (Input.GetMouseButtonDown(0) && firstPress)
            {
                firstPress = false;
                return;
            }
            else
            {
                if (Input.GetMouseButtonDown(0) && !interMang.InUI)
                {
                    if (!Input.GetButton("Shift"))
                    {
                        selectedInsteadOfMoving = selectWorker();

                        if (!selectedInsteadOfMoving && mCC.currentColonistSelect != null)
                        {
                            moveWorkerClick();
                        }
                    }
                }
            }
        }
    }

    private void moveWorkerClick()
    {
        if (mCC.currentColonistSelect.transform.position == this.gameObject.transform.position)
        {
            ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z));
            if (Physics.Raycast(ray, out hit))
            {
                moveWorker(hit.point);
            }

            else
            {
                mCC.currentColonistSelect = null;
                return;
            }
        }
    }

    private IEnumerator breakBlock()
    {
        breakBlockStarted = true;
        lock (path)
        {
            while (blocksBreaking.Count > 0)
            {
                if(path.Count == 0)
                {
                    for (int i = 0; i < inv.inventorySlots.Length; i++)
                    {
                        if (inv.inventorySlots[i].getByteID() == world.getChunkFromVector3(blocksBreaking.Peek()).getVoxelFromGlobalVector3(blocksBreaking.Peek()))
                        {
                            inv.inventorySlots[i].incrementAmount();
                            inv.changeAmountInText(inv.inventorySlots[i]);
                        }
                    }
                    world.getChunkFromVector3(blocksBreaking.Peek()).editVoxel(blocksBreaking.Dequeue(), 0);
                }
                yield return new WaitForSeconds(2);                
            }

            if (blocksBreaking.Count == 0)
            {
                colonistBreakingBlock = false;
                breakBlockStarted = false;
            }
        }
    }

    private bool selectWorker()
    {
        ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z));

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 colonistCheck = new Vector3(Mathf.FloorToInt(hit.point.x), Mathf.Round(hit.point.y), Mathf.FloorToInt(hit.point.z)) + Vector3.up + new Vector3(0.5f, 0, 0.5f);
            if (colonistCheck.x >= this.transform.position.x - 0.5f && colonistCheck.x <= this.transform.position.x + 0.5f)
            {
                if (colonistCheck.y >= this.transform.position.y && colonistCheck.y <= this.transform.position.y + 5)
                {
                    if (colonistCheck.z >= this.transform.position.z - 0.5f && colonistCheck.z <= this.transform.position.z + 0.5f)
                    {
                        mCC.currentColonistSelect = this.gameObject;
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }

        return false;
    }

    public void moveWorker(Vector3 hit)
    {
        //Vector3 moveTo = new Vector3(Mathf.FloorToInt(hit.x), Mathf.Round(hit.y), Mathf.FloorToInt(hit.z)) + new Vector3(0.5f, 0, 0.5f);
        Vector3 colonistBlock = new Vector3(Mathf.FloorToInt(gameObject.transform.position.x), Mathf.FloorToInt(gameObject.transform.position.y), Mathf.FloorToInt(gameObject.transform.position.z));
        Vector3 endBlock = new Vector3(Mathf.Round(hit.x - 0.25f), Mathf.Round(hit.y), Mathf.Round(hit.z - 0.25f));

        //Variables instead of parameters to allow possible usage of threads

        //pathfindStore pathfindStore = mCC.pathfind(endBlock - Vector3.up, colonistBlock - Vector3.up, new Vector3(0.5f, 1, 0.5f));
        //path = pathfindStore.path;
        //rotation = pathfindStore.rotation;

        pathfindStore pathfindInformation = new pathfindStore();
        pathfindInformation.endingPosition = endBlock - Vector3.up;
        pathfindInformation.currentPosition = colonistBlock - Vector3.up;
        pathfindInformation.offset = new Vector3(0.5f, 1, 0.5f);

        mCC.pathsToFind.Enqueue(pathfindInformation);
        mCC.colonistRef.Enqueue(this);
    }

    public void setPath(pathfindStore pathStore)
    {
        path = pathStore.path;
        rotation = pathStore.rotation;
    }

    void updateMovement()
    {
        Vector3 colonistPos = this.transform.position;

        //if (world.checkForVoxel(path.Peek() - offset))
        {
            if (colonistPos.y != path.Peek().y)
            {
                if (Physics.Raycast(colonistPos + Vector3.up * 2, Vector3.down, out hit))
                {
                    if (Mathf.Round(hit.point.y) != Mathf.FloorToInt(path.Peek().y))
                    {
                        this.gameObject.transform.rotation = changeRotate(rotation.Peek());
                        this.gameObject.transform.position = Vector3.MoveTowards(colonistPos, new Vector3(path.Peek().x, path.Peek().y + 1, path.Peek().z), colonistSpeed * Time.deltaTime);
                        if (anim.GetBool("isWalking") != true)
                        { anim.SetBool("isWalking", true); anim.SetBool("isIdle", false); }
                    }
                    else
                    {
                        this.gameObject.transform.position = Vector3.MoveTowards(colonistPos, path.Peek(), colonistSpeed * Time.deltaTime);
                    }
                }
            }
            else
            {
                this.gameObject.transform.rotation = changeRotate(rotation.Peek());
                this.gameObject.transform.position = Vector3.MoveTowards(colonistPos, path.Peek(), colonistSpeed * Time.deltaTime);
                if (anim.GetBool("isWalking") != true)
                { anim.SetBool("isWalking", true); anim.SetBool("isIdle", false); }
            }

            if (colonistPos == path.Peek())
            {
                path.Dequeue();
                changeRotate(rotation.Dequeue());
            }

            if (path.Count == 0)
            {
                anim.SetBool("isWalking", false); anim.SetBool("isIdle", true);
            }
        }

       // else
       // {
       //     Vector3 endPos = new Vector3();

       //    for (int i = 1; i == path.Count; i = 1)
       //         path.Dequeue();

       //     if (path.Count == 1)
       //         endPos = path.Dequeue();

       //     moveWorker(endPos);
       // }
    }

    Quaternion changeRotate(byte rotateID)
    {
        Quaternion rotate = Quaternion.Euler(0, 0, 0);

        switch (rotateID)
        {
            case 0:
                rotate = Quaternion.Euler(0, 0, 0);
                return rotate;

            case 1:
                rotate = Quaternion.Euler(0, 90, 0);
                return rotate;

            case 2:
                rotate = Quaternion.Euler(0, 180, 0);
                return rotate;

            case 3:
                rotate = Quaternion.Euler(0, 270, 0);
                return rotate;
        }

        return rotate;
    }

    public void randomIdleAnim()
    {
        int maxIdleAnimations = 3;
        float chancePercentage = 150 / Time.deltaTime;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idles.IdleNeutral"))
        {          
            if (Random.Range(0,chancePercentage) >= chancePercentage - 5)
            {
                //Debug.Log("Passed");
                anim.SetInteger("IdleVariation", Random.Range(1,maxIdleAnimations));
                //anim.SetInteger("IdleVariation", 0);
                StartCoroutine("idleBuffer");
            }
        }
    }

    public IEnumerator idleBuffer()
    {
        yield return new WaitForSeconds(0.5f);
        anim.SetInteger("IdleVariation", 0);
        //Debug.Log("Idle Buffer complete");
    }
}