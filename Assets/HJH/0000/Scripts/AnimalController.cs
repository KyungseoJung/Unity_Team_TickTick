using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamInterface;
using UnityEngine.AI;

public class AnimalController : EnemyAttack
{
   
     
    public float moveSpeed = 1f; // 동물의 이동 속도
    public float rotateSpeed = 1f; // 동물의 회전 속도
    public float directionChangeInterval = 5f; // 방향 변경 주기 (초)

    private float directionChangeTimer = 0f; // 방향 변경 타이머
private Vector3 moveDirection; // 이동 방향

private void Start()
{
    // 초기 이동 방향 설정
    moveDirection = GetRandomDirection();
}

private void Update()
{
    // 방향 변경 주기마다 새로운 이동 방향 설정
    directionChangeTimer += Time.deltaTime;
    if (directionChangeTimer >= directionChangeInterval)
    {
        moveDirection = GetRandomDirection();
        directionChangeTimer = 0f;
    }

    // 현재 이동 방향으로 이동
    transform.position += moveDirection * moveSpeed * Time.deltaTime;

    // 현재 이동 방향으로 회전
    Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

    // 회전 방향으로 동물의 모습도 회전
    float rotationAngle = Vector3.SignedAngle(Vector3.forward, moveDirection, Vector3.up);
    transform.rotation = Quaternion.Euler(0, rotationAngle, 0);
}

private Vector3 GetRandomDirection()
{
    // 랜덤한 방향 벡터 생성
    Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));

    // 벡터를 단위 벡터로 정규화
    randomDirection.Normalize();

    return randomDirection;
}

      
}