using UnityEngine;
using System.Collections;

public class Heart : MonoBehaviour
{
    public Sprite emptyHeart;
    public float damageAnimationTime = 0.5f;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool lost = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    public void LoseHeart()
    {
        if (lost)
            return;

        lost = true;

        animator.SetTrigger("Damage");

        StartCoroutine(BecomeEmpty());
    }

    IEnumerator BecomeEmpty()
    {
        yield return new WaitForSeconds(damageAnimationTime);

        animator.enabled = false;
        spriteRenderer.sprite = emptyHeart;
    }
}