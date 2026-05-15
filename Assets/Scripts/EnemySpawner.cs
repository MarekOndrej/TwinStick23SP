using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // Enemy to spawn
    [SerializeField] Enemy enemyToSpawn;

    // Chase target
    [SerializeField] Transform chaseTarget;

    // Spawn points
    [SerializeField] EnemySpawnPoints spawnPoints;

    // Number enemies to spawn
    [SerializeField] int numberOfEnemiesToSpawn = 10;

    // Spawn delay
    [SerializeField] float spawnDelay = 1f;

    // Event manager
    private EventManagerSO eventManager;

    // Level manager
    LevelManager levelManager;

    bool isSpawning;

    private void Awake()
    {
        eventManager = Resources.Load<EventManagerSO>("EventManager");
        levelManager = FindFirstObjectByType<LevelManager>();
        isSpawning = false;
    }

    private void OnEnable()
    {

        eventManager.onZoneTriggered += StartSpawningEnemies;
    }

    private void OnDisable()
    {
        eventManager.onZoneTriggered -= StartSpawningEnemies;
    }

    private void StartSpawningEnemies()
    {
        if (isSpawning)
            return; // Already spawning, nothing to do

        isSpawning = true;
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        // Get all the possible spawn locations
        var possibleLocations = spawnPoints.GetSpawnPoint(); // using method

        int enemiesSpawnedThisWave = 0;

        while (enemiesSpawnedThisWave < numberOfEnemiesToSpawn)
        {
            // Pause coroutine if the game is not running
            while (levelManager.CurrentGameState != GameState.running)
            {
                yield return null;
            }

            // choose a random spawn location
            int randomIndex = Random.Range(0, possibleLocations.Count);
            var chosenSpawnPoint = possibleLocations[randomIndex];

            // Spawn an enemy at the spawn location
            Instantiate(enemyToSpawn, chosenSpawnPoint.position, Quaternion.identity).SetChaseTarget(chaseTarget);

            enemiesSpawnedThisWave++;

            // Delay
            float timer = 0f;
            while (timer < spawnDelay)
            {
                if (levelManager.CurrentGameState == GameState.running)
                {
                    timer += Time.deltaTime;
                }
                yield return null;
            }

        }

        isSpawning = false;
    }

}
