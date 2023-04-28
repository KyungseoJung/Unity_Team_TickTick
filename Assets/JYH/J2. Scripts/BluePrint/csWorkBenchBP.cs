using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csWorkBenchBP : csPreViewBase
{
    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void ChangeMat(bool canBuild)
    {
        if (canBuild)
        {
            //preViewObj.GetComponent<MeshRenderer>().material = preViewGreen;

            MeshRenderer[] childMat = preViewObj.GetComponentsInChildren<MeshRenderer>();

            for (int i = 0; i < childMat.Length; i++)
            {
                childMat[i].material = preViewGreen;
            }
        }
        else
        {
            //preViewObj.GetComponent<MeshRenderer>().material = preViewRed;

            MeshRenderer[] childMat = preViewObj.GetComponentsInChildren<MeshRenderer>();

            for (int i = 0; i < childMat.Length; i++)
            {
                childMat[i].material = preViewRed;
            }
        }
    }
}
