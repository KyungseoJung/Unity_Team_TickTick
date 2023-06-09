﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TeamInterface;

public class csPreViewBase : MonoBehaviour, IPreViewBase
{
    [SerializeField]
    bool canBuild;
    [SerializeField]
    bool groundCheck;


    public bool CanBuild { get { return canBuild; } set { canBuild = value; } }
    bool showPreViewCheck;
    public bool ShowPreViewCheck { get { return showPreViewCheck; } set { showPreViewCheck = value; } }

    [SerializeField]
    int sizeX, sizeZ;
    public int SizeX { get { return sizeX; } set { sizeX = value; } }
    public int SizeZ { get { return sizeZ; } set { sizeZ = value; } }

    [SerializeField]
    float sizeY;

    [SerializeField]
    Enum_PreViewType preViewType;
    public Enum_PreViewType PreViewType { get { return preViewType; } set { preViewType = value; } }

    [SerializeField]
    protected GameObject preViewObj;
    [SerializeField]
    protected GameObject buildObj;
    public GameObject PreViewObj { get { return preViewObj; } set { preViewObj = value; } }
    public GameObject BuildObj { get { return buildObj; } set { buildObj = value; } }

    [SerializeField]
    protected Material preViewGreen;
    [SerializeField]
    protected Material preViewRed;
    public Material PreViewGreen { get { return preViewGreen; } set { preViewGreen = value; } }
    public Material PreViewRed { get { return preViewRed; } set { preViewRed = value; } }

    Vector3 targetPos;
    public Vector3 TargetPos { get { return targetPos; } set { targetPos = value; } }


    LayerMask layer;
    float yVal = 0;

    bool isActive;

    public void HiedPreView()
    {
        showPreViewCheck = false;
        preViewObj.SetActive(false);
    }

    public void ShowPreView()
    {
        preViewObj.SetActive(true);
    }

    public void ShowPreView(Vector3 pos, bool groundCheck)
    {
        this.groundCheck = groundCheck;

        
        switch (preViewType)
        {
            case Enum_PreViewType.FIRE:
                yVal = 0.6f;
                break;
            case Enum_PreViewType.TENT:
                yVal =0.3f;
                break;
            case Enum_PreViewType.WORKBENCH:
                yVal = 0.3f;
                break;
            case Enum_PreViewType.HOUSE_CHAIR:
                yVal = 0.3f;
                break;
            case Enum_PreViewType.HOUSE_TABLE:
                yVal = 0.3f;
                break;
        }
        targetPos = new Vector3(pos.x, pos.y + yVal, pos.z);
        showPreViewCheck = true;
        //preViewObj.SetActive(true);
    }

    public void CreateBuilding()
    {
        if (isActive)
        {
            StartCoroutine(Create());
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
    public virtual void Update()
    {
        Collider[] colHit = Physics.OverlapBox(new Vector3(transform.position.x, transform.position.y+(sizeY/2), transform.position.z), this.transform.localScale / 2, Quaternion.identity, layer);

        if (colHit.Length != 0 || !groundCheck)
        {
            //Debug.Log(CanBuild+"캔빌드"+ colHit.Length+ groundCheck);
            CanBuild = false;
        }
        else
        {
            CanBuild = true;
        }
        //Debug.Log(CanBuild);

        if (showPreViewCheck)
        {
            ChangeMat(canBuild);

            this.transform.position = targetPos;
        }
    }

    public virtual void ChangeMat(bool canBuild)
    {
        if (canBuild)
        {
            preViewObj.GetComponent<MeshRenderer>().material = preViewGreen;
        }
        else
        {
            preViewObj.GetComponent<MeshRenderer>().material = preViewRed;
        }
    }

    IEnumerator Create()
    {
        if (canBuild)
        {
            //Instantiate(buildObj, transform).transform.SetParent(null);
            GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().CreateBluePrint(buildObj.name, transform.position);
        }

        HiedPreView();

        yield return null;
    }

    public virtual void Start()
    {
        layer = 1 << LayerMask.NameToLayer("PreViewCheck");
        preViewObj.SetActive(false);
    }

    private void OnEnable()
    {
        isActive = true;
    }

    private void OnDisable()
    {
        isActive = false;
    }
}
