using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] Projectile projectile;     //whatShoot
    [SerializeField] Transform firingPoint;    //whereFrom

    [SerializeField] float fireDelay = 1f;

    public bool WantsToFire = false;

    public float RecoilAmount => recoilAmount;
    [SerializeField] float recoilAmount = 0.3f;

    EventManagerSO eventManager;
    double _nextAttackTime;

    private void Awake()
    {
        eventManager = Resources.Load<EventManagerSO>("EventManager");
    }

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
        if (projectile == null || firingPoint == null) return;

        Instantiate(projectile, firingPoint.position, firingPoint.rotation);

        // Notify listeners (player recoil, audio, muzzle FX, etc.)
        if (eventManager != null) eventManager.GunFired();
    }
}
