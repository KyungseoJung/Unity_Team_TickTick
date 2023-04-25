using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//https://www.youtube.com/watch?v=-cKiC0huc_w&ab_channel=RamJack

[System.Serializable]
public class csLevelManager : csGenericSingleton<csLevelManager>
{ 
    [Header("블록")]
    public GameObject[] cube;    

    [Header("필드 오브젝트")]
    public GameObject[] fieldObj;
    public GameObject field;

    [Header("애너미 프리팹")]
    public GameObject[] enemyPrefaps;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}