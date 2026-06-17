using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    public GameObject asteroidPrefab;

    public float spawnInterval = 1f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnAsteroid), 0f, spawnInterval);
    }

    void SpawnAsteroid()
    {
        Instantiate(asteroidPrefab, transform.position, Quaternion.identity);
    }
}
