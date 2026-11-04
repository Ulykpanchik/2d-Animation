using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] hazardPrefabs;

    public float spawnRate = 2f;

    public float minX = -8f;
    public float maxX = 8f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnHazard), 1f, spawnRate);
    }

    void SpawnHazard()
    {
        int randomIndex = Random.Range(
            0,
            hazardPrefabs.Length
        );

        float randomX = Random.Range(
            minX,
            maxX
        );

        Vector3 spawnPosition = new Vector3(
            randomX,
            transform.position.y,
            0f
        );

        GameObject newHazard = Instantiate(
            hazardPrefabs[randomIndex],
            spawnPosition,
            Quaternion.identity
        );

        Hazard hazard = newHazard.GetComponent<Hazard>();

        if (hazard != null)
        {
            hazard.Activate();
        }
    }
}