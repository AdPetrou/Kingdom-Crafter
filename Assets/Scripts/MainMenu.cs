using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class MainMenu : MonoBehaviour
{
    public GameObject[] sideMenu;

    [Header("New World Settings")]
    public Slider WWslider;
    public TMP_Text WWsliderText;

    [Header("Settings Settings")]
    public Slider VDslider;
    public TMP_Text VDsliderText;

    public void Start()
    {
        sideMenuDisable();
    }

    public void newGame()
    {
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void quitGame()
    {
        Application.Quit();
    }

    public void enableSideMenu(int index)
    {
        sideMenuDisable();
        sideMenu[index].SetActive(true);
    }

    private void sideMenuDisable()
    {
        for(int i = 0; i < sideMenu.Length; i++)
        {
            sideMenu[i].SetActive(false);
        }
    }

    public void setSliderText(int index)
    {
        if (index == 1)
            WWsliderText.text = (WWslider.value * 100).ToString();
        if (index == 2)
            VDsliderText.text = (VDslider.value).ToString();
    }

    public void setWorldWidth()
    {
        VoxelData.setWorldWidth(Mathf.RoundToInt(WWslider.value * 100));
    }
}
