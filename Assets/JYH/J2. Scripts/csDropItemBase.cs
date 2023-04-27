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

    private void Awake()
    {
        if (!isTemp)
        {
            pV = GetComponent<PhotonView>();
        }
    }

    private void Start()
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
            isRoot = true; 
            //Debug.Log("타긴하니");

            //사운드 재생?           

            StartCoroutine(DelObj());
        }
    }

    IEnumerator DelObj()
    {
        //플레이어가 없어서 임시로 메니저에서 처리함
        GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().tPlayer.CollectItem(ItemType);

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
