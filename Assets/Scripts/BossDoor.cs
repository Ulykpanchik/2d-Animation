
using UnityEngine;

public class BossDoor : MonoBehaviour
{
    [Header("Boss Settings")]
    public Boss boss;

    [Header("Portal Settings")]
    public AudioClip openSound;

    [Header("Final Victory")]
    public string restartScene = "Level1";

    private Animator animator;
    private bool isOpen = false;
    private bool victoryShown = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Open portal after boss dies
        if (!isOpen && boss != null && boss.IsDead)
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        if (isOpen) return;

        isOpen = true;

        if (animator != null)
            animator.SetTrigger("Open");

        if (openSound != null)
        {
            GameObject soundObject =
                new GameObject("Boss Portal Open Sound");

            soundObject.transform.position = transform.position;

            AudioSource source =
                soundObject.AddComponent<AudioSource>();

            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1f;
            source.maxDistance = 20f;
            source.dopplerLevel = 0f;
            source.volume = 0.7f;
            source.clip = openSound;
            source.Play();

            Destroy(soundObject, openSound.length + 0.1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryShowVictory(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryShowVictory(other);
    }

    private void TryShowVictory(Collider2D other)
    {
        if (!isOpen || victoryShown)
            return;

        if (!other.CompareTag("Player"))
            return;

        GameInterface gameInterface =
            FindAnyObjectByType<GameInterface>();

        if (gameInterface == null)
        {
            Debug.LogWarning(
                "BossDoor: GameInterface not found!"
            );
            return;
        }

        victoryShown = true;

        // Show final victory
        gameInterface.ShowVictory(restartScene, true);
    }
}
