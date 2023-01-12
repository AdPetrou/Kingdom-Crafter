using UnityEngine;
using UnityEngine.UI;

public class inventory : MonoBehaviour
{
    public int currentVoxelSelected;
    public GameObject buttonParent;
    public GameObject buttonTemplate;

    public CameraController camControl;
    public World world;
    public int[] blockType;
    public inventorySlot[] inventorySlots;
    public GameObject[] buttons;
    public bool invOpen = false;

    // Start is called before the first frame update
    void Start()
    {
        generateInventory();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("I") && invOpen == false)
            invOpen = true;
        else if (Input.GetButtonDown("I") && invOpen == true)
            invOpen = false;
    }

    private void generateInventory()
    {
        int count = 0;
        for (int i = 0; i < world.blockType.Length; i++)
            if (world.blockType[i].isSolid)
                count++;

        blockType = new int[count];
        inventorySlots = new inventorySlot[blockType.Length];
        buttons = new GameObject[blockType.Length];

        int blockTypeCount = 0;

        for (int i = 0; i < world.blockType.Length; i++)
        {
            if (world.blockType[i].isSolid)
            {
                blockType[blockTypeCount] = i;
                inventorySlots[blockTypeCount] = new inventorySlot(world.blockType[i]);
                blockTypeCount++;
            }
        }

        for (int i = 0; i < blockType.Length; i++)
        {
            GameObject button = Instantiate(buttonTemplate);
            button.transform.SetParent(buttonParent.transform);

            if ((Mathf.FloorToInt(i / 7) * 4) > 0)
                button.transform.localPosition = new Vector3((-300) + ((i - (7 * Mathf.FloorToInt(i / 7))) * 100), (buttonParent.GetComponent<RectTransform>().rect.yMax - 100) - (46.875f * (Mathf.FloorToInt(i / 7) * 4)), button.transform.position.z);
            else
                button.transform.localPosition = new Vector3((-300) + (i * 100), buttonParent.GetComponent<RectTransform>().rect.yMax - 100, button.transform.position.z);

            button.name = inventorySlots[i].getBlockName();           
            button.transform.localScale = new Vector3(0.5f, 4.5f, 0);
            buttons[i] = button;            
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            changeAmountInText(inventorySlots[i]);
            Button btn = buttons[i].GetComponentInChildren<Button>();
            int i2 = i;
            btn.onClick.AddListener(() => { buttonClick(buttons[i2]); });
        }
    }

    public void changeAmountInText(inventorySlot invSlot)
    {
        for(int i = 0; i < buttons.Length; i++)
        {
            if(invSlot.getByteID() == inventorySlots[i].getByteID())
            {
                buttons[i].GetComponentInChildren<Text>().text = invSlot.getBlockName() + " " + invSlot.getAmount().ToString();
            }
        }
    }

    public int getIndexOfButton(GameObject button)
    {
        int i = -1;
        for(int u = 0; u < buttons.Length; u++)
        {
            if(buttons[u].name == button.name)
            {
                i = u;
                break;
            }
        }
        return i;
    }

    void buttonClick(GameObject button)
    {
        int index = getIndexOfButton(button);
        currentVoxelSelected = inventorySlots[index].getByteID();

        BlockType currentVoxel = inventorySlots[index].getBlockType();

        if (currentVoxel.isFurniture && currentVoxel.isSolid)
        {
            if (camControl.objGhost.isObject() && (camControl.objGhost.getgameObj().gameObject != currentVoxel.furniture))
                camControl.objGhost.clearObject();

            camControl.objGhost.setParentObject(camControl.placeBlock);
            camControl.objGhost.setObject(currentVoxel.furniture);
            camControl.objGhost.setBlockType(currentVoxel);
            MeshRenderer camControlRenderer = camControl.objGhost.getgameObj().GetComponent<MeshRenderer>();
            camControl.objGhost.changeShader(camControlRenderer.materials, camControl.ghostMat, false);
        }
        else
        {
            camControl.objGhost.clearObject();
            camControl.revealPlaceBlock();
        }
    }
}
