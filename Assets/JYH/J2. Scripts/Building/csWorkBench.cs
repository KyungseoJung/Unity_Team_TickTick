using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csWorkBench : MonoBehaviour
{
    public GameObject useEkey3DText;

    public bool isActive = false;

    private void Start()
    {
        useEkey3DText.SetActive(false);
    }

    public void ShowText()
    {
        if (!isActive)
        {
            useEkey3DText.SetActive(true);
        }
    }

    public void HideText()
    {
        if (isActive)
        {
            useEkey3DText.SetActive(false);
        }
    }
}
