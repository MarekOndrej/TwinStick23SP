using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    //spawn points
    [SerializeField] EnemySpawnPoints spawnPoints;

    //enemy to spawn
    [SerializeField] Enemy enemyPrefab;

    //who to chase
    [SerializeField] GameObject chaseTarget;

    //how long to wait before spawning, how long between enemies
    [SerializeField] float waitSeconds = 4f;
    [SerializeField] float betweenEnemies = 5f;

    private void Start()
    {
        chaseTarget = GameObject.Find("ChaseTarget");
        //SpawnEnemy();

        //StartCoroutine(SpawnEnemy());
    }
    
    IEnumerator SpawnEnemy()
    {
        // === get acces to spawnpoints ===

        // == method ==
        //var possibleLocations = spawnPoints.GetSpawnPoint();


        //property
        var possibleLocations = spawnPoints.SpawnPoints;

        // chose a random spawn point
        while (true)
        {
            int randomIndex = Random.Range(0, possibleLocations.Count);
            var chosenSpawnPoint = possibleLocations[randomIndex];

            // spawn at chosen position
            Enemy newEnemy = Instantiate(enemyPrefab, chosenSpawnPoint.position, Quaternion.identity);
            newEnemy.SetChaseTarget(chaseTarget);

            //delay
            yield return new WaitForSeconds(betweenEnemies);
        }
    
        
    }
    
    // == old code ==
    //===============

    //IEnumerator enemySpawnDelay(float startDelay, float delayBetweenEnemies)
    //{
    //    yield return new WaitForSeconds(startDelay);

    //    while (true)
    //    {
    //        SpawnEnemy();
    //        yield return new WaitForSeconds(delayBetweenEnemies);
    //    }



    //}


    //IEnumerator Pause(float delay)
    //{
    //    Debug.Log("starting pause");
    //    yield return new WaitForSeconds(delay);
    //    Debug.Log("pause is over");
    //}

    
}
