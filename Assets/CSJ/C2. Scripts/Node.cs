using UnityEngine;
using UnityEngine.UI;

public enum NodeType
{
    None,
    Water,
    Obstacle
}

public class Node : MonoBehaviour
{
    public NodeType m_nodeType = NodeType.None;
    public int m_fCost = 0;
    public int m_hCost = 0;
    public int m_gCost = 0;
    public int m_row = 0;
    public int m_col = 0;
    public float y = 0;
    private Node m_parent;

    private void Start()
    {
        csCube tmpCube = GetComponent<csCube>();

        if (tmpCube.childObj != null)
        {
            m_nodeType = NodeType.Obstacle;
        }
        else if(tmpCube.cubeInfo.type == TeamInterface.Enum_CubeType.WATER)
        {
            Debug.Log("이것은 물이다");
            m_nodeType = NodeType.Water;
        }
        else
        {
            m_nodeType = NodeType.None;
        }

        m_row = (int)transform.position.x;
        m_col = (int)transform.position.z;
        y = transform.position.y;

        GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().m_nodeArr[(int)transform.position.x, (int)transform.position.z] = this;
        Debug.Log(GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().m_nodeArr[(int)transform.position.x, (int)transform.position.z]);
    }

    public int Row
    {
        get { return m_row; }
    }

    public int Col
    {
        get { return m_col; }
    }

    public Node Parent
    {
        get { return m_parent; }
    }

    public Vector3 POS
    {
        set
        {
            transform.position = value;
        }
        get { return transform.position; }
    }

    public NodeType NType
    {
        get { return m_nodeType; }
    }

    public void SetNodeType(NodeType nodeType)
    {
        if (nodeType == NodeType.Water || nodeType == NodeType.Obstacle)
        {
            Reset();
        }
        m_nodeType = nodeType;
    }

    public void Reset()
    {
        //m_nodeType = NodeType.None;
        m_parent = null;
        m_fCost = 0;
        m_hCost = 0;
        m_gCost = 0;
    }
    
    public void SetParent(Node parent)
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
        get { return m_hCost + m_gCost; }
    }

    public int HCost
    {
        get { return m_hCost; }
    }

    public int GCost
    {
        get { return m_gCost; }
    }

    public void SetHCost(int cost)
    {
        m_hCost = cost;
    }

    public void SetGCost(int cost)
    {
        m_gCost = cost;
    }
}
