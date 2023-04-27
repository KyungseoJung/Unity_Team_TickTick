using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csFireBP : csPreViewBase
{
    //private void OnTriggerEnter(Collider other)
    //{
    //    if(other.tag == "Block" || other.tag == "Rock" || other.tag == "Grass" || other.tag == "Tree")
    //    {
    //        CanBuild = false;
    //    }
    //    else
    //    {
    //        CanBuild = true;
    //    }
    //}

   

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
