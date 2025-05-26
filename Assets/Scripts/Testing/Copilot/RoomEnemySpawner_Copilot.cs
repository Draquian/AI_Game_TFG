using UnityEngine;

public class RoomEnemySpawner_Copilot: MonoBehaviour
{
    [Header("Enemy Spawn Settings")]
    [Tooltip("Relative path (inside the Resources folder) for the enemy asset prefab (e.g., 'EnemyAsset')")]
    public string enemyFolderPath = "Enemies";
    
    [Tooltip("Number of enemies to spawn in this room.")]
    public int enemyCount = 3;

    // Flag to ensure that enemies spawn only once per room entry.
    private bool hasSpawned = false;

    // When the player enters the room (detected by the room floor’s trigger collider), spawn enemies.
    private void OnTriggerEnter(Collider other)
    {
        if (!hasSpawned && other.CompareTag("Player"))
        {
            SpawnEnemies();
            hasSpawned = true;
        }
    }

    // Spawns enemies across the entire room, placing them at floor level.
    void SpawnEnemies()
    {
        // Get the collider attached to the room floor.
        Collider roomCollider = GetComponent<Collider>();
        if (roomCollider == null)
        {
            Debug.LogWarning("RoomEnemySpawner: No Collider found on " + gameObject.name);
            return;
        }

        // Use the collider's bounds to determine the floor area.
        Bounds bounds = roomCollider.bounds;

        // Load the enemy prefab from the Resources folder.
        GameObject[] enemyPrefabs = Resources.LoadAll<GameObject>(enemyFolderPath);
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("RoomEnemySpawner: Could not load enemy asset from Resources/" + enemyFolderPath);
            return;
        }

        // Randomly choose an enemy prefab.
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject enemyPrefab = enemyPrefabs[randomIndex];

        // Spawn the specified number of enemies.
        for (int i = 0; i < enemyCount; i++)
        {
            // Generate a random position within the room's floor area.
            float randomX = Random.Range(bounds.min.x + enemyPrefab.transform.localScale.x, bounds.max.x - enemyPrefab.transform.localScale.x);
            float randomZ = Random.Range(bounds.min.z + enemyPrefab.transform.localScale.z, bounds.max.z - enemyPrefab.transform.localScale.z);
            // Use bounds.min.y to set the enemy exactly at the floor level.
            Vector3 spawnPos = new Vector3(randomX, bounds.min.y + enemyPrefab.transform.localScale.y, randomZ);

            // Instantiate the chosen enemy enemy at the given position.
            GameObject currentEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            currentEnemy.transform.localScale = new Vector3(100f, 100f, 100f);
        }
    }
}
