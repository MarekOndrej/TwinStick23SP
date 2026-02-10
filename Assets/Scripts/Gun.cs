using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class Gun : MonoBehaviour
{
    [SerializeField] Projectile projectile;     //whatShoot
    [SerializeField] Transform firingPoint;    //whereFrom

    [SerializeField] float fireDelay = 1f;

    public bool WantsToFire = false;

    double _nextAttackTime;
    private void Update()
    {
        if (!WantsToFire) return;
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
