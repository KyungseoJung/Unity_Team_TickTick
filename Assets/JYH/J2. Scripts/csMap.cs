using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TeamInterface;


public class csMap : MonoBehaviour
{
    [Header("맵정보")]
    public MapDataClass mapData = new MapDataClass();

    [Header("블록 정보 3차원으로 저장")]
    public Block[,,] worldBlock = null;

    List<Vector3> childVector = new List<Vector3>();

    bool mapLoadOk = false;

    private void Awake()
    {
        StartCoroutine(AwakeInitMap());
    }

    IEnumerator AwakeInitMap()
    {
        worldBlock = new Block[mapData.widthX, mapData.height, mapData.widthZ];

        for (int x = 0; x < mapData.widthX; x++)
        {
            for (int z = 0; z < mapData.widthZ; z++)
            {
                float xb = (x + 0) / mapData.waveLength;
                float zb = (z + 0) / mapData.waveLength;
                int y = (int)((Mathf.PerlinNoise(xb, zb) * mapData.amplitude) * mapData.amplitude + mapData.groundHeightOffset);
                Vector3 pos = new Vector3(x, y, z);

                CreateBlockData(y, pos, true);

                if (UnityEngine.Random.Range(0, 100) < 40 && worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] != null && worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].top && !worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].type.Equals(Enum_CubeType.WATER))
                {
                    childVector.Add(pos);
                }

