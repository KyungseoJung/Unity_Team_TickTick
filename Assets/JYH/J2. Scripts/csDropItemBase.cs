using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using TeamInterface;

public class csDropItemBase : Item
{
    //public Enum_DropItemType dropItemType;

    bool isRoot =false;

    public bool isTemp = false;

    PhotonView pV;

    protected virtual void Awake()
    {
        if (!isTemp)
        {
            pV = GetComponent<PhotonView>();
        }
    }
    private void OnEnable()
    {
        Vector3 randomDir = Random.insideUnitSphere;
        if (!isTemp)
        {
            GetComponent<Rigidbody>().AddForce(randomDir * 1.5f);
        }
    }

    protected virtual void Start()
    {
        if (!isTemp)
        {
            Debug.Assert(pV);
        }
        isRoot = false;
    }

    public void OnCollisionStay(Collision col)
    {
        //Debug.Log(col.transform.tag);

        if(!isTemp && col.transform.tag == "Player" && !isRoot)
        {           
            //Debug.Log("타긴하니");
            //만약 인벤토리가 꽉차있지 않으면
            if (GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>().CanAddItem(ItemType,count))
            {
                GetComponent<Rigidbody>().isKinematic = true;
                isRoot = true;
                //사운드 재생
                GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().PlayEffectSoundPhoton(transform.position, 2);
                StartCoroutine(DelObj());
            }
        }
    }

    IEnumerator DelObj()
    {
        //플레이어가 없어서 임시로 메니저에서 처리함
        GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().tPlayer.CollectItem(ItemType,null,count);

        yield return new WaitForSeconds(0.1f);

        //gameObject.SetActive(false);

        if (!PhotonNetwork.isMasterClient)
        {
            pV.RPC("DestroyObjRPC", PhotonTargets.MasterClient, null);
        }
        else
        {
            DestroyObjRPC();
        }

        //Destroy(gameObject);
    }

    [PunRPC]
    public void DestroyObjRPC()
    {
        // PhotonView의 소유권을 변경합니다.
        pV.TransferOwnership(PhotonNetwork.masterClient);
        PhotonNetwork.Destroy(this.gameObject);
    }
}
