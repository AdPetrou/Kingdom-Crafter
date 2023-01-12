using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inventorySlot
{
    private BlockType blockType;
    private int amount;
    private Vector2 pos;
    
    public inventorySlot(BlockType block)
    {
        blockType = block;
        amount = 0;
    }

    public string getBlockName()
    {
        return blockType.name;
    }

    public int getByteID()
    {
        return blockType.byteID;
    }

    public int getAmount()
    {
        return amount;
    }

    public void incrementAmount()
    {
        amount++;
    }

    public BlockType getBlockType()
    {
        return blockType;
    }
}
