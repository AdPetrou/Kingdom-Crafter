using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceManager : MonoBehaviour
{
    public GameObject workerInterface;
    public GameObject inventory;
    private CanvasGroup invCanvasGroup;

    private bool inUI;

    public bool InUI
    {
        get { return inUI; }

        set { inUI = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        invCanvasGroup = inventory.GetComponent<CanvasGroup>();
        workerInterface.SetActive(false);
        inUI = false;
        invCanvasGroup.alpha = 0;
        invCanvasGroup.blocksRaycasts = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("H"))
        {
            if (workerInterface.activeSelf == false)
            {
                disableAllUi();
                workerInterface.SetActive(true);
                inUI = true;
            }
            else
                disableAllUi();
        }

        if (Input.GetButtonDown("I"))
        {
            if (invCanvasGroup.alpha == 0)
            {
                disableAllUi();
                invCanvasGroup.alpha = 1;
                invCanvasGroup.blocksRaycasts = true;
                inUI = true;
            }
            else
                disableAllUi();
        }

        if (Input.GetButtonDown("TAB"))
        {
            disableAllUi();
        }

    }

    public void disableAllUi()
    {
        workerInterface.SetActive(false);
        invCanvasGroup.alpha = 0;
        invCanvasGroup.blocksRaycasts = false;
        inUI = false;
    }
}
