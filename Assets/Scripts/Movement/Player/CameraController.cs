using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector2[] posChecks = new Vector2[4] { new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, -1) };

    private float mouseX, mouseY;
    private int speed, rotateSpeed;
    private float zoom, zoomSpeed = 2f, zoomMin = -1f, zoomMax = -100f;
    //zoomMin is the furthest distance you can be, zoomMax is the closest distance you can be (to the pivot)

    public Transform pivot, followThis;
    public Transform highlightBlock, placeBlock;
    public InterfaceManager interfaceManager;

    private RaycastHit hit;
    public World world;
    public Camera mainCam;

    private bool breakOrPlace = false;

    public mainColonistControl mCC;
    public bool enableCamera;

    public ObjectGhost objGhost = new ObjectGhost();
    public Material ghostMat;

    // Start is called before the first frame update
    void Start()
    {
        zoom = -20f;
        followThis.transform.position = new Vector3((VoxelData.worldWidth * VoxelData.chunkWidth) / 2f, 400, (VoxelData.worldWidth * VoxelData.chunkWidth) / 2f);
        if (Physics.Raycast(followThis.position, -Vector3.up, out hit))
            followThis.position = new Vector3(followThis.position.x, hit.point.y + 202, followThis.position.z);
        pivot.position = new Vector3(followThis.position.x, followThis.position.y - 200, followThis.position.z) - Vector3.up;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (enableCamera)
        {
            rotateSpeed = world.settings.rotateSpeed;
            zoomSpeed = world.settings.scrollSpeed;
            speed = world.settings.camSpeed;

            this.transform.LookAt(pivot);
            basicMove();
            rotateCamera();

            if (!interfaceManager.InUI)
            {
                if (!Input.GetButton("Shift") && (Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetAxis("Mouse ScrollWheel") < 0))
                    zoomIn();

                if (Input.GetButton("Shift"))
                {
                    placeCursorBlock();
                    changeVoxels();

                    if (Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetAxis("Mouse ScrollWheel") < 0)
                    {
                        breakOrPlace = !breakOrPlace;
                    }
                }

                else
                {
                    highlightBlock.gameObject.SetActive(false);
                    placeBlock.gameObject.SetActive(false);
                }

                if (Input.GetKeyDown(KeyCode.R) && objGhost.isObject())
                {
                    objGhost.rotateObject();

                    MeshRenderer ghostRender = objGhost.getingameGhost().GetComponent<MeshRenderer>();
                    ghostRender.materials = objGhost.changeShader(ghostRender.materials, ghostMat, false);
                }

                //if (Input.GetKeyDown(KeyCode.P))
                //{
                //   revealPlaceBlock();
                //}
            }
        }
    }

    public void revealPlaceBlock()
    {
        MeshRenderer highlightBlockRenderer = placeBlock.GetComponentInChildren<MeshRenderer>();
        highlightBlockRenderer.materials = objGhost.changeShader(highlightBlockRenderer.materials, ghostMat, false);
    }

    private void placeCursorBlock()
    {
        Ray ray = mainCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z));
        Vector3 forward = new Vector3(Mathf.Round(mainCam.transform.forward.x), 0, Mathf.Round(mainCam.transform.forward.z));
        //Vector3 forwardZ = new Vector3(0, 0, Mathf.Round(mainCam.transform.forward.z));

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 blockHitForwardOrRightDown = new Vector3(Mathf.Round(hit.point.x - 0.5f), Mathf.Round(hit.point.y), Mathf.Round(hit.point.z - 0.5f)) + Vector3.down;
            Vector3 blockHitBackOrLeftDown = new Vector3(Mathf.Round(hit.point.x + 0.5f), Mathf.Round(hit.point.y), Mathf.Round(hit.point.z + 0.5f)) + Vector3.back + Vector3.left + Vector3.down;
            Vector3 blockhitForward = new Vector3(Mathf.CeilToInt(hit.point.x - 0.5f), Mathf.Round(hit.point.y), Mathf.CeilToInt(hit.point.z - 0.5f)) + forward;
            Vector3 blockhitBackward = new Vector3(Mathf.FloorToInt(hit.point.x - 0.5f), Mathf.Round(hit.point.y), Mathf.FloorToInt(hit.point.z - 0.5f)) + forward;

            if (breakOrPlace)
            {
                if (world.checkForVoxel(blockhitForward) && !world.checkForVoxel(blockhitForward - forward))
                    worldCheckForVoxelRay(blockhitForward, -forward);

                else if (world.checkForVoxel(blockhitBackward) && !world.checkForVoxel(blockhitBackward - forward))
                    worldCheckForVoxelRay(blockhitBackward, -forward);

                else if (world.checkForVoxel(blockHitForwardOrRightDown) && !world.checkForVoxel(blockHitForwardOrRightDown + VoxelData.y))
                    worldCheckForVoxelRay(blockHitForwardOrRightDown);

                else if (world.checkForVoxel(blockHitBackOrLeftDown) && !world.checkForVoxel(blockHitBackOrLeftDown + VoxelData.y))
                    worldCheckForVoxelRay(blockHitBackOrLeftDown);
            }

            else if (!breakOrPlace)
            {
                if (world.checkForVoxel(blockHitForwardOrRightDown))
                    worldCheckForVoxelRay(blockHitForwardOrRightDown);

                else if (world.checkForVoxel(blockHitBackOrLeftDown))
                    worldCheckForVoxelRay(blockHitBackOrLeftDown);
            }

            else
            {
                highlightBlock.gameObject.SetActive(false);
                placeBlock.gameObject.SetActive(false);
                objGhost.clearGhost();
            }
        }
    }

    private void worldCheckForVoxelRay(Vector3 blockHit, Vector3 placeBlockDir = new Vector3())
    {
        objGhost.clearGhost();

        if (placeBlockDir == new Vector3())
            placeBlockDir = Vector3.up;

        if (breakOrPlace == false)
        {
            highlightBlock.gameObject.SetActive(true);
            placeBlock.gameObject.SetActive(false);
        }

        else
        {
            placeBlock.gameObject.SetActive(true);
            highlightBlock.gameObject.SetActive(false);

            if (objGhost.isObject())
            {
                objGhost.instantiateGhost();

                MeshRenderer ghostRender = objGhost.getingameGhost().GetComponent<MeshRenderer>();
                ghostRender.materials = objGhost.changeShader(ghostRender.materials, ghostMat, false);

                MeshRenderer highlightBlockRenderer = placeBlock.GetComponentInChildren<MeshRenderer>();
                highlightBlockRenderer.materials = objGhost.changeShader(highlightBlockRenderer.materials, ghostMat, false);
            }
            else
            {
                objGhost.clearGhost();
                revealPlaceBlock();
            }
        }

        highlightBlock.position = blockHit;
        placeBlock.position = blockHit + placeBlockDir;

        return;
    }

    private void changeVoxels()
    {
        GameObject colonist = null;

        //Destroy block
        if (highlightBlock.gameObject.activeSelf && Input.GetMouseButtonDown(0))
        {
            Vector3 pos = highlightBlock.position;

            colonist = mCC.findClosestColonistToVector3(pos);
            colonist colMove = colonist.GetComponent<colonist>();
            //clearBlockData data = findClosestClearBlock(pos, colonist);
            //int index = data.index;
            //float yPos = data.yPos;

            //if (colMove.isPathEmpty())
            //    colMove.moveWorker(new Vector3(pos.x + posChecks[index].x, yPos, pos.z + posChecks[index].y));
            //else
            //    StartCoroutine(queuePath(new Vector3(pos.x + posChecks[index].x, yPos, pos.z + posChecks[index].y), colonist));

            colMove.setBlockToBreak(pos);

            //world.getChunkFromVector3(pos).editVoxel(pos, 0);
        }

        else if (placeBlock.gameObject.activeSelf && (Input.GetMouseButtonDown(0)))
        {
            inventory inv = FindObjectOfType<inventory>();
            Vector3 pos = placeBlock.position;

            if (objGhost.getingameGhost())
                world.getChunkFromVector3(pos).editVoxel(pos, inv.currentVoxelSelected, objGhost);
            else
                world.getChunkFromVector3(pos).editVoxel(pos, inv.currentVoxelSelected);
        }
    }

    IEnumerator queuePath(Vector3 pos, GameObject colonist)
    {
        colonist colMove = colonist.GetComponent<colonist>();

        if (!colMove.isPathEmpty())
        {
            yield return new WaitForSeconds(0.2f);
        }
        else if (colMove.isPathEmpty())
        {
            colMove.moveWorker(pos);
            yield return null;
        }
    }

    void basicMove()
    {
        Vector3 velocity = Vector3.zero;
        Vector3 pivotStart = pivot.position;

        if (Input.GetButton("W"))
        {
            followThis.position += followThis.forward * speed * Time.deltaTime;
            if (Physics.Raycast(followThis.position, -Vector3.up, out hit))
                followThis.position = new Vector3(followThis.position.x, hit.point.y + 202, followThis.position.z);
        }
        if (Input.GetButton("S"))
        {
            followThis.position -= followThis.forward * speed * Time.deltaTime;
            if (Physics.Raycast(followThis.position, -Vector3.up, out hit))
                followThis.position = new Vector3(followThis.position.x, hit.point.y + 202, followThis.position.z);
        }
        if (Input.GetButton("D"))
        {
            followThis.position += followThis.right * speed * Time.deltaTime;
            if (Physics.Raycast(followThis.position, -Vector3.up, out hit))
                followThis.position = new Vector3(followThis.position.x, hit.point.y + 202, followThis.position.z);
        }
        if (Input.GetButton("A"))
        {
            followThis.position -= followThis.right * speed * Time.deltaTime;
            if (Physics.Raycast(followThis.position, -Vector3.up, out hit))
                followThis.position = new Vector3(followThis.position.x, hit.point.y + 202, followThis.position.z);
        }
        pivot.position = new Vector3(followThis.position.x, Vector3.SmoothDamp(pivotStart, new Vector3(followThis.position.x, followThis.position.y - 200, followThis.position.z), ref velocity, 0.065f).y, followThis.position.z);
    }

    void rotateCamera()
    {
        if (Input.GetMouseButton(1))
        {
            mouseX += Input.GetAxis("Mouse X");
            mouseY -= Input.GetAxis("Mouse Y");
        }

        if (Input.GetAxis("Vertical") > 0 || Input.GetAxis("Vertical") < 0)
        {
            followThis.rotation = Quaternion.Euler(0, pivot.eulerAngles.y, 0);
        }

        mouseY = Mathf.Clamp(mouseY, 20f / rotateSpeed, 177f / rotateSpeed);

        pivot.localRotation = Quaternion.Euler(mouseY * rotateSpeed, mouseX * rotateSpeed, 0);
    }

    void zoomIn()
    {
        zoom += Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;

        if (zoom > zoomMin)
            zoom = zoomMin;
        if (zoom < zoomMax)
            zoom = zoomMax;

        this.transform.localPosition = new Vector3(0, 0, zoom);
    }
}
