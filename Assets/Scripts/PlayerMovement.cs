
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float playerSize = 3f;

    [Header("Arrow Settings")]
    public GameObject arrowPrefab;
    public Transform arrowPoint;
    public float arrowSize = 8f;
    public float shootCooldown = 1f;
[Header("Sounds")]
public AudioClip runSound;

private AudioSource runAudio;
public AudioClip[] jumpSounds;
public AudioClip[] shootSounds;
public AudioClip landSound;
private AudioSource jumpAudio;
    private float nextShootTime = 0f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 moveInput;
    private bool jumpRequested;
    private bool touchLeft, touchRight;
    private float Horizontal => Mathf.Clamp(moveInput.x + (touchRight ? 1 : 0) - (touchLeft ? 1 : 0), -1, 1);
    private bool CanAct => !GameInterface.BlocksGameplay && Time.timeScale > 0 &&
        (GetComponent<PlayerHealth>() == null || !GetComponent<PlayerHealth>().IsDead);
    public void SetTouchDirection(int direction, bool pressed)
    {
        if (direction < 0) touchLeft = pressed; else touchRight = pressed;
    }
    public void ClearInput() { moveInput = Vector2.zero; touchLeft = touchRight = jumpRequested = false; }
    public void RequestJump() { if (CanAct) jumpRequested = true; }
    public void RequestFire()
    {
        if (!CanAct || Time.time < nextShootTime) return;
        animator.SetTrigger("Attack");
        nextShootTime = Time.time + shootCooldown;
    }
    void OnDisable() { ClearInput(); }


    private bool isGrounded;
    private bool facingRight = true;
    private Vector3 arrowPointStartPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        runAudio = gameObject.AddComponent<AudioSource>();
runAudio.playOnAwake = false;
runAudio.loop = true;
runAudio.spatialBlend = 0f;
runAudio.volume = 1f;
jumpAudio = gameObject.AddComponent<AudioSource>();
jumpAudio.playOnAwake = false;
jumpAudio.spatialBlend = 0f;
jumpAudio.volume = 1.2f;
    }

    void Start()
{
    transform.localScale = new Vector3(
        playerSize,
        playerSize,
        playerSize
    );

    if (arrowPoint != null)
    {
        arrowPointStartPosition = arrowPoint.localPosition;
        UpdateArrowPoint();
    }
}

    // NEW INPUT SYSTEM - MOVE
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = CanAct ? context.ReadValue<Vector2>() : Vector2.zero;
    }

    // NEW INPUT SYSTEM - JUMP
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            RequestJump();
        }
    }

    // NEW INPUT SYSTEM - FIRE
    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed) RequestFire();
    }

    void Update()
    {
        if (!CanAct) { jumpRequested = false; if (runAudio.isPlaying) runAudio.Stop(); return; }
        float move = Horizontal;
        bool isRunning = Mathf.Abs(move) > 0.01f && isGrounded;

if (isRunning && runSound != null)
{
    if (!runAudio.isPlaying)
    {
        runAudio.clip = runSound;
        runAudio.Play();
    }
}
else if (runAudio.isPlaying)
{
    runAudio.Stop();
}

        // Animator
        animator.SetFloat("Speed", Mathf.Abs(move));
        animator.SetBool("IsGrounded", isGrounded);

        // Flip sprite
        if (move > 0.01f && !facingRight)
{
    facingRight = true;
    spriteRenderer.flipX = false;
    UpdateArrowPoint();
}
else if (move < -0.01f && facingRight)
{
    facingRight = false;
    spriteRenderer.flipX = true;
    UpdateArrowPoint();
}
    }

    void FixedUpdate()
    {
        if (!CanAct) return;
        // Movement
        rb.linearVelocity = new Vector2(
            Horizontal * moveSpeed,
            rb.linearVelocity.y
        );

        // Jump
        if (jumpRequested && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            isGrounded = false;
            runAudio.Stop();

if (jumpSounds != null && jumpSounds.Length > 0)
{
    int index = Random.Range(0, jumpSounds.Length);
    AudioClip clip = jumpSounds[index];

    if (clip != null)
    {
        jumpAudio.PlayOneShot(clip);
    }
}
        }

        jumpRequested = false;
    }

    // Called by Attack animation event
    public void ShootArrow()
    {
        if (!CanAct || arrowPrefab == null || arrowPoint == null)
            return;
if (shootSounds != null && shootSounds.Length > 0)
{
    int index = Random.Range(0, shootSounds.Length);
    AudioClip clip = shootSounds[index];

    if (clip != null)
    {
        jumpAudio.PlayOneShot(clip);
    }
}
        float direction = facingRight ? 1f : -1f;

        GameObject arrow = Instantiate(
            arrowPrefab,
            arrowPoint.position,
            Quaternion.identity
        );

        Arrow arrowScript = arrow.GetComponent<Arrow>();

        if (arrowScript != null)
        {
            arrowScript.SetDirection(direction);
            arrowScript.SetSize(arrowSize);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
{
    if (!collision.gameObject.CompareTag("Ground"))
        return;

    // Учитываем только поверхность под ногами.
    bool landedOnTop = false;

    foreach (ContactPoint2D contact in collision.contacts)
    {
        if (contact.normal.y > 0.5f)
        {
            landedOnTop = true;
            break;
        }
    }

    if (landedOnTop)
    {
        if (!isGrounded && landSound != null)
        {
            jumpAudio.PlayOneShot(landSound);
        }

        isGrounded = true;
    }
}
    private void UpdateArrowPoint()
{
    if (arrowPoint == null) return;

    float direction = facingRight ? 1f : -1f;

    arrowPoint.localPosition = new Vector3(
        Mathf.Abs(arrowPointStartPosition.x) * direction,
        arrowPointStartPosition.y,
        arrowPointStartPosition.z
    );
}
}
