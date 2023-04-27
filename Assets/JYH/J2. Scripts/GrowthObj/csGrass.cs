using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TeamInterface;

using JinscObjectBase;
public class csGrass : csObjectBase, IGrowth
{
    [SerializeField]
    bool haveGrowth;//흔들기 했을 때 뭐가 떨어지는앤가?

    [SerializeField]
    GameObject dropItem;//흔들기 했을 때 떨어지는 애

    [SerializeField]
    Material[] changeMat;//성장단계에따라 다른 머티리얼 적용

    [SerializeField]
    Enum_ObjectGrowthLevel growthLevel;//성장단계표현

    [SerializeField]
    Renderer mesh;

    public PhotonView pVPG;

    public override void Awake()
    {
        base.Awake();
        pVPG = GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<PhotonView>();
    }

    public override void Start()
    {
        base.Start();
    }
    public override void Shake()//흔들기 당했을 때
    {
        if (haveGrowth && growthLevel.Equals(Enum_ObjectGrowthLevel.ONE))
        {
            StartCoroutine(DropItem());

            growthLevel = Enum_ObjectGrowthLevel.ZERO;
        }

        base.Shake();
    }

    public override void DropItemFct()//제거 당했을 때
    {
        if (haveGrowth && growthLevel.Equals(Enum_ObjectGrowthLevel.ONE))
        {
            StartCoroutine(DropItem());
        }

        base.DropItemFct();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void GrowthDay()//시간에 따른 흐름마다 일어나는 일
    {
        if (growthLevel.Equals(Enum_ObjectGrowthLevel.ZERO))
        {
            mesh.material = changeMat[1];
            growthLevel = Enum_ObjectGrowthLevel.ONE;
        }
        else if (growthLevel.Equals(Enum_ObjectGrowthLevel.ONE))
        {
            if (haveGrowth && Random.Range(0, 5) == 0)
            {
                StartCoroutine(DropItem());
            }
            growthLevel = Enum_ObjectGrowthLevel.ZERO;
        }
    }
    public override void GrowthRain()//비올때 일어나는 일
    {

    }

    IEnumerator DropItem()
    {
        mesh.material = changeMat[0];

        for (int i = 0; i < Random.Range(1, 3); i++)
        {
            if (!PhotonNetwork.isMasterClient)
            {
                pV.RPC("CreateGrassRPC", PhotonTargets.MasterClient, new Vector3(transform.position.x, transform.position.y + 0.7f, transform.position.z));
            }
            else
            {
                CreateGrassRPC(new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z));
            }
        }

        yield return null;
    }

    [PunRPC]
    public void CreateGrassRPC(Vector3 pos)
    {
        GameObject tmp = PhotonNetwork.InstantiateSceneObject(dropItem.name, pos, Quaternion.identity, 0, null);
        tmp.GetComponent<Rigidbody>().AddForce(Vector3.up * Time.deltaTime * 6000f);
        tmp.transform.SetParent(null);
    }
}
