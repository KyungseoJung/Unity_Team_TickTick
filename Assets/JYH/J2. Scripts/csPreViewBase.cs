using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TeamInterface;

public class csPreViewBase : MonoBehaviour, IPreViewBase
{
    bool canBuild;
    public bool CanBuild { get { return canBuild; } set { canBuild = value; } }
    bool showPreViewCheck;
    public bool ShowPreViewCheck { get { return showPreViewCheck; } set { showPreViewCheck = value; } }

    [SerializeField]
    int sizeX, sizeZ;
    public int SizeX { get { return sizeX; } set { sizeX = value; } }
    public int SizeZ { get { return sizeZ; } set { sizeZ = value; } }

    [SerializeField]
    Enum_PreViewType preViewType;
    public Enum_PreViewType PreViewType { get { return preViewType; } set { preViewType = value; } }

    [SerializeField]
    GameObject preViewObj;
    [SerializeField]
    GameObject buildObj;
    public GameObject PreViewObj { get { return preViewObj; } set { preViewObj = value; } }
    public GameObject BuildObj { get { return buildObj; } set { buildObj = value; } }

    [SerializeField]
    Material preViewGreen;
    [SerializeField]
    Material preViewRed;
    public Material PreViewGreen { get { return preViewGreen; } set { preViewGreen = value; } }
    public Material PreViewRed { get { return preViewRed; } set { preViewRed = value; } }

    Vector3 targetPos;
    public Vector3 TargetPos { get { return targetPos; } set { targetPos = value; } }


    LayerMask layer;
    bool groundCheck;

    public void HiedPreView()
    {
        showPreViewCheck = false;
        preViewObj.SetActive(false);
    }

    public void ShowPreView(Vector3 pos, bool groundCheck)
    {
        this.groundCheck = groundCheck;

        float yVal=0;
        switch (preViewType)
        {
            case Enum_PreViewType.FIRE:
                yVal = 0.7f;
                break;
            case Enum_PreViewType.TENT:
                yVal = 2.7f;
                break;
        }
        targetPos = new Vector3(pos.x, pos.y + yVal, pos.z);
        showPreViewCheck = true;
        preViewObj.SetActive(true);
    }

    public void CreateBuilding()
    {
        StartCoroutine(Create());
    }
    public virtual void Update()
    {
        Collider[] colHit = Physics.OverlapBox(this.transform.position, this.transform.localScale / 2, Quaternion.identity, layer);

        if (colHit.Length != 0 || !groundCheck)
        {
            CanBuild = false;
        }
        else
        {
            CanBuild = true;
        }
        //Debug.Log(CanBuild);

        if (showPreViewCheck)
        {
            if (canBuild)
            {
                preViewObj.GetComponent<MeshRenderer>().material = preViewGreen;
            }
            else
            {
                preViewObj.GetComponent<MeshRenderer>().material = preViewRed;
            }
            this.transform.position = targetPos;
        }
    }

    IEnumerator Create()
    {
        if (canBuild)
        {
            Instantiate(buildObj, transform).transform.SetParent(null);

            
           
        }

        HiedPreView();

        yield return null;
    }

    public virtual void Start()
    {
        layer = 1 << LayerMask.NameToLayer("PreViewCheck");
        preViewObj.SetActive(false);
    }
}
