using UnityEngine;

public class BossRoomManager_Copilot : MonoBehaviour
{
    [Tooltip("Assign the Boss Enemy prefab that should spawn in this room when the player enters.")]
    public string bossFolderPath = "Bosses";

    [Tooltip("Optional: Assign a Transform where the boss should spawn. If left empty, the boss will spawn at this object's position.")]
    public Transform spawnPoint;

    private bool bossSpawned = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is the player and the boss hasn't spawned yet.
        if (!bossSpawned && other.CompareTag("Player"))
        {
            SpawnBossEnemy();
        }
    }

    private void SpawnBossEnemy()
    {
        GameObject[] bossPrefabs = Resources.LoadAll<GameObject>(bossFolderPath);


        if (bossPrefabs == null || bossPrefabs.Length == 0)
        {
            Debug.LogError("BossRoomManager: Boss enemy prefab is not assigned!");
            return;
        }

        // Randomly choose an enemy prefab.
        int randomIndex = Random.Range(0, bossPrefabs.Length);
        GameObject bossEnemyPrefab = bossPrefabs[randomIndex];

        // Determine spawn position
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;

        // Optionally adjust the Y if needed (for example, if you want the boss to spawn at floor level).
        // spawnPos.y = desiredYPosition;

        GameObject theBoss = Instantiate(bossEnemyPrefab, spawnPos, Quaternion.identity);
        bossSpawned = true;
        Debug.Log("Boss spawned in the boss room.");

        theBoss.transform.localScale = new Vector3(250f, 250f, 250f);

    }
}
