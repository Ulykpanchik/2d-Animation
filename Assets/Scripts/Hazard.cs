using UnityEngine;

public class Hazard : MonoBehaviour
{
    public int damage = 1;

    public virtual void Activate()
    {
        Debug.Log("Hazard activated");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage(damage);
        }
    }
}