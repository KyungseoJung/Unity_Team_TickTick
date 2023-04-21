// using UnityEngine;
// using UnityEngine.UI;

// public class HPBar : MonoBehaviour
// {
//     [SerializeField] 
//     public Image fillImage; // HP바 이미지
    
//     [SerializeField]
//     private Shader shader; //Hp바 쉐이더

//     [SerializeField] 
//     public float fillSpeed; // HP바가 채워지는 속도

//     private float currentHP; // 현재 체력
//     private float maxHP; // 최대 체력

//     private Material material; //머트리얼 타입 변수


//    

//     // HP바 이미지를 갱신
//     public void UpdateHealthBar()
//     {
//         fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, currentHP / maxHP, Time.deltaTime * fillSpeed);
//     }


// }
// 아까워서 남겨둠.....

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer sr;


    [SerializeField]
    private Material material;

    private float currentHP=1;
    private float maxHP=1;

    private void Awake() {
        material = sr.material;
        material.SetFloat("_Progress", 0.8f);
    }

     public void UpdateHPBar(float currentHP, float maxHP)
    {
        
        material.SetFloat("_Progress", (((float)currentHP / maxHP) * 0.8f));
        Debug.Log(currentHP + "..." + maxHP + "..." + ((float)currentHP / maxHP)+"./." + material.GetFloat("_Progress"));
    }

    //플레이어의 체력을 갱신
    public void UpdateHealth(float health)
    {
        currentHP = health;
        UpdateHPBar(currentHP, maxHP);
    }

    // 플레이어의 최대 체력을 설정
    public void SetMaxHealth(float maxHealth)
    {
        //....
        currentHP = maxHP = maxHealth;
    }
}