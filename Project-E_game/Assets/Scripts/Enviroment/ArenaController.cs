
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaController : MonoBehaviour
{
    [Header("Arena Settings")]
    public Vector2 arenaMin; // Minimum x and y of the arena
    public Vector2 arenaMax; // Maximum x and y of the arena

    [Header("Obstacle Settings")]
    [SerializeField] GameObject[] obstaclePrefabs;
    [SerializeField] int maxObstacles = 10;
    [SerializeField] float spawnRadius = 2f;
    [SerializeField] int maxSpawnAttempts = 10;

    [Header("Rendering")]
    public string sortingLayerName = "Default";

    public void SpawnObstacles()
    {
        // Clear out old obstacles first
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Prop") || child.CompareTag("MovableProp"))
            {
                Destroy(child.gameObject);
            }
        }

        for (int i = 0; i < maxObstacles; i++)
        {
            // Generate a random position within the local bounds of the arena
            Vector3 localPosition = RandomPosition(arenaMin, arenaMax);

            // Convert local position to world space considering the arena's position and orientation
            Vector3 worldPosition = transform.TransformPoint(localPosition);

            // Instantiate the obstacle at the world position, but parented to the arena
            GameObject obstacle = Instantiate(obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)], worldPosition, Quaternion.identity, transform);

            SpriteRenderer spriteRenderer = obstacle.GetComponent<SpriteRenderer>();
            if (spriteRenderer)
            {
                spriteRenderer.sortingLayerName = sortingLayerName;
            }
        }
    }

    public Vector3 FindFreePosition()
    {
        Vector3 potentialPosition;
        bool positionFree;
        int attempts = 0;

        do
        {
            potentialPosition = RandomPosition(arenaMin, arenaMax);
            positionFree = !Physics2D.OverlapCircle(potentialPosition, spawnRadius);

            foreach (Transform child in transform)
            {
                if (child.CompareTag("Prop"))
                {
                    float distanceToObstacle = Vector3.Distance(potentialPosition, child.position);
                    if (distanceToObstacle < spawnRadius)
                    {
                        positionFree = false;
                    }
                }
            }

            attempts++;
        } while (!positionFree && attempts < maxSpawnAttempts);

        return potentialPosition; // This will return the last attempted position even if it's not free after max attempts
    }

    Vector3 RandomPosition(Vector2 min, Vector2 max)
    {
        return new Vector3(
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y),
            transform.position.z);
    }
}
