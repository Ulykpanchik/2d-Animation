using UnityEngine;

public class Coin : MonoBehaviour
{
    public AudioClip collectSound;

[Range(0f, 1f)]
public float soundVolume = 0.7f;
    private Animator animator;
    private Collider2D coinCollider;

    private bool collected = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        coinCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !collected)
        {
            collected = true;
            if (collectSound != null)
{
    GameObject soundObject = new GameObject("Coin Sound");
    AudioSource source = soundObject.AddComponent<AudioSource>();

    source.playOnAwake = false;
    source.spatialBlend = 0f;
    source.volume = soundVolume;
    source.clip = collectSound;
    source.Play();

    Destroy(soundObject, collectSound.length + 0.1f);
}

            // Засчитываем монету
            CoinManager manager = FindAnyObjectByType<CoinManager>();

            if (manager != null)
            {
                manager.AddCoin();
            }

            // Чтобы игрок не мог подобрать монету второй раз
            if (coinCollider != null)
            {
                coinCollider.enabled = false;
            }

            // Запускаем анимацию подбора
            if (animator != null)
            {
                animator.SetTrigger("Collect");
            }

            // Удаляем монету после анимации
            Destroy(gameObject, 1f);
        }
    }
}