using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class Boss : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Collider2D bodyCollider;
    public LayerMask groundLayer;
    public Transform mouthHigh;
    public Transform mouthLow;
    public GameObject laserPrefab;

    [Header("Sounds")]
    public AudioClip encounterRoar;
    public AudioClip jumpSound;
    public AudioClip landingSound;
    public AudioClip burrowSound;
    public AudioClip undergroundMoveSound;
    public AudioClip emergeSound;
    public AudioClip laserChargeSound;
    public AudioClip laserLoopSound;
    public AudioClip deathSound;
    public AudioClip breathingSound;
    [Range(0, 1)] public float soundVolume = 0.7f;
    [Range(0, 1)] public float breathingVolume = 0.25f;
    public float soundMinDistance = 3;
    public float soundMaxDistance = 25;
    public float encounterDistance = 25;

    private AudioSource ambientAudio;
    private AudioSource laserAudio;
    private AudioSource chargeAudio;

    [Header("Health")]
    public int maxHealth = 100;
    public int arrowDamage = 1;
    public float redFlashTime = 0.08f;
    public Vector2 healthBarOffset = new Vector2(0, 4);
    public float healthBarWidth = 4;

    [Header("Timing")]
    public float firstAttackDelay = 3;
    public float attackPause = 2;
    public float warningTime = 2;
    public float jumpDuration = 1.2f;
    public float jumpHeight = 5;
    public float undergroundTime = 3;
    public float undergroundSpeed = 4;

    [Header("Damage")]
    public int contactDamage = 1;
    public int landingDamage = 1;
    public float landingRadius = 2;
    public float laserLength = 20;
    public float laserThickness = 0.7f;
    public int laserDamage = 1;
    public float laserStartsAt = 0.35f;
    public float laserEndsAt = 0.85f;
    public bool debugLaserBox;

    private Animator anim;
    private SpriteRenderer sprite;
    private Rigidbody2D rb;
    private Color normalColor;
    private int health;
    public int CurrentHealth => health;
    private bool dead;
    public bool IsDead => dead;
    private Coroutine flashRoutine;
    private GameObject warning;
    private GameObject laser;
    private GameObject bar;
    private Transform fill;
    private Sprite square;
    private readonly HashSet<Arrow> usedArrows = new HashSet<Arrow>();
    private bool originalCollider;
    private Vector2 laserCenter;
    private int lastAttack = -1;
    private float footOffset;
    private bool facingRight;
    private Vector3 highMouthLocal;
    private Vector3 lowMouthLocal;

    void Awake()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();
        originalCollider = bodyCollider != null && bodyCollider.enabled;
        footOffset = bodyCollider != null ? transform.position.y - bodyCollider.bounds.min.y : 0;
        normalColor = sprite.color;
        ambientAudio = CreateAudioSource(true);
        laserAudio = CreateAudioSource(true);
        chargeAudio = CreateAudioSource(false);
        if (mouthHigh != null) highMouthLocal = mouthHigh.localPosition;
        if (mouthLow != null) lowMouthLocal = mouthLow.localPosition;
        // All movement is controlled by this script, including the jump arc.
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.useFullKinematicContacts = true;
        health = Mathf.Max(1, maxHealth);
        square = Sprite.Create(Texture2D.whiteTexture,
            new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
            new Vector2(0.5f, 0.5f), Texture2D.whiteTexture.width);
    }

    IEnumerator Start()
    {
        if (player == null)
        {
            GameObject target = GameObject.FindGameObjectWithTag("Player");
            if (target != null) player = target.transform;
        }
        // Health is displayed by the screen-space GameInterface.
        // Wait until the player approaches before announcing the encounter.
        while (!dead && (player == null || !player.gameObject.activeInHierarchy ||
               Vector2.Distance(player.position, transform.position) > encounterDistance))
            yield return null;
        if (dead) yield break;
        FacePlayer();
        PlaySound(encounterRoar);
        yield return new WaitForSeconds(Mathf.Max(firstAttackDelay,
            encounterRoar != null ? encounterRoar.length : 0));
        SetAmbient(breathingSound, breathingVolume);
        while (!dead)
        {
            if (player == null || !player.gameObject.activeInHierarchy)
            {
                yield return null;
                continue;
            }
            int attack;
            do { attack = Random.Range(0, 4); } while (attack == lastAttack);
            lastAttack = attack;
            SetAmbient(null, 0);
            FacePlayer();
            if (attack == 0) yield return JumpAttack(false);
            else if (attack == 1) yield return UndergroundAttack();
            else yield return LaserAttack(attack == 2);
            SetAmbient(breathingSound, breathingVolume);
            yield return new WaitForSeconds(attackPause);
        }
    }

    AudioSource CreateAudioSource(bool looping)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        ConfigureAudio(source);
        source.loop = looping;
        return source;
    }

    void ConfigureAudio(AudioSource source)
    {
        source.playOnAwake = false;
        source.spatialBlend = 1;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = Mathf.Max(0.01f, soundMinDistance);
        source.maxDistance = Mathf.Max(source.minDistance + 0.01f, soundMaxDistance);
        source.dopplerLevel = 0;
        source.volume = soundVolume;
    }

    void SetAmbient(AudioClip clip, float volume)
    {
        ambientAudio.Stop();
        ambientAudio.clip = clip;
        ambientAudio.volume = volume;
        if (clip != null && !dead) ambientAudio.Play();
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        // Detached sound survives the boss object being disabled or destroyed.
        GameObject go = new GameObject("Boss Sound - " + clip.name);
        go.transform.position = transform.position;
        AudioSource source = go.AddComponent<AudioSource>();
        ConfigureAudio(source);
        source.clip = clip;
        source.Play();
        Destroy(go, clip.length + 0.1f);
    }

    void StopContinuousSounds()
    {
        if (ambientAudio != null) ambientAudio.Stop();
        if (laserAudio != null) laserAudio.Stop();
        if (chargeAudio != null) chargeAudio.Stop();
    }

    void LateUpdate()
    {
        if (!dead && anim.GetCurrentAnimatorStateInfo(0).IsName("BossIdle")) FacePlayer();
        if (bar != null)
            bar.transform.position = transform.position + (Vector3)healthBarOffset;
    }

    void FacePlayer()
    {
        if (player == null) return;
        facingRight = player.position.x > transform.position.x;
        // The original sprites face left. Mirror only the art and mouth points.
        sprite.flipX = facingRight;
        if (mouthHigh != null)
            mouthHigh.localPosition = new Vector3(facingRight ? -highMouthLocal.x : highMouthLocal.x,
                highMouthLocal.y, highMouthLocal.z);
        if (mouthLow != null)
            mouthLow.localPosition = new Vector3(facingRight ? -lowMouthLocal.x : lowMouthLocal.x,
                lowMouthLocal.y, lowMouthLocal.z);
    }

    GameObject MakeRect(string name, Vector2 size, Color color, int order)
    {
        GameObject go = new GameObject(name);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = square;
        sr.color = color;
        sr.sortingLayerID = sprite.sortingLayerID;
        sr.sortingOrder = order;
        go.transform.localScale = new Vector3(size.x, size.y, 1);
        return go;
    }

    void CreateHealthBar()
    {
        bar = new GameObject("Boss Health Bar");
        GameObject back = MakeRect("Background", new Vector2(healthBarWidth, 0.25f),
            new Color(0.15f, 0.02f, 0.02f), sprite.sortingOrder + 30);
        back.transform.SetParent(bar.transform, false);
        GameObject front = MakeRect("Health", new Vector2(healthBarWidth, 0.18f),
            new Color(0.2f, 0.95f, 0.25f), sprite.sortingOrder + 31);
        front.transform.SetParent(bar.transform, false);
        fill = front.transform;
    }

    public void TakeDamage(int amount)
    {
        if (dead || amount <= 0) return;
        health = Mathf.Max(0, health - amount);
        if (fill != null)
        {
            float fraction = health / (float)Mathf.Max(1, maxHealth);
            fill.localScale = new Vector3(healthBarWidth * fraction, 0.18f, 1);
            fill.localPosition = new Vector3(-healthBarWidth * (1 - fraction) / 2, 0, 0);
        }
        if (health == 0) { Die(); return; }
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(redFlashTime);
        sprite.color = normalColor;
        flashRoutine = null;
    }

    void HitArrow(Collider2D other)
    {
        Arrow arrow = other.GetComponentInParent<Arrow>();
        if (dead || arrow == null || !usedArrows.Add(arrow)) return;
        arrow.PlayHitSound();
        Destroy(arrow.gameObject);
        TakeDamage(arrowDamage);
    }

    void OnCollisionEnter2D(Collision2D collision) { HitArrow(collision.collider); }
    void OnTriggerEnter2D(Collider2D other) { HitArrow(other); }
    void OnCollisionStay2D(Collision2D collision)
    {
        if (dead || contactDamage <= 0) return;
        PlayerHealth target = collision.collider.GetComponentInParent<PlayerHealth>();
        if (target != null) target.TakeDamage(contactDamage);
    }

    // Wait for the named state, with a timeout to avoid a stuck attack on bad transitions.
    IEnumerator EnterState(string state, string trigger)
    {
        anim.SetTrigger(trigger);
        float timer = 0;
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName(state) && timer < 2)
        { timer += Time.deltaTime; yield return null; }
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName(state))
            Debug.LogWarning("Boss: check Animator state / trigger: " + state + " / " + trigger);
    }

    IEnumerator FinishState(string state)
    {
        float timer = 0;
        while (anim.GetCurrentAnimatorStateInfo(0).IsName(state) && timer < 15)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) break;
            timer += Time.deltaTime;
            yield return null;
        }
    }

    bool LandingPoint(out Vector2 destination, out Vector2 ground)
    {
        destination = rb.position;
        ground = rb.position;
        if (player == null) return false;
        RaycastHit2D hit = Physics2D.Raycast(
            new Vector2(player.position.x, player.position.y + 15), Vector2.down, 60, groundLayer);
        if (!hit) { Debug.LogWarning("Boss: assign Ground Layer and put ground on that layer."); return false; }
        ground = hit.point;
        destination = hit.point + Vector2.up * (footOffset + 0.03f);
        return true;
    }

    IEnumerator Warn(Vector2 ground)
    {
        warning = MakeRect("Danger - landing", new Vector2(landingRadius * 2, 0.18f),
            new Color(1, 0.03f, 0.03f, 0.7f), sprite.sortingOrder + 10);
        warning.transform.position = ground + Vector2.up * 0.08f;
        SpriteRenderer sr = warning.GetComponent<SpriteRenderer>();
        float timer = 0;
        while (timer < warningTime)
        {
            timer += Time.deltaTime;
            sr.color = new Color(1, 0.03f, 0.03f, 0.4f + 0.4f * Mathf.Abs(Mathf.Sin(timer * 9)));
            yield return null;
        }
    }

    IEnumerator JumpAttack(bool emerging)
    {
        if (!LandingPoint(out Vector2 destination, out Vector2 ground)) yield break;
        yield return Warn(ground);
        // The warning stays at the locked destination until impact; it does not follow the player.
        if (emerging) rb.position = destination;
        SetAmbient(null, 0);
        PlaySound(emerging ? emergeSound : jumpSound);
        FacePlayer();
        yield return EnterState(emerging ? "BossEmerge" : "BossJump", emerging ? "Emerge" : "Jump");
        Vector2 origin = rb.position;
        if (bodyCollider != null) bodyCollider.enabled = false;
        float timer = 0;
        float duration = Mathf.Max(0.1f, jumpDuration);
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            rb.position = Vector2.Lerp(origin, destination, t)
                + Vector2.up * (4 * jumpHeight * t * (1 - t));
            yield return null;
        }
        rb.position = destination;
        PlaySound(landingSound);
        DamageCircle(ground + Vector2.up * 0.5f);
        if (warning != null) Destroy(warning);
        if (bodyCollider != null) bodyCollider.enabled = originalCollider;
        yield return FinishState(emerging ? "BossEmerge" : "BossJump");
        anim.Play("BossIdle", 0, 0);
    }

    IEnumerator UndergroundAttack()
    {
        PlaySound(burrowSound);
        yield return EnterState("BossBurrow", "Burrow");
        yield return FinishState("BossBurrow");
        anim.Play("BossUnder", 0, 0);
        SetAmbient(undergroundMoveSound, soundVolume);
        if (bodyCollider != null) bodyCollider.enabled = false;
        float timer = 0;
        while (timer < undergroundTime)
        {
            timer += Time.deltaTime;
            if (player != null)
            {
                FacePlayer();
                rb.position = new Vector2(Mathf.MoveTowards(rb.position.x,
                    player.position.x, undergroundSpeed * Time.deltaTime), rb.position.y);
            }
            yield return null;
        }
        yield return JumpAttack(true);
        SetAmbient(null, 0);
        if (bodyCollider != null) bodyCollider.enabled = originalCollider;
        anim.Play("BossIdle", 0, 0);
    }

    IEnumerator LaserAttack(bool high)
    {
        string state = high ? "BossLaser" : "BossLaserDown";
        Transform mouth = high ? mouthHigh : mouthLow;
        if (mouth == null || laserPrefab == null)
        { Debug.LogWarning("Boss: assign both mouth points and Laser Prefab."); yield break; }
        chargeAudio.Stop();
        chargeAudio.clip = laserChargeSound;
        if (laserChargeSound != null) chargeAudio.Play();
        yield return EnterState(state, high ? "LaserHigh" : "LaserLow");
        float timeout = 0;
        bool fired = false;
        float nextDamage = 0;
        while (anim.GetCurrentAnimatorStateInfo(0).IsName(state) && timeout < 15)
        {
            timeout += Time.deltaTime;
            float t = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
            if (t >= laserEndsAt || t >= 1) break;
            if (t >= laserStartsAt)
            {
                if (!fired)
                {
                    chargeAudio.Stop();
                    laserAudio.clip = laserLoopSound;
                    if (laserLoopSound != null) laserAudio.Play();
                    laser = Instantiate(laserPrefab);
                    BossLaserVisual visual = laser.GetComponent<BossLaserVisual>();
                    if (visual != null)
                    {
                        visual.length = laserLength; visual.thickness = laserThickness;
                        if (visual.frames == null || visual.frames.Length == 0)
                            Debug.LogError("Boss: Laser Prefab has no Frames in BossLaserVisual.");
                    }
                    else Debug.LogError("Boss: add BossLaserVisual to the root of Laser Prefab.");
                    fired = true;
                }
                Vector2 direction = facingRight ? Vector2.right : Vector2.left;
                laserCenter = (Vector2)mouth.position + direction * laserLength / 2;
                laser.transform.position = laserCenter;
                laser.transform.rotation = Quaternion.identity;
                if (Time.time >= nextDamage)
                {
                    DamagePlayers(Physics2D.OverlapBoxAll(laserCenter,
                        new Vector2(laserLength, laserThickness), 0), laserDamage);
                    nextDamage = Time.time + 0.15f;
                }
            }
            yield return null;
        }
        if (laser != null) Destroy(laser);
        laserAudio.Stop();
        chargeAudio.Stop();
        yield return FinishState(state);
        anim.Play("BossIdle", 0, 0);
    }

    void DamageCircle(Vector2 point)
    { DamagePlayers(Physics2D.OverlapCircleAll(point, landingRadius), landingDamage); }

    void DamagePlayers(Collider2D[] targets, int amount)
    {
        HashSet<PlayerHealth> hit = new HashSet<PlayerHealth>();
        foreach (Collider2D target in targets)
        {
            PlayerHealth hp = target.GetComponentInParent<PlayerHealth>();
            if (hp != null && hit.Add(hp)) hp.TakeDamage(amount);
        }
    }

    void Die()
    {
        dead = true;
        StopAllCoroutines();
        StopContinuousSounds();
        PlaySound(deathSound);
        sprite.color = normalColor;
        if (warning != null) Destroy(warning);
        if (laser != null) Destroy(laser);
        if (bar != null) bar.SetActive(false);
        if (bodyCollider != null) bodyCollider.enabled = false;
        foreach (string trigger in new[] { "Jump", "Burrow", "Emerge", "LaserHigh", "LaserLow" })
            anim.ResetTrigger(trigger);
        anim.SetTrigger("Death");
    }

    void OnDisable()
    {
        StopAllCoroutines();
        StopContinuousSounds();
        if (warning != null) Destroy(warning);
        if (laser != null) Destroy(laser);
        if (bar != null) bar.SetActive(false);
    }

    void OnDestroy()
    {
        if (bar != null) Destroy(bar);
        if (square != null) Destroy(square);
    }

    void OnDrawGizmosSelected()
    {
        if (!debugLaserBox) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(laserCenter, new Vector3(laserLength, laserThickness, 0));
    }
}
