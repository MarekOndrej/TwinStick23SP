using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    // === Per-wave configuration ===

    [System.Serializable]
    public class WaveConfig
    {
        [Tooltip("Display label for the wave (used in log/HUD).")]
        public string name = "Wave";

        [Tooltip("Enemy prefab to spawn during this wave.")]
        public Enemy enemyPrefab;

        [Tooltip("How many enemies to spawn during this wave.")]
        [Min(1)] public int count = 5;

        [Tooltip("Seconds between successive spawns in this wave.")]
        [Min(0f)] public float spawnInterval = 1f;

        [Tooltip("After all this wave's enemies are defeated, pause this many " +
                 "seconds before the next wave begins.")]
        [Min(0f)] public float restAfterWave = 4f;

        [Tooltip("If >= 0, override the enemy prefab's sight range for enemies " +
                 "spawned in this wave. Use to scale difficulty: early waves " +
                 "with short LOS, later waves with long LOS. -1 = use prefab default.")]
        public float sightRangeOverride = -1f;
    }

    [Header("Wave list")]
    [Tooltip("Waves are run in order. Leave empty to auto-build a single wave " +
             "from the legacy 'Enemy To Spawn' / 'Number Of Enemies To Spawn' " +
             "/ 'Spawn Delay' fields below.")]
    [SerializeField] WaveConfig[] waves;

    [Header("Endless mode")]
    [Tooltip("After the last wave clears, loop back to the start with scaled difficulty.")]
    [SerializeField] bool endlessLoop = true;

    [Tooltip("Per loop, multiply each wave's enemy count by this factor (1 = no scaling).")]
    [Min(1f)] [SerializeField] float endlessCountMultiplier = 1.3f;

    [Tooltip("Per loop, divide each wave's spawnInterval by this factor (1 = no scaling).")]
    [Min(1f)] [SerializeField] float endlessSpeedMultiplier = 1.15f;

    [Header("Targets & spawn points")]
    [SerializeField] Transform chaseTarget;
    [SerializeField] EnemySpawnPoints spawnPoints;

    [Header("Legacy single-wave fields (used only if Waves list is empty)")]
    [SerializeField] Enemy enemyToSpawn;
    [SerializeField] int numberOfEnemiesToSpawn = 10;
    [SerializeField] float spawnDelay = 1f;

    EventManagerSO eventManager;
    LevelManager levelManager;

    // Coroutine + state
    Coroutine _runCoroutine;
    int _currentWaveIndex = -1;
    int _aliveThisWave;

    private void Awake()
    {
        eventManager = Resources.Load<EventManagerSO>("EventManager");
        levelManager = FindFirstObjectByType<LevelManager>();

        // Back-compat: synthesize a single wave from legacy fields.
        if (waves == null || waves.Length == 0)
        {
            waves = new[]
            {
                new WaveConfig
                {
                    name = "Wave 1",
                    enemyPrefab = enemyToSpawn,
                    count = numberOfEnemiesToSpawn,
                    spawnInterval = spawnDelay,
                    restAfterWave = 4f,
                }
            };
        }
    }

    private void OnEnable()
    {
        if (eventManager == null) return;
        eventManager.onZoneTriggered += StartWaves;
        eventManager.onEnemyDefeated += HandleEnemyDefeated;
    }

    private void OnDisable()
    {
        if (eventManager == null) return;
        eventManager.onZoneTriggered -= StartWaves;
        eventManager.onEnemyDefeated -= HandleEnemyDefeated;
    }

    private void StartWaves()
    {
        if (_runCoroutine != null) return; // already running
        _runCoroutine = StartCoroutine(RunWaves());
    }

    private void HandleEnemyDefeated(int _)
    {
        _aliveThisWave = Mathf.Max(0, _aliveThisWave - 1);
    }

    private IEnumerator RunWaves()
    {
        var possibleLocations = spawnPoints.GetSpawnPoint();
        int loopIndex = 0;

        while (true)
        {
            for (int i = 0; i < waves.Length; i++)
            {
                _currentWaveIndex = i;
                var wave = waves[i];

                if (wave.enemyPrefab == null)
                {
                    Debug.LogWarning($"EnemySpawner: wave '{wave.name}' has no enemy prefab assigned, skipping.");
                    continue;
                }

                int scaledCount = Mathf.RoundToInt(wave.count * Mathf.Pow(endlessCountMultiplier, loopIndex));
                float scaledInterval = wave.spawnInterval / Mathf.Pow(endlessSpeedMultiplier, loopIndex);

                int waveDisplayNumber = loopIndex * waves.Length + i + 1;
                eventManager.WaveStarted(waveDisplayNumber);

                _aliveThisWave = 0;
                yield return SpawnEnemies(wave.enemyPrefab, scaledCount, scaledInterval, possibleLocations, wave.sightRangeOverride);

                // Wait for the wave to be fully defeated.
                while (_aliveThisWave > 0)
                {
                    yield return null;
                }

                eventManager.WaveCleared(waveDisplayNumber);

                yield return WaitPauseAware(wave.restAfterWave);
            }

            if (!endlessLoop)
            {
                eventManager.AllWavesCleared();
                _runCoroutine = null;
                yield break;
            }

            loopIndex++;
        }
    }

    private IEnumerator SpawnEnemies(Enemy prefab, int count, float interval, List<Transform> locations, float sightRangeOverride)
    {
        for (int i = 0; i < count; i++)
        {
            // Pause-aware: stall while the game isn't running.
            while (levelManager != null && levelManager.CurrentGameState != GameState.running)
            {
                yield return null;
            }

            var chosenSpawnPoint = locations[Random.Range(0, locations.Count)];
            Vector3 spawnPos = chosenSpawnPoint.position;

            // Snap to the nearest navmesh point of the AGENT'S OWN TYPE.
            // Critical detail: the scene has multiple NavMeshSurfaces with
            // different AgentTypeIDs. NavMesh.SamplePosition without a filter
            // would happily snap to any one of them — then Instantiate of an
            // enemy with a Humanoid-type agent would fail to bind to a
            // custom-type navmesh ("Failed to create agent because it is not
            // close enough to the NavMesh"). Using NavMeshQueryFilter with the
            // prefab's agent type makes the snap pick a navmesh the agent can
            // actually use.
            int agentTypeID = 0;
            var prefabAgent = prefab.GetComponent<NavMeshAgent>();
            if (prefabAgent != null) agentTypeID = prefabAgent.agentTypeID;

            var filter = new NavMeshQueryFilter
            {
                agentTypeID = agentTypeID,
                areaMask = NavMesh.AllAreas,
            };

            if (NavMesh.SamplePosition(spawnPos, out NavMeshHit navHit, 50f, filter))
            {
                float snapDistance = Vector3.Distance(spawnPos, navHit.position);
                if (snapDistance > 5f)
                {
                    Debug.LogWarning($"EnemySpawner: spawn point '{chosenSpawnPoint.name}' is {snapDistance:F1}u from the nearest navmesh of AgentTypeID {agentTypeID}. " +
                                     $"Snapping to {navHit.position}. Consider re-baking the navmesh to cover the spawn area, or moving the spawn point.");
                }
                spawnPos = navHit.position;
            }
            else
            {
                Debug.LogWarning($"EnemySpawner: no navmesh of AgentTypeID {agentTypeID} within 50u of spawn point '{chosenSpawnPoint.name}' at {chosenSpawnPoint.position}. " +
                                 "Skipping this spawn. Open Window > AI > Navigation, verify the Humanoid (or matching) NavMeshSurface is baked, " +
                                 "and that the blue navmesh tint in the Scene view covers the spawn points.");
                continue;
            }

            var instance = Instantiate(prefab, spawnPos, Quaternion.identity);
            instance.SetChaseTarget(chaseTarget);
            // Apply per-wave sight range override (no-op when -1).
            instance.SetSightRange(sightRangeOverride);
            _aliveThisWave++;

            if (i < count - 1)
            {
                yield return WaitPauseAware(interval);
            }
        }
    }

    // Time-budget that stalls during pause / game-over. Used between spawns
    // and between waves so the player can pause without rushing the rest period.
    private IEnumerator WaitPauseAware(float seconds)
    {
        float remaining = seconds;
        while (remaining > 0f)
        {
            if (levelManager == null || levelManager.CurrentGameState == GameState.running)
            {
                remaining -= Time.deltaTime;
            }
            yield return null;
        }
    }
}
