using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] 
    public Image fillImage; // HP바 이미지
    
    [SerializeField] 
    public float fillSpeed; // HP바가 채워지는 속도

    private float currentHP; // 현재 체력
    private float maxHP; // 최대 체력

    // 플레이어의 최대 체력을 설정
    public void SetMaxHealth(float health)
    {
        maxHP = health;
        currentHP = maxHP;
        UpdateHealthBar();
    }

    // 플레이어의 체력을 갱신
    public void UpdateHealth(float health)
    {
        currentHP = health;
        UpdateHealthBar();
    }

    // HP바 이미지를 갱신
    public void UpdateHealthBar()
    {
        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, currentHP / maxHP, Time.deltaTime * fillSpeed);
    }


    public void UpdateHPBar(float currentHP, float maxHP)
    {
        
    }

}