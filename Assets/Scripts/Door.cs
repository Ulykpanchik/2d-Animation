
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Portal")]
    public AudioClip openSound;

    public string nextSceneName = "Level2";

    private Animator animator;
    private bool isOpen;
    private bool victoryShown;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OpenDoor()
    {
        if (isOpen) return;

        isOpen = true;

        if (animator != null)
            animator.SetTrigger("Open");

        if (openSound != null)
        {
            GameObject soundObject =
                new GameObject("Portal Open Sound");

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
        if (!isOpen || victoryShown) return;

        if (!other.CompareTag("Player")) return;

        GameInterface gameInterface =
            FindAnyObjectByType<GameInterface>();

        if (gameInterface == null)
        {
            Debug.LogWarning(
                "Door: GameInterface not found."
            );
            return;
        }

        victoryShown = true;

        gameInterface.ShowVictory(nextSceneName, false);
    }
}
