using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public int totalCoins = 5;
    private int collectedCoins = 0;

    public Door door;

    public void AddCoin()
    {
        collectedCoins++;

        Debug.Log("Coins: " + collectedCoins + " / " + totalCoins);

        if (collectedCoins >= totalCoins)
        {
            if (door != null)
            {
                door.OpenDoor();
            }

            Debug.Log("All coins collected! Portal opened!");
        }
    }
}