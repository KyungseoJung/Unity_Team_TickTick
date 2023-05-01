using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csCubeTopCheck : MonoBehaviour
{
    public csCube cube;

    bool check = false;

    private void Update()
    {
        if (!check)
        {
            if (cube.childObj != null)
            {
                check = true;
            }
        }
        else
        {
            if (cube.childObj == null)
            {
                check = false;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(!check && other.tag == "Building")
        {
            Debug.Log("땅위에 건물있음");

            cube.childObj = other.gameObject;
            cube.cubeInfo.haveChild = true;
            //GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().m_nodeArr[(int)transform.position.x, (int)transform.position.z].m_nodeType = NodeType.Obstacle;
            transform.parent.GetComponent<Node>().m_nodeType = NodeType.Obstacle;
        }
    }
}
