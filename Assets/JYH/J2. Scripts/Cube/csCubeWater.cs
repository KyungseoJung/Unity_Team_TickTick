using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csCubeWater : csCube
{
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Item"))
        {
            collision.gameObject.GetComponent<Collider>().enabled = false;
            //other.GetComponent<Rigidbody>().isKinematic = true;
        }
    }
}
