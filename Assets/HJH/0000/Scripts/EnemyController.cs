using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public GameObject player;  // Player GameObject의 참조 변수
    public float fleeDistance; // 도망갈 거리
    private bool isFleeing;    // 도망 중인지 여부를 저장하는 변수

    void Update()
    {
        // 플레이어와의 거리를 계산
        float distance = Vector3.Distance(player.transform.position, transform.position);

        // 도망 중이 아닌 경우
        if (!isFleeing)
        {
            // 일정 거리 이내에 플레이어가 있는 경우
            if (distance < fleeDistance)
            {
                // 도망 상태로 변경
                isFleeing = true;
            }
        }
        // 도망 중인 경우
        else
        {
            // 일정 거리 이상으로 멀어지면 다시 돌아오기
            if (distance > fleeDistance + 2f)
            {
                // 도망 상태 해제
                isFleeing = false;
            }
            else
            {
                // 플레이어를 피하며 도망
                // transform.position 등을 이용하여 이동 처리
            }
        }
    }

}
