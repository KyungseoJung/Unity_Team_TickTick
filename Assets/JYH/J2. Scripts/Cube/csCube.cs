using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TeamInterface;
using JinscObjectBase;

public class csCube : MonoBehaviour, ICubeInfo, IHighlighter
{
    [HideInInspector]
    public Block cubeInfo;

    Enum_CubeState cubeState;

    public GameObject highlighter;

    public GameObject childObj = null;

    public Block CubeInfo { get { return cubeInfo; } set { cubeInfo = value; } }
    public Enum_CubeState CubeState { get { return cubeState; } set { cubeState = value; } }

    private void Start()
    {
        highlighter.SetActive(false);
        cubeState = Enum_CubeState.NONE;
    }

    public void SetCube(Block cube)
    {
        cubeInfo = cube;
    }

    public void SetObj(Enum_CubeState state, int val=0, Enum_ObjectGrowthLevel gl = Enum_ObjectGrowthLevel.ZERO)
    {
        if (!cubeInfo.type.Equals(Enum_CubeType.WATER))
        {
            StartCoroutine(CreateObj(state, val, gl));
        }
    }

    IEnumerator CreateObj(Enum_CubeState state, int val, Enum_ObjectGrowthLevel gl)
    {
        if (childObj == null)
        {
            switch (state)
            {
                case Enum_CubeState.FIELD:
                    cubeState = Enum_CubeState.FIELD;
                    childObj = Instantiate(csLevelManager.Ins.field, new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), Quaternion.identity);
                    childObj.transform.SetParent(transform);
                    childObj.GetComponent<csObjectBase>().SetGrowthLevel(gl);
                    cubeInfo.haveChild = true;
                    break;
                case Enum_CubeState.GRASS1:
                    cubeState = Enum_CubeState.GRASS1;
                    childObj = Instantiate(csLevelManager.Ins.fieldObj[val], new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), Quaternion.identity);
                    childObj.transform.SetParent(transform);
                    childObj.GetComponent<csObjectBase>().SetGrowthLevel(gl);
                    cubeInfo.haveChild = true;
                    break;
                case Enum_CubeState.TREE1:
                    cubeState = Enum_CubeState.TREE1;
                    childObj = Instantiate(csLevelManager.Ins.fieldObj[val], new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), Quaternion.identity);
                    childObj.transform.SetParent(transform);
                    childObj.GetComponent<csObjectBase>().SetGrowthLevel(gl);
                    cubeInfo.haveChild = true;
                    break;
                case Enum_CubeState.TREE2:
                    cubeState = Enum_CubeState.TREE2;
                    childObj = Instantiate(csLevelManager.Ins.fieldObj[val], new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), Quaternion.identity);
                    childObj.transform.SetParent(transform);
                    childObj.GetComponent<csObjectBase>().SetGrowthLevel(gl);
                    cubeInfo.haveChild = true;
                    break;
                case Enum_CubeState.GRASS2:
                    cubeState = Enum_CubeState.GRASS2;
                    childObj = Instantiate(csLevelManager.Ins.fieldObj[val], new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), Quaternion.identity);
                    childObj.transform.SetParent(transform);
                    childObj.GetComponent<csObjectBase>().SetGrowthLevel(gl);
                    cubeInfo.haveChild = true;
                    break;
                case Enum_CubeState.ROCK1:
                    cubeState = Enum_CubeState.ROCK1;
                    childObj = Instantiate(csLevelManager.Ins.fieldObj[val], new Vector3(transform.position.x, transform.position.y + 0.35f, transform.position.z), Quaternion.identity);
                    childObj.transform.SetParent(transform);
                    childObj.GetComponent<csObjectBase>().SetGrowthLevel(gl);
                    cubeInfo.haveChild = true;
                    break;
            }
        }
        yield return null;
    }

    public void OnHighlighter()
    {
        if (cubeState.Equals(Enum_CubeType.WATER))
        {
            return;
        }

        if (cubeInfo.top)
        {
            highlighter.SetActive(true);
        }
    }

    public void OffHighlighter()
    {
        if (cubeState.Equals(Enum_CubeType.WATER))
        {
            return;
        }

        highlighter.SetActive(false);
    }

    public void StartAction(float dmg,Enum_PlayerUseItemType useItemType)
    {
        if (cubeState.Equals(Enum_CubeType.WATER))
        {
            return;
        }

        if (childObj != null)
        {
            //Debug.Log("dmg");
            IObjectStatus tmp = childObj.GetComponent<IObjectStatus>();

            if (tmp!= null)
            {
                //GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetObjDMG(dmg, tmp, useItemType);
                //Debug.Log("dmg2");
                tmp.SetHpDamaged(dmg, useItemType);
            }           
        }
    }

    public Vector3 GetCubePos()
    {
        return new Vector3(transform.position.x, transform.position.y * 2, transform.position.z);
    }

    public void DelayDestroy()
    {
        Invoke("DestroyObj", 0.3f);
    }

    public void DestroyObj()
    {
        Destroy(gameObject);
    }

}
