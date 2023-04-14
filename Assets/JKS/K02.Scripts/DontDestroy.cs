using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;  //@3-1 SceneManager를 위해서 선언해줘야 함

public class DontDestroy : MonoBehaviour
{
    void Awake()    
    {
        //@3-1 이 게임오브젝트가 사라지지 않도록
        DontDestroyOnLoad(this.gameObject);
        
    }
}
