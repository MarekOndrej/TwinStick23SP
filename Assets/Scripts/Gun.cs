using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] Projectile projectile;     //whatShoot
    [SerializeField] Transform firingPoint;    //whereFrom


    private void Update()
    {
        SpawnProjectile();
    }
    private void SpawnProjectile()
    {


        Debug.Log(projectile);

        Instantiate(projectile, firingPoint.position, firingPoint.rotation);
    }
}
