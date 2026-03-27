using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnPoints : MonoBehaviour
{

    // modern way to interact with private code
    // property
    public List<Transform> SpawnPoints
    {
        get { return spawnPoints; }
        set { spawnPoints = value; }
    }

    [SerializeField] List<Transform> spawnPoints;

    private void Awake()
    {
        //initialize the list
        spawnPoints = new List<Transform>();

        //populate the list
        foreach (Transform child in transform)
        {
            spawnPoints.Add(child);
        }
    }

    
    //old way to interact with private code
    //method
   public List<Transform> GetSpawnPoint()
    {
        return spawnPoints;
    }


}
