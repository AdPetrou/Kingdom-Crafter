using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure 
{
    public static Queue<VoxelMod> generateMajorFlora(int index, Vector3 pos, int minHeight, int maxHeight)
    {
        switch (index)
        {
            case 0:
                return makeTree(pos, minHeight, maxHeight);
        }

        return new Queue<VoxelMod>();
    }

    public static Queue<VoxelMod> makeTree(Vector3 pos, int minTrunkHeight, int maxTrunkHeight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();

        int height = (int)(maxTrunkHeight * Utils.Get2DPerlin(new Vector2(pos.x, pos.z), 250f, 3f));

        if (height < minTrunkHeight)
            height = minTrunkHeight;

        System.Random r = new System.Random();
        int bottomRange = r.Next(-3,-1);
        int topRange = (-bottomRange) + 1;

        for (int y = 0; y < r.Next(4, 8); y++)
        {
            //Debug.Log("Y Pass");
            int heightOffset = Mathf.FloorToInt(y / 1.5f);

            for (int x = bottomRange + heightOffset; x < topRange - heightOffset; x++)
            {
                for (int z = bottomRange + heightOffset; z < topRange - heightOffset; z++)
                {
                    if (r.Next(0, 101) > 97)
                        queue.Enqueue(new VoxelMod(new Vector3(pos.x + x, pos.y + y + (height - 2), pos.z + z + r.Next(-1, 2)), 8));
                    else if (r.Next(0, 101) > 94)
                        queue.Enqueue(new VoxelMod(new Vector3(pos.x + x + r.Next(-1, 2), pos.y + y + (height - 2), pos.z + z), 8));
                    else
                        queue.Enqueue(new VoxelMod(new Vector3(pos.x + x, pos.y + y + (height - 2), pos.z + z), 8));
                }
            }
        }

        for (int i = 1; i < height; i++)
        {
            queue.Enqueue(new VoxelMod(new Vector3(pos.x, pos.y + i, pos.z), 9));
        }

        return queue;
    }
}
