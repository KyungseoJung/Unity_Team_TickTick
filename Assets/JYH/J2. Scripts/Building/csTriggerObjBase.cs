using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csTriggerObjBase : MonoBehaviour
{
    public Transform childPos;

    public csTriggerObjBase _TOB;

    public bool isUse = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && !isUse)
        {
            _TOB.isUse = true;
            isUse = true;

            _TOB.Invoke("EnableIsUse", 2f);
            Invoke("EnableIsUse", 2f);

            other.transform.position = childPos.position;
        }
    }

    void EnableIsUse()
    {
        isUse = false;
    }
}
