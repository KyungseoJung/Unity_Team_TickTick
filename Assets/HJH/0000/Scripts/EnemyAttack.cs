using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float attackRange = 2f;      // 일정 범위
    public float energy = 100f;         // 에너지 초기값
    public float damagePerHit = 10f;    // 공격 데미지

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            float distance = Vector3.Distance(transform.position, other.transform.position);
            if (distance <= attackRange)
            {
                // 플레이어가 일정 범위 내에 있을 때 공격 애니메이션 실행
                animator.SetTrigger("Attack");

                // 플레이어 캐릭터의 TakeDamage 함수 호출
                other.GetComponent<PlayerController>().TakeDamage(damagePerHit);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        energy -= damage;
        if (energy <= 0)
        {
            // 에너지가 없으면 사망 애니메이션 실행
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
