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

    [Header("오디오클립")]
    public AudioClip delBlock;//0
    public AudioClip[] footStep;//1
    public AudioClip getItem;//2
    public AudioClip[] shake;//3
    public AudioClip[] axe;//4
    public AudioClip[] delStone;//5
    public AudioClip hoe;//6
    public AudioClip openUI;//7
    public AudioClip[] pickaxe;//8
    public AudioClip[] createbuild;//9
    public AudioClip delTree;//10


    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public void PlayAudioClip(Vector3 pos, int tpye)
    {
        switch (tpye)
        {
            case 0:
                OptionManager.Ins.PlayEffect(pos,delBlock);
                break;
            case 1:
                OptionManager.Ins.PlayEffect(pos, footStep[Random.Range(0, footStep.Length)]);
                break;
            case 2:
                OptionManager.Ins.PlayEffect(pos, getItem);
                break;
            case 3:
                OptionManager.Ins.PlayEffect(pos, shake[Random.Range(0, shake.Length)]);
                break;
            case 4:
                OptionManager.Ins.PlayEffect(pos, axe[Random.Range(0, axe.Length)]);
                break;
            case 5:
                OptionManager.Ins.PlayEffect(pos, delStone[Random.Range(0, delStone.Length)]);
                break;
            case 6:
                OptionManager.Ins.PlayEffect(pos, hoe);
                break;
            case 7:
                OptionManager.Ins.PlayEffect(pos, openUI);
                break;
            case 8:
                OptionManager.Ins.PlayEffect(pos, pickaxe[Random.Range(0, pickaxe.Length)]);
                break;
            case 9:
                OptionManager.Ins.PlayEffect(pos, createbuild[Random.Range(0, createbuild.Length)]);
                break;
            case 10:
                OptionManager.Ins.PlayEffect(pos, delTree);
                break;
            default:
                break;
        }
    }
}