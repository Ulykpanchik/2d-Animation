using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float moveDistance = 3f;

    public float idleTime = 3f;
[Header("Sounds")]
public AudioClip walkSound;
public AudioClip deathSound;

private AudioSource walkAudio;
    private Vector3 startPosition;
    private bool movingRight = true;
    private bool isWalking = true;
    private bool isDead = false;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    void Start()
    {
        startPosition = transform.position;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        animator.SetBool("IsWalking", true);
        walkAudio = gameObject.AddComponent<AudioSource>();
walkAudio.playOnAwake = false;
walkAudio.loop = true;
walkAudio.spatialBlend = 1f;
walkAudio.rolloffMode = AudioRolloffMode.Linear;
walkAudio.minDistance = 1f;
walkAudio.maxDistance = 20f;
walkAudio.dopplerLevel = 0f;
walkAudio.volume = 0.5f;
walkAudio.clip = walkSound;

if (walkSound != null)
{
    walkAudio.Play();
}
    }

    void Update()
    {
        if (isDead)
            return;

        if (!isWalking)
            return;

        if (movingRight)
        {
            transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);

            if (transform.position.x >= startPosition.x + moveDistance)
            {
                movingRight = false;
                StartCoroutine(Idle());
            }
        }
        else
        {
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);

            if (transform.position.x <= startPosition.x - moveDistance)
            {
                movingRight = true;
                StartCoroutine(Idle());
            }
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !movingRight;
        }
    }

    IEnumerator Idle()
    {
        isWalking = false;
        walkAudio.Stop();

        animator.SetBool("IsWalking", false);

        yield return new WaitForSeconds(idleTime);

        if (isDead)
            yield break;

        animator.SetBool("IsWalking", true);

        isWalking = true;
        if (walkSound != null)
{
    walkAudio.Play();
}
    }

    public void Die()
{
    if (isDead)
        return;

    isDead = true;
    walkAudio.Stop();

if (deathSound != null)
{
    GameObject soundObject = new GameObject("Enemy Death Sound");
    soundObject.transform.position = transform.position;

    AudioSource source = soundObject.AddComponent<AudioSource>();
    source.playOnAwake = false;
    source.spatialBlend = 1f;
    source.rolloffMode = AudioRolloffMode.Linear;
    source.minDistance = 1f;
    source.maxDistance = 20f;
    source.dopplerLevel = 0f;
    source.volume = 0.7f;
    source.clip = deathSound;
    source.Play();

    Destroy(soundObject, deathSound.length + 0.1f);
}

    animator.SetTrigger("Death");

    Collider2D collider = GetComponent<Collider2D>();

    if (collider != null)
    {
        collider.enabled = false;
    }
}

    
private void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.CompareTag("Arrow") && !isDead)
{
    Arrow arrow = collision.gameObject.GetComponent<Arrow>();

    if (arrow != null)
    {
        arrow.PlayHitSound();
    }

    Die();
    Destroy(collision.gameObject);
}

    if (collision.gameObject.CompareTag("Player"))
    {
        PlayerHealth playerHealth =
            collision.gameObject.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1);
        }
    }
}
    
}