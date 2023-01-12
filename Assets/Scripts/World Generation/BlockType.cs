using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlockType
{
    public string name;
    public int byteID;
    public bool isSolid;

    [Header("Special Blocks: Is new block Special?")]
    public bool isFurniture = false;
    public bool isJobBlock = false;

    public GameObject furniture = null;
    public string jobName = null;

    [Header("Furniture size: is the furnitue greater than 1 block in size?")]
    public bool fill = true;
    public int xSize = 1;
    public int zSize = 1;
    public int ySize = 1;

    [Header("Texture Value from atlas")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int rightFaceTexture;
    public int leftFaceTexture;
    public int bottomFaceTexture;
    public int topFaceTexture;

    //Front, Top, Right, Bottom, Left, Back

    public int GetTextureId(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return frontFaceTexture;
            case 1:
                return topFaceTexture;
            case 2:
                return rightFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return backFaceTexture;
            default:
                Debug.Log("Error in GetTextureID, Invalid face index");
                return 0;
        }
    }
}