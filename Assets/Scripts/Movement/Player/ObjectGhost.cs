using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGhost
{
    private BlockType blockType;

    private static GameObject gameObj;
    private static GameObject ingameGhost;
    public int CurrentRotation
    {
        get { return currentRotation; }
    }
    private static int currentRotation;

    private static Transform parentObj;

    public ObjectGhost()
    {
        currentRotation = 0;
    }

    public GameObject getgameObj() { return gameObj; }

    public GameObject getingameGhost() { return ingameGhost; }

    public void setObject(GameObject _gameObj) { gameObj = GameObject.Instantiate(_gameObj, parentObj); }

    public void setParentObject(Transform _parentObj) { parentObj = _parentObj; }

    public void setBlockType(BlockType _blockType) { blockType = _blockType; }

    public void clearRotation() { currentRotation = 0; }

    public void clearObject()
    {
        if (ingameGhost)
            clearGhost();

        GameObject.Destroy(gameObj);

        clearRotation();
    }

    public void clearGhost()
    {
        GameObject.Destroy(ingameGhost);
    }

    public void instantiateGhost()
    {
        ingameGhost = GameObject.Instantiate(gameObj, parentObj);
    }

    public bool isObject()
    {
        if (gameObj)
            return true;
        else
            return false;
    }

    public Material[] changeShader(Material[] mats, Material ghostMat, bool opaque)
    {
        Material[] newMats = new Material[mats.Length];

        for (int i = 0; i < mats.Length; i++)
        {
            newMats[i] = mats[i];

            if (opaque)
            {
                newMats[i].shader = Shader.Find("Universal Render Pipeline/Simple Lit");

                newMats[i] = mats[i];
                newMats[i].name = mats[i].name + " Solid";
            }
            else
            {
                newMats[i].shader = Shader.Find("Universal Render Pipeline/Simple Lit");
                newMats[i] = ghostMat;
                newMats[i].name = mats[i].name + " Alpha";
            }
        }

        return newMats;
    }

    public void updateGhost()
    {
        lock (ingameGhost)
        {
            clearGhost();
            instantiateGhost();
        }
    }

    public void rotateObject()
    {
        Transform gO = gameObj.transform;

        gO.localEulerAngles = new Vector3(gO.localEulerAngles.x, gO.localEulerAngles.y, 90);

        if (blockType.xSize > 1 || blockType.zSize > 1)
        {
            switch (currentRotation)
            {
                case 0:
                    gO.transform.position += VoxelData.z;
                    currentRotation = 1;
                    break;

                case 1:
                    gO.transform.position += VoxelData.x;
                    currentRotation = 2;
                    break;

                case 2:
                    gO.transform.position -= VoxelData.z;
                    currentRotation = 3;
                    break;

                case 3:
                    gO.transform.position -= VoxelData.x;
                    currentRotation = 0;
                    break;
            }
        }

        updateGhost();
    }
}
