using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    //[SerializeField] Vector3 destination;

    //[SerializeField] float speed = 0f;

    [SerializeField] Transform chaseTarget;

    NavMeshAgent agent;

    

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        //chaseTarget = GameObject.Find("ChaseTarget");
        
    }
    private void Update()
    {
        // Update agent's destination

        agent.destination = chaseTarget.transform.position;
    }


    public void SetChaseTarget(GameObject newChaseTarget)
    {
        chaseTarget = newChaseTarget.transform;
    }
}