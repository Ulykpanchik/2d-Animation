using UnityEngine;

public class Bomb : Hazard
{
    public float explosionTime = 2f;
    public float explosionRadius = 2f;
    public int explosionDamage = 1;

    public GameObject explosionPrefab;
[Header("Sounds")]
public AudioClip spawnSound;
public AudioClip fallSound;
public AudioClip explosionSound;

private AudioSource fallAudio;
private Rigidbody2D rb;
    void Start()
{
    rb = GetComponent<Rigidbody2D>();

    fallAudio = gameObject.AddComponent<AudioSource>();
    fallAudio.playOnAwake = false;
    fallAudio.loop = true;
    fallAudio.spatialBlend = 1f;
fallAudio.rolloffMode = AudioRolloffMode.Linear;
fallAudio.minDistance = 1f;
fallAudio.maxDistance = 20f;
fallAudio.dopplerLevel = 0f;
    fallAudio.volume = 0.6f;
    fallAudio.clip = fallSound;

    PlaySound(spawnSound);

    Invoke(nameof(Explode), explosionTime);
}

void Update()
{
    bool isFalling = rb != null && rb.linearVelocity.y < -0.1f;

    if (isFalling && fallSound != null)
    {
        if (!fallAudio.isPlaying)
            fallAudio.Play();
    }
    else if (fallAudio.isPlaying)
    {
        fallAudio.Stop();
    }
}

    public override void Activate()
    {
        Debug.Log("Bomb activated!");
    }

    void Explode()
    {
        Debug.Log("BOOM!");
        fallAudio.Stop();
PlaySound(explosionSound);

        // Создаём взрыв
        if (explosionPrefab != null)
        {
            Instantiate(
                explosionPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        // Ищем игрока в радиусе взрыва
        Collider2D[] objectsInRange =
            Physics2D.OverlapCircleAll(
                transform.position,
                explosionRadius
            );

        foreach (Collider2D obj in objectsInRange)
        {
            PlayerHealth player = obj.GetComponent<PlayerHealth>();

            if (player != null)
            {
                player.TakeDamage(explosionDamage);
            }
        }

        Destroy(gameObject);
    }
void PlaySound(AudioClip clip)
{
    if (clip == null)
        return;

    GameObject soundObject = new GameObject("Bomb Sound");
    soundObject.transform.position = transform.position;
    AudioSource source = soundObject.AddComponent<AudioSource>();

    source.playOnAwake = false;
    source.spatialBlend = 1f;
source.rolloffMode = AudioRolloffMode.Linear;
source.minDistance = 1f;
source.maxDistance = 20f;
source.dopplerLevel = 0f;
    source.volume = 0.7f;
    source.clip = clip;
    source.Play();

    Destroy(soundObject, clip.length + 0.1f);
}
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius
        );
    }
}