using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csGenericSingleton<T> : MonoBehaviour where T: csGenericSingleton<T>
{
    private static T ins = null;
    public static T Ins
    {
        get
        {
            if (ins == null)
            {
                ins = GameObject.FindObjectOfType(typeof(T)) as T;

                if (ins == null)
                {
                    ins = new GameObject("Singleton_" + typeof(T).ToString(), typeof(T)).GetComponent<T>();

                    DontDestroyOnLoad(ins);
                }
            }

            return ins;
        }
    }

    public static T IsExist()
    {
        if (ins)
        {
            return ins;
        }

        return null;
    }

    protected virtual void Awake()
    {
        if (ins == null)
        {
            ins = this as T;
            //Debug.Log("ins virtual Awake");
            ins.Init();
        }
    }

    protected virtual void OnDestroy()
    {
        if (ins != null)
        {
            ins.Clear();
            ins = null;
        }
    }

    private void OnApplicationQuit()
    {
        ins = null;//가비지 콜랙터가 알아서 지워줄것
    }

    public virtual void Init() { }
    public virtual void Clear() { }
    public virtual void Test() { }
}
