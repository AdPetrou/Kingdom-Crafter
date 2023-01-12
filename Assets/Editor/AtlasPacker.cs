using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AtlasPacker : EditorWindow
{
    int blockSize = 512; //block size in pixels
    int atlasSizeInBlocks = 16;
    int atlasSize;

    Object[] rawTextures;
    List<Texture2D> sortedTextures = new List<Texture2D>();
    Texture2D atlas;

    [MenuItem("Kingdom Crafter/Atlas Packer")]

    public static void showWindow()
    {
        EditorWindow.GetWindow(typeof(AtlasPacker));
    }

    private void OnGUI()
    {
        atlasSize = blockSize * atlasSizeInBlocks;
        rawTextures = new Object[atlasSize];

        GUILayout.Label("Kingdom Crafter texture atlas packer", EditorStyles.boldLabel);

        blockSize = EditorGUILayout.IntField("Block Size", blockSize);
        atlasSizeInBlocks = EditorGUILayout.IntField("Atlas Size in Blocks", atlasSizeInBlocks);      

        if(GUILayout.Button("Load Textures"))
        {
            loadTextures();
            packAtlas();
        }

        if(GUILayout.Button("Clear Textures"))
        {
            atlas = new Texture2D(atlasSize, atlasSize);
            Debug.Log("Atlas Packer: Textures Cleared");
        }

        if(GUILayout.Button("Save Textures"))
        {
            byte[] bytes = atlas.EncodeToPNG();

            try
            {
                File.WriteAllBytes(Application.dataPath + "/Packed_Atlas.png", bytes);
            }
            catch
            {
                Debug.LogError("Atlas Packer: Couldn't save atlas to file");
            }
        }

        GUILayout.Label(atlas);
    }

    private void loadTextures()
    {
        sortedTextures.Clear();
        rawTextures = Resources.LoadAll("AtlasPacker", typeof(Texture2D));

        int index = 0;
        foreach(Object tex in rawTextures)
        {
            Texture2D t = (Texture2D)tex;

            if (t.width == blockSize && t.height == blockSize)
            {
                sortedTextures.Add(t);
            }
            else
                Debug.Log("Asset Packer: " + tex.name + " incorrect size. Texture not loaded.");

            index++;
        }

        Debug.Log("Atlas Packer: " + sortedTextures.Count + " successfully loaded");
    }

    private void packAtlas()
    {
        atlas = new Texture2D(atlasSize, atlasSize);
        Color[] pixels = new Color[atlasSize * atlasSize];

        for(int x = 0; x < atlasSize; x++)
        {
            for (int y = 0; y < atlasSize; y++)
            {
                //Get Current block we're looking for
                int currentBlockX = x / blockSize;
                int currentBlockY = y / blockSize;

                int index = currentBlockY * atlasSizeInBlocks + currentBlockX;

                //Get the Pixel in current block
                int currentPixelX = x - (currentBlockX * blockSize);
                int currentPixelY = y - (currentBlockY * blockSize);

                if (index < sortedTextures.Count)
                    pixels[(atlasSize - y - 1) * atlasSize + x] = sortedTextures[index].GetPixel(x, blockSize - y - 1);
                else
                    pixels[(atlasSize - y - 1) * atlasSize + x] = new Color(0f, 0f, 0f, 0f);
            }
        }

        atlas.SetPixels(pixels);
        atlas.Apply();
    }
}