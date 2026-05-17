using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// Lightweight per-prefab GameObject pool. Use PrefabPool.For(prefab) to lazily
// create / fetch the pool for any prefab, then call Get(pos, rot) instead of
// Instantiate and Release(go) instead of Destroy.
//
// Lifecycle:
//   - createFunc: Instantiate(prefab), SetActive(false)
//   - actionOnGet: SetActive(true)  -> OnEnable fires on all components
//   - then PrefabPool.Get calls IPoolable.OnTakenFromPool on each IPoolable
//     in the instance so it can reset per-life state (timers, velocity, etc).
//   - actionOnRelease: SetActive(false) -> OnDisable fires, instance reparented
//     under the pool GameObject for tidy hierarchy.
//
// All pools live as children of a single "[PrefabPools]" root GameObject for
// easy Editor inspection.
//
// Note: prefab references must be the actual asset (or a prefab variant), not
// scene instances — otherwise the pool would mutate the scene object.
public class PrefabPool : MonoBehaviour
{
    static readonly Dictionary<GameObject, PrefabPool> _pools = new();
    static Transform _rootTransform;

    // Reset statics every play session in case Domain Reload is disabled —
    // otherwise we'd keep stale pool references from the previous session.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        _pools.Clear();
        _rootTransform = null;
    }

    [SerializeField] GameObject prefab;
    [Tooltip("Initial pool size — instances created up front when the pool is " +
             "first asked for one beyond what's already cached.")]
    [SerializeField] int defaultCapacity = 16;
    [Tooltip("Maximum pool size. Releases beyond this destroy the instance " +
             "instead of caching it.")]
    [SerializeField] int maxSize = 256;

    ObjectPool<GameObject> _pool;

    public GameObject Prefab => prefab;
    public int CountInactive => _pool?.CountInactive ?? 0;
    public int CountActive => _pool?.CountActive ?? 0;

    public static PrefabPool For(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("PrefabPool.For called with null prefab.");
            return null;
        }

        // Unity's overloaded == treats destroyed pools as null; that's what we want.
        if (_pools.TryGetValue(prefab, out var existing) && existing != null)
        {
            return existing;
        }

        EnsureRoot();

        var poolGo = new GameObject($"Pool ({prefab.name})");
        poolGo.transform.SetParent(_rootTransform, worldPositionStays: false);
        var pool = poolGo.AddComponent<PrefabPool>();
        pool.prefab = prefab;
        pool.Init();
        _pools[prefab] = pool;
        return pool;
    }

    static void EnsureRoot()
    {
        if (_rootTransform != null) return;
        var rootGo = new GameObject("[PrefabPools]");
        _rootTransform = rootGo.transform;
    }

    void Awake()
    {
        if (_pool == null) Init();
    }

    void OnDestroy()
    {
        if (prefab != null && _pools.TryGetValue(prefab, out var existing) && existing == this)
        {
            _pools.Remove(prefab);
        }
        // Don't dispose _pool — Unity's ObjectPool just holds C# references to
        // GameObjects that are being torn down anyway as the scene unloads.
    }

    void Init()
    {
        _pool = new ObjectPool<GameObject>(
            createFunc: CreateInstance,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroyInstance,
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    GameObject CreateInstance()
    {
        var go = Instantiate(prefab);
        go.SetActive(false);
        return go;
    }

    void OnGet(GameObject go)
    {
        go.SetActive(true);
    }

    void OnRelease(GameObject go)
    {
        if (go == null) return;
        go.SetActive(false);
        if (transform != null) go.transform.SetParent(transform, worldPositionStays: false);
    }

    void OnDestroyInstance(GameObject go)
    {
        if (go != null) Destroy(go);
    }

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        if (_pool == null) Init();
        var go = _pool.Get();
        // Detach from the pool root so the active instance is at scene root
        // rather than nested under [PrefabPools].
        go.transform.SetParent(null, worldPositionStays: false);
        go.transform.SetPositionAndRotation(position, rotation);

        // Notify all IPoolable components so they can reset per-life state.
        // Cached allocation: GetComponentsInChildren allocates, but pooled
        // objects are short-lived churners so this still wins net vs Instantiate.
        var poolables = go.GetComponentsInChildren<IPoolable>(includeInactive: true);
        for (int i = 0; i < poolables.Length; i++)
        {
            poolables[i].OnTakenFromPool(this);
        }

        return go;
    }

    public void Release(GameObject instance)
    {
        if (instance == null) return;
        if (_pool == null) { Destroy(instance); return; }
        _pool.Release(instance);
    }
}
