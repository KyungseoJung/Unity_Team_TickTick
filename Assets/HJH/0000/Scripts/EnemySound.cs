using UnityEngine;

public class EnemySound : MonoBehaviour
{
    public float maxDistance = 2.0f; // 플레이어와의 최대 거리
    public AudioClip enemySound; // 적 음성 오디오 클립

    //private AudioSource audioSource; // AudioSource 컴포넌트
    private GameObject player; // 플레이어 객체

    void Start()
    {
        //audioSource = GetComponent<AudioSource>();
        //audioSource.enabled = false;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            float distance = Vector3.Distance(player.transform.position, transform.position);
            if (distance <= maxDistance)
            {
                OptionManager.Ins.PlayEffect(transform.position, enemySound);
                //audioSource.enabled = true;
                // if (!audioSource.isPlaying)
                // {
                //     audioSource.clip = enemySound;
                //     audioSource.Play();
                // }
            }
        }
    }

    // private void OnCollisionStay(Collision collision)
    // {
    //     if (collision.gameObject.CompareTag("Player"))
    //     {
    //         //audioSource.enabled = false;
    //         //audioSource.Stop();
    //     }
    // }
}
