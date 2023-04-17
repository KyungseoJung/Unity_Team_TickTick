using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using TeamInterface;

public class csDropItemBase : Item
{
    //public Enum_DropItemType dropItemType;

    bool isRoot =false;

    private void Start()
    {
        isRoot = false;
    }

    public void OnCollisionStay(Collision col)
    {
        //Debug.Log(col.transform.tag);

        if(col.transform.tag == "Player" && !isRoot)
        {
            isRoot = true; 
            //Debug.Log("타긴하니");

            //사운드 재생?           

            StartCoroutine(DelObj());
        }
    }

    IEnumerator DelObj()
    {
        //플레이어가 없어서 임시로 레벨메니저에서 처리함
        csLevelManager.Ins.tPlayer.CollectItem(ItemType);

        yield return new WaitForSeconds(0.1f);

        //gameObject.SetActive(false);

        Destroy(gameObject);

    }
}
