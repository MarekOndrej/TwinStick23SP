using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] Enemy enemyPrefab;

    [SerializeField] GameObject chaseTarget;

    [SerializeField] float waitSeconds = 4f;
    [SerializeField] float betweenEnemies = 5f;

    private void Start()
    {
        chaseTarget = GameObject.Find("ChaseTarget");
        SpawnEnemy();

        StartCoroutine(enemySpawnDelay(waitSeconds, betweenEnemies));
    }

    private void SpawnEnemy()
    {
        Enemy newEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        newEnemy.SetChaseTarget(chaseTarget);
    }

    IEnumerator enemySpawnDelay(float startDelay, float delayBetweenEnemies)
    {
        yield return new WaitForSeconds(startDelay);


        SpawnEnemy();

        yield return new WaitForSeconds(delayBetweenEnemies);

        SpawnEnemy();

    }


    //IEnumerator Pause(float delay)
    //{
    //    Debug.Log("starting pause");
    //    yield return new WaitForSeconds(delay);
    //    Debug.Log("pause is over");
    //}
}
