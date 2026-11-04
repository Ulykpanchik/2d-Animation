using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 12f;
    public float lifeTime = 3f;
    public AudioClip[] hitSounds;   

    private Rigidbody2D rb;
    private float direction = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        rb.linearVelocity = new Vector2(
            direction * speed,
            0f
        );

        Destroy(gameObject, lifeTime);
    }
public void PlayHitSound()
{
    if (hitSounds == null || hitSounds.Length == 0)
        return;

    AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];

    if (clip == null)
        return;

    GameObject soundObject = new GameObject("Arrow Hit Sound");
    soundObject.transform.position = transform.position;

    AudioSource source = soundObject.AddComponent<AudioSource>();
    source.playOnAwake = false;
    source.spatialBlend = 1f;
    source.rolloffMode = AudioRolloffMode.Linear;
    source.minDistance = 5f;
    source.maxDistance = 20f;
    source.dopplerLevel = 0f;
    source.volume = 0.7f;
    source.clip = clip;
    source.Play();

    Destroy(soundObject, clip.length + 0.1f);
}
    public void SetDirection(float newDirection)
    {
        direction = newDirection;

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        if (sprite != null)
        {
            sprite.flipX = direction < 0;
        }
    }

    public void SetSize(float size)
    {
        transform.localScale = new Vector3(
            size,
            size,
            size
        );
    }
}