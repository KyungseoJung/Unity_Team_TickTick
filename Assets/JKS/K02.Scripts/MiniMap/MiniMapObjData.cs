using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapObjData : MonoBehaviour //#11-3 용훈님 추가 - 맵 데이터 삭제
{
    int x=0;
    int z=0;

    public int GetX { get { return x; }  }
    public int GetZ { get { return z; }  }

    public void SetData(int x, int z)
    {
        this.x = x;
        this.z = z;
    }
}
