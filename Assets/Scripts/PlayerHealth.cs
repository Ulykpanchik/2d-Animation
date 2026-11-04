using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    [Header("Sounds")]
public AudioClip damageSound;
public AudioClip deathSound;

[Range(0f, 1f)]
public float soundVolume = 0.7f;

    private int currentHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;
    private bool isInvulnerable = false;
    private bool isDead = false;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    public Heart[] hearts;

    void Start()
    {
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable || isDead || damage <= 0 || GameInterface.BlocksGameplay)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }
PlayHealthSound(damageSound);
        animator.SetTrigger("Damage");

        StartCoroutine(Invulnerability());
    }

    IEnumerator Invulnerability()
    {
        isInvulnerable = true;

        float duration = 1.5f;
        float blinkTime = 0.15f;

        float timer = 0f;

        while (timer < duration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(blinkTime);

            timer += blinkTime;
        }

        spriteRenderer.enabled = true;

        isInvulnerable = false;
    }
void PlayHealthSound(AudioClip clip)
{
    if (clip == null)
        return;

    GameObject soundObject = new GameObject("Player Health Sound");
    AudioSource source = soundObject.AddComponent<AudioSource>();

    source.playOnAwake = false;
    source.spatialBlend = 0f;
    source.volume = soundVolume;
    source.clip = clip;
    source.Play();

    Destroy(soundObject, clip.length + 0.1f);
}
    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("Player Died!");
PlayHealthSound(deathSound);
        animator.SetTrigger("Death");

        StartCoroutine(DeathDelay());
    }

    IEnumerator DeathDelay()
    {
        yield return new WaitForSeconds(1f);

        gameObject.SetActive(false);
    }
}