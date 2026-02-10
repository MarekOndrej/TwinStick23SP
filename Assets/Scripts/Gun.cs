using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] Projectile projectile;     //whatShoot
    [SerializeField] Transform firingPoint;    //whereFrom

    [SerializeField] float fireDelay = 1f;

    double _nextAttackTime;
    private void Update()
    {
        double now = Time.timeAsDouble;

        if (now >= _nextAttackTime)
        {
            SpawnProjectile();
            _nextAttackTime = now + fireDelay;
        }



        
    }
    private void SpawnProjectile()
    {


        Debug.Log(projectile);

        Instantiate(projectile, firingPoint.position, firingPoint.rotation);
    }
}
