// Implemented by any MonoBehaviour that wants to participate in PrefabPool
// reuse. The pool calls OnTakenFromPool exactly once per acquisition (after
// SetActive(true) has fired OnEnable on every component) so the implementer
// can reset per-flight / per-life state and remember a pool reference for
// later self-release.
public interface IPoolable
{
    void OnTakenFromPool(PrefabPool pool);
}
