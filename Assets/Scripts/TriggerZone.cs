using UnityEngine;

public class TriggerZone : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("ChaseTarget"))
        {
            // player detected!


        }
    }
}