                while (y > 0)
                {
                    y--;
                    pos = new Vector3(x, y, z);
                    CreateBlockData(y, pos, false);
                }
            }
        }

        mapLoadOk = true;

        yield return null;
    }

    public List<Vector3> GetList()
    {
        if (mapLoadOk && childVector!=null)
        {
            return childVector;
        }
        else
        {
            return null;
        }
    }

    public Block[,,] GetBlock()
    {
        if (mapLoadOk && worldBlock!=null)
        {
            return worldBlock;
        }
        else
        {
            return null;
        }
    }

    void CreateBlockData(int y, Vector3 pos, bool v)
    {
        if (worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] != null)
        {
            return;
        }

        if (y > 28)
        {
            if (v)
            {
                GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[2], new Vector3(pos.x, pos.y * 0.5f, pos.z), Quaternion.identity);
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.GRASS, v, tmpObj);
                tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z]);

                //m_nodeArr[(int)pos.x, (int)pos.z] = tmpObj.GetComponent<Node>();
            }
            else
            {
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.GRASS, v, null, false);
            }
        }
        else if (y > 5)
        {
            if (v)
            {
                GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[3], new Vector3(pos.x, pos.y * 0.5f, pos.z), Quaternion.identity);
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.SOIL, v, tmpObj);
                tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z]);
                //m_nodeArr[(int)pos.x, (int)pos.z] = tmpObj.GetComponent<Node>();
            }
            else
            {
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.SOIL, v, null, false);
            }
        }
        else if (y >= 0)
        {
            if (v)
            {
                GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[1], new Vector3(pos.x, pos.y * 0.5f, pos.z), Quaternion.identity);
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.STON, v, tmpObj);
                tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z]);
                //m_nodeArr[(int)pos.x, (int)pos.z] = tmpObj.GetComponent<Node>();
            }
            else
            {
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.STON, v, null, false);
            }
        }

        if (y >= 23 && y < 27 && worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] != null && worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].vis)
        {
            worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].vis = false;
            worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].top = false;
            Destroy(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].obj);

            int tmpY = y;

            while (tmpY <= 27)
            {
                if (tmpY == 27)
                {
                    GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[5], new Vector3(pos.x, (27 * 0.5f), pos.z), Quaternion.identity);
                    worldBlock[(int)pos.x, 27, (int)pos.z] = new Block(Enum_CubeType.WATER, true, tmpObj, true);
                    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)pos.x, 27, (int)pos.z]);
                }
                else
                {
                    worldBlock[(int)pos.x, tmpY, (int)pos.z] = new Block(Enum_CubeType.WATER, false, null, false);
                }
                tmpY++;
            }


        }

        //if (y + 1 <= 27 && y + 1 >= 24)
        //{
        //    for (int i = y + 1; i <= 27; i++)
        //    {
        //        if (worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z] == null && i == 27)
        //        {
        //            GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[5], new Vector3(pos.x, (pos.y + (i - y)) * 0.5f, pos.z), Quaternion.identity);
        //            worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z] = new Block(Enum_CubeType.WATER, true, tmpObj, true);
        //            tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z]);
        //            //m_nodeArr[(int)pos.x, (int)pos.z] = tmpObj.GetComponent<Node>();
        //        }
        //        else if (worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z] == null)
        //        {
        //            worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z] = new Block(Enum_CubeType.WATER, false, null, false);
        //        }
        //        else if (worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z] != null && !worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z].type.Equals(Enum_CubeType.WATER))
        //        {
        //            Debug.Log("물 밑에 땅 제거");
        //            childVector.Remove(new Vector3(pos.x, pos.y + (i - y), pos.z));
        //            worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z].top = false;
        //            worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z].vis = false;
        //            Destroy(worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z].obj);
        //        }


        //    }
        //    //Debug.Log(1111);
        //}
    }



    //rpc 대응 함수
    public void CreateBlockChildRPCAction(Vector3 pos, Enum_CubeState tmpCS, int tmpNum)
    {
        worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].obj.GetComponent<csCube>().SetObj(tmpCS, tmpNum);
    }

    public void SetObjDMGRPCAction(Vector3 pos, float dmg, Enum_PlayerUseItemType ui)
    {
        worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].obj.GetComponent<csCube>().StartAction(dmg, ui);
    }

    public void RPCActionHOEAction(Vector3 blockPos)
    {
        worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].obj.GetComponent<csCube>().SetObj(Enum_CubeState.FIELD);
    }

    public void CreateCubeAction(Vector3 blockPos, Enum_CubeType type)
    {
        switch (type)
        {
            case Enum_CubeType.SOIL:
                {
                    GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[3], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.SOIL, true, tmpObj, true, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    //m_nodeArr[(int)blockPos.x, (int)blockPos.z] = tmpObj.GetComponent<Node>();
                }
                break;
            case Enum_CubeType.WATER:
                {
                    GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[5], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.WATER, true, tmpObj, true, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    // m_nodeArr[(int)blockPos.x, (int)blockPos.z] = tmpObj.GetComponent<Node>();

                    int tmpY = (int)blockPos.y;
                    while (tmpY > 0)
                    {
                        if (worldBlock[(int)blockPos.x, tmpY, (int)blockPos.z] == null)
                        {
                            worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.WATER, false, tmpObj, false, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);

                            tmpY--;
                            //Debug.Log(blockPos);
                        }
                        else if (worldBlock[(int)blockPos.x, tmpY, (int)blockPos.z] != null)
                        {
                            break;
                        }
                    }
                }
                break;
        }
    }

    public void ActionSHOVELRPCAction(Vector3 blockPos)
    {
       // Debug.Log("누군가 삽질함" + blockPos); // + csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].obj.name+"//"+ csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].type);
        Destroy(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].obj);
        worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = null;
    }

    public void DrawBlockAction(Vector3 blockPos)//블록 그리는 함수
    {
        if (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] == null)
        {
            //Debug.Log(1);
            return;
        }


        if (!worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].vis)
        {
            worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].vis = true;

            GameObject tmpObj = null;

            bool tmpTop = false;

            if (worldBlock[(int)blockPos.x, (int)blockPos.y + 1, (int)blockPos.z] == null)
            {
                tmpTop = true;
            }

            switch (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].type)
            {
                case Enum_CubeType.DARKSOIL:
                    tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[0], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.DARKSOIL, true, tmpObj, tmpTop, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    break;
                case Enum_CubeType.STON:
                    tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[1], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.STON, true, tmpObj, tmpTop, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    break;
                case Enum_CubeType.GRASS:
                    tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[2], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.GRASS, true, tmpObj, tmpTop, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    break;
                case Enum_CubeType.SOIL:
                    tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[3], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.SOIL, true, tmpObj, tmpTop, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    break;
                case Enum_CubeType.SEND:
                    tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[4], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.SEND, true, tmpObj, tmpTop, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    break;
                case Enum_CubeType.WATER:
                    tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[5], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.WATER, true, tmpObj, tmpTop, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    break;
            }
        }
    }

    public void DelChildObjRPCAction(Vector3 pos)
    {
        worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].obj.GetComponent<csCube>().DestroyChild();
    }
}
