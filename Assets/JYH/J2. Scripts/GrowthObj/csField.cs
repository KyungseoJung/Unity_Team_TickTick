using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TeamInterface;
using JinscObjectBase;


public class csField : csObjectBase, IGrowth
{
    [SerializeField]
    bool haveGrowth;//흔들기 했을 때 뭐가 떨어지는앤가?

    [SerializeField]
    GameObject dropItem;//흔들기 했을 때 떨어지는 애

    [SerializeField]
    GameObject childObj;//성장단계에따라 오브젝트 껏다 켰다
    [SerializeField]
    GameObject fakeChildObj;

    [SerializeField]
    Enum_ObjectGrowthLevel growthLevel;//성장단계표현


    public override void Shake()//흔들기 당했을 때
    {
        if (haveGrowth && growthLevel.Equals(Enum_ObjectGrowthLevel.TWO))
        {
            StartCoroutine(DropItem());
        }

        base.Shake();
    }

    public override void DropItemFct()//채집 당했을 때
    {
        if (haveGrowth && growthLevel.Equals(Enum_ObjectGrowthLevel.TWO))
        {
            StartCoroutine(DropItem());

            childObj.SetActive(false);
            fakeChildObj.SetActive(false);
            growthLevel = Enum_ObjectGrowthLevel.ZERO;
        }

        //base.DropItemFct();
    }

    IEnumerator DropItem()
    {
        for (int i = 0; i < Random.Range(0, 2); i++)
        {
            GameObject tmp = Instantiate(dropItem, new Vector3(transform.position.x, transform.position.y + 0.7f, transform.position.z), Quaternion.identity);
            tmp.GetComponent<Rigidbody>().AddForce(Vector3.up * Time.deltaTime * 6000f);
            tmp.transform.SetParent(null);
        }

        yield return null;
    }

    public override void GrowthDay()//시간에 따른 흐름마다 일어나는 일
    {
        if (growthLevel.Equals(Enum_ObjectGrowthLevel.ZERO))
        {
            childObj.SetActive(true);
            fakeChildObj.SetActive(false);
            growthLevel = Enum_ObjectGrowthLevel.ONE;
        }
        else if (growthLevel.Equals(Enum_ObjectGrowthLevel.ONE))
        {
            if (haveGrowth && Random.Range(0, 5) == 0)
            {
                StartCoroutine(DropItem());
            }

            childObj.SetActive(true);
            fakeChildObj.SetActive(true);
            growthLevel = Enum_ObjectGrowthLevel.TWO;
        }
        else if (growthLevel.Equals(Enum_ObjectGrowthLevel.TWO))
        {
            if (haveGrowth && Random.Range(0, 5) == 0)
            {
                StartCoroutine(DropItem());

                childObj.SetActive(false);
                fakeChildObj.SetActive(false);
                growthLevel = Enum_ObjectGrowthLevel.ZERO;
            }           
        }
     }

}
