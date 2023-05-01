using UnityEngine;
using UnityEngine.UI;

public enum NodeType 
{
    None, //특정 노드가 어떤 유형도 아닐 때
    Water, //노드가 물일 때
    Obstacle //노드가 장애물로 표시되는 경우
}

public class Node : MonoBehaviour
{
    public NodeType m_nodeType = NodeType.None; //노드의 타입을 저장
    public int m_fCost = 0; //노드의 Fcost 저장
    public int m_hCost = 0; //노드의 Hcost 저장
    public int m_gCost = 0; //노드의 Gcost 저장
    public int m_row = 0; //노드가 위치한 행
    public int m_col = 0; //노드가 위치한 열
    public float y = 0; //노드의 y축 좌표
    private Node m_parent; //부모 노드

    private void Start()
    {
        csCube tmpCube = GetComponent<csCube>(); //현재 노드에 달려있는 큐브 컴포넌트 가져오기

        if (tmpCube.childObj != null) //만약에 노드가 장애물이면
        {
            m_nodeType = NodeType.Obstacle; //노드 타입을 장애물로 지정
        }
        else if(tmpCube.cubeInfo.type == TeamInterface.Enum_CubeType.WATER) //만약 노드가 물이면
        {
            Debug.Log("이것은 물이다");
            m_nodeType = NodeType.Water; //노드 타입을 물로 지정
        }
        else //그 외라면
        {
            m_nodeType = NodeType.None; //노드 타입을 없음으로 지정
        }

        //현재 노드의 위치에 따라 행과 열 변수 저장
        m_row = (int)transform.position.x;
        m_col = (int)transform.position.z;

        y = transform.position.y; //현재 노드의 y축 좌표 저장

        //현재 노드를 노드 배열에 저장
        GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().m_nodeArr[(int)transform.position.x, (int)transform.position.z] = this;
        
        //로그 출력
        Debug.Log(GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().m_nodeArr[(int)transform.position.x, (int)transform.position.z]);
    }

    public int Row  //현재 노드의 행값을 반환
    {
        get { return m_row; } 
    }

    public int Col //현재 노드의 열값을 반환
    {
        get { return m_col; }
    }

    public Node Parent //현재 노드의 부모 노드를 반환~
    {
        get { return m_parent; }
    }

    public Vector3 POS //이 노드의 위치를 가져오거나 설정
    {
        set
        {
            transform.position = value; //위치를 설정
        }
        get { return transform.position; } //위치를 가져와서 노드의 위치를 반환
    }

    public NodeType NType //노드의 노드 타입을 가져옴
    {
        get { return m_nodeType; } //노트 타입 반환
    }

    public void SetNodeType(NodeType nodeType) //노드의 노드 타입을 설정
    {
        if (nodeType == NodeType.Water || nodeType == NodeType.Obstacle) //노드 타입이 장애물이나 물일 경우 노드를 리셋!
        {
            Reset();
        }
        m_nodeType = nodeType; //노드 타입 설정
    }

    public void Reset() //노드를 초기화
    {
        //m_nodeType = NodeType.None; //노드 타입을 없음으로 설정하고 부모 노드를 'null' 로 설정
        m_parent = null; //부모 노드를 'null' 로 초기화
        m_fCost = 0; //Fcost 초기화
        m_hCost = 0; //Hcost 초기화
        m_gCost = 0; //Gcost 초기화
    }


    public void SetParent(Node parent) //노드의 부모 노드를 설정
    {
        m_parent = parent;
    }

    //public void SetNode(int row, int col)
    //{
    //    m_row = row;
    //    m_col = col;
    //}

    public int FCost
    {
        get { return m_hCost + m_gCost; } //FCost는 HCost와 GCost의 합이다~
    }

    public int HCost
    {
        get { return m_hCost; } //HCost를 가져옴
    }

    public int GCost
    {
        get { return m_gCost; } //GCost를 가져옴
    }

    public void SetHCost(int cost) //HCost 설정
    {
        m_hCost = cost;
    }

    public void SetGCost(int cost) //GCost 설정
    {
        m_gCost = cost; 
    }
}
