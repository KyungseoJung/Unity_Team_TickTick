using UnityEngine;

public class EnemySound : MonoBehaviour
{
    public float maxDistance = 2.0f; // 플레이어와의 최대 거리
    public AudioClip enemySound; // 적 음성 오디오 클립

    private AudioSource audioSource; // AudioSource 컴포넌트
    private GameObject player; // 플레이어 객체

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player");
        InvokeRepeating("CheckDistance", 0.0f, 2.0f);
    }

    void CheckDistance()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= maxDistance && !audioSource.isPlaying)
        {
            audioSource.clip = enemySound;
            audioSource.Play();
        }
        else if (distance > maxDistance && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}