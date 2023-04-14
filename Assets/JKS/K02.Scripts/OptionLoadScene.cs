using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  

public class OptionLoadScene : MonoBehaviour
{
    void Awake()
    {
        SceneManager.LoadScene("scLobby0");
    }
}
