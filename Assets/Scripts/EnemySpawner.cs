using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] Enemy enemyPrefab;

    [SerializeField] GameObject chaseTarget;

    private void Start()
    {
        chaseTarget = GameObject.Find("ChaseTarget");
        Enemy newEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        newEnemy.SetChaseTarget(chaseTarget);
    }


}
