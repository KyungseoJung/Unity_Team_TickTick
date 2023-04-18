using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TeamInterface;

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

    public void SetObj()
    {
        if (cubeState.Equals(Enum_CubeType.WATER))
        {
            return;
        }

        if (cubeInfo.obj.transform.position.y < 13.5f)
        {
            return;
        }
        else
        {
            StartCoroutine(InsObj((Enum_CubeState)Random.Range(0, 100)));
        }
    }
    public void SetObj(Enum_CubeState state)
    {
        StartCoroutine(CreateObj(state));
    }

    IEnumerator CreateObj(Enum_CubeState state)
    {
        if (childObj == null)
        {
            switch (state)
            {
                case Enum_CubeState.FIELD:
                    childObj = Instantiate(csLevelManager.Ins.field, new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), Quaternion.identity);
                    childObj.transform.SetParent(transform);
                    cubeInfo.haveChild = true;
                    break;
            }
        }
            yield return null;
    }

    IEnumerator InsObj(Enum_CubeState cs)
    {
        if (childObj == null)
        {
            switch (cs)
            {
                case Enum_CubeState.GRASS1:
                    childObj = Instantiate(csLevelManager.Ins.fieldObj[Random.Range(0, 3)], new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), Quaternion.identity);
                    childObj.transform.SetParent(transform);
                    cubeInfo.haveChild = true;
                    break;
                case Enum_CubeState.TREE1:
                    childObj = Instantiate(csLevelManager.Ins.fieldObj[Random.Range(15, 27)], new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), Quaternion.identity);
                    childObj.transform.SetParent(transform);
                    cubeInfo.haveChild = true;
                    break;
                case Enum_CubeState.TREE2:
                    childObj = Instantiate(csLevelManager.Ins.fieldObj[Random.Range(22, 27)], new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), Quaternion.identity);
                    childObj.transform.SetParent(transform);
                    cubeInfo.haveChild = true;
                    break;
                case Enum_CubeState.GRASS2:
                    childObj = Instantiate(csLevelManager.Ins.fieldObj[Random.Range(0, 15)], new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), Quaternion.identity);
                    childObj.transform.SetParent(transform);
                    cubeInfo.haveChild = true;
                    break;
                case Enum_CubeState.ROCK1:
                    childObj = Instantiate(csLevelManager.Ins.fieldObj[Random.Range(27, 35)], new Vector3(transform.position.x, transform.position.y + 0.35f, transform.position.z), Quaternion.identity);
                    childObj.transform.SetParent(transform);
                    cubeInfo.haveChild = true;
                    break;
            }
        }        

        if (childObj != null)
        {
            childObj.transform.SetParent(transform);
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
                tmp.SetHpDamaged(dmg, useItemType);
            }           
        }
    }

}
