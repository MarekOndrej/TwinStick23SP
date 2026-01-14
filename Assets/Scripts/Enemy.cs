using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    //[SerializeField] Vector3 destination;
    [SerializeField] Transform chaseTarget;
    [SerializeField] float speed = 0f;
    private void Update()
    {
        //transform.position = destination;
        


        // gradual position update
        transform.position = Vector3.MoveTowards(
            transform.position,         //where from
            chaseTarget.position,       //where to
            speed * Time.deltaTime);    //how fast
    }
}