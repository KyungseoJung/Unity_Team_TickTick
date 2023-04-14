using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csDayCtrl : MonoBehaviour
{
    [Header("[라이트 컬러 값]")]
    [SerializeField]
    public Light dayLight;

    [Header("[스카이돔 오프셋]")]
    [SerializeField]
    public Material skyOffSet;
    float skyOffSetVal = 0;

    [Header("[스카이돔 해=0, 달, 별]")]
    public GameObject[] skyObj= new GameObject[3];
    bool skyObjActive = true;

    private void Start()
    {
        skyObj[0].SetActive(true);
        for (int i=1;i< skyObj.Length; i++)
        {
            skyObj[i].SetActive(false);
        }

        //StartCoroutine(DayEffect());
    }

    public void NextTime()
    {
        StartCoroutine(DayEffect());
    }

    IEnumerator DayEffect()
    {
        //while (true)
        {
            //yield return new WaitForSeconds(0.2f);

            if (skyOffSetVal >= 1f)
            {
                skyOffSetVal = 0f;

                csLevelManager.Ins.NextDay();
            }

            //스카이박스 머테리얼 설정
            SetDaySkyOffSet(skyOffSetVal);


            //환경광 색갈 설정
            float tmpVal = 0f;

            if (skyOffSetVal<=0.5f) {
                tmpVal = 1f - skyOffSetVal;
                dayLight.color = new Color(tmpVal, tmpVal, tmpVal, 1f);//0f~1f : 0~255
            }
            else
            {
                dayLight.color = new Color(skyOffSetVal, skyOffSetVal, skyOffSetVal, 1f);//0f~1f : 0~255
            }


            //해 달 별 오브젝트 설정
            if (skyObjActive && (skyOffSetVal<0.3f || skyOffSetVal > 0.7f))
            {
                skyObjActive = false;
                skyObj[0].SetActive(true);
                skyObj[1].SetActive(false);
                skyObj[2].SetActive(false);
            }
            else if(!skyObjActive && (skyOffSetVal >= 0.3f && skyOffSetVal <= 0.7f))
            {
                skyObjActive = true;
                skyObj[0].SetActive(false);
                skyObj[1].SetActive(true);
                skyObj[2].SetActive(true);
            }

            skyOffSetVal += 0.1f;//틱당 시간흐름
        }

        yield return null;
    }

    ~csDayCtrl()
    {
        StopAllCoroutines();
    }

    public void SetDaySkyOffSet(float val)
    {
        //Debug.Log(val);
        skyOffSet.mainTextureOffset = new Vector2(val,0);
    }
}
