using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int stage;   // 몇 번째 스테이지냐 - 스테이지에 따라 배경음악 다르게
    private OptionManager _oMgr;

    void Start()
    {
        _oMgr = GameObject.Find("OptionManager").GetComponent<OptionManager>();
        _oMgr.PlayBackground(stage);

        //플레이어 없어서 ui창 생성된 뒤 레벨매니저의 인벤토리 연결
        GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().tPlayer = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
    }
}
