using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxHealth = 100f;      // 최대 체력
    public float currentHealth = 100f;  // 현재 체력

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            // 사망 애니메이션 실행
            animator.SetTrigger("Death");
            // 이후, 다른 액션을 막기 위해 스크립트를 비활성화
            enabled = false;
        }
        else
        {
            // 피격 애니메이션 실행
            animator.SetTrigger("TakeDamage");
        }
    }
}
