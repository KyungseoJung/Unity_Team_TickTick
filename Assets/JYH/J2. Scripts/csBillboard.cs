using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csBillboard : MonoBehaviour
{
    Transform mainCamTrans;
    Transform myTrans;

    private void Awake()
    {
        myTrans = GetComponent<Transform>();
        mainCamTrans = Camera.main.transform;
    }

    private void Update()
    {
        myTrans.LookAt(mainCamTrans);
    }
}
