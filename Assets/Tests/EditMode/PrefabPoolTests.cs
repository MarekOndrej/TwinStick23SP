using NUnit.Framework;
using UnityEngine;

// Edit-mode tests for PrefabPool. Constructs a throwaway primitive cube as the
// "prefab" so the tests don't depend on any project asset.
public class PrefabPoolTests
{
    GameObject prefab;

    [SetUp]
    public void Setup()
    {
        prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        prefab.SetActive(false); // act as a template, not a scene instance
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up scene-side pool root + any pool instances.
        var root = GameObject.Find("[PrefabPools]");
        if (root != null) Object.DestroyImmediate(root);
        if (prefab != null) Object.DestroyImmediate(prefab);
    }

    [Test]
    public void For_ReturnsSamePoolForSamePrefab()
    {
        var a = PrefabPool.For(prefab);
        var b = PrefabPool.For(prefab);
        Assert.AreSame(a, b);
    }

    [Test]
    public void Get_ReturnsActiveInstanceAtRequestedPose()
    {
        var pool = PrefabPool.For(prefab);
        var pos = new Vector3(1, 2, 3);
        var rot = Quaternion.Euler(0, 45, 0);

        var instance = pool.Get(pos, rot);

        Assert.IsNotNull(instance);
        Assert.IsTrue(instance.activeSelf, "Got instance should be active");
        Assert.AreEqual(pos, instance.transform.position);
        // Quaternion equality is fuzzy by design.
        Assert.IsTrue(Quaternion.Angle(rot, instance.transform.rotation) < 0.01f);
    }

    [Test]
    public void Release_DeactivatesAndCachesForReuse()
    {
        var pool = PrefabPool.For(prefab);
        var first = pool.Get(Vector3.zero, Quaternion.identity);

        Assert.AreEqual(1, pool.CountActive);
        Assert.AreEqual(0, pool.CountInactive);

        pool.Release(first);

        Assert.AreEqual(0, pool.CountActive);
        Assert.AreEqual(1, pool.CountInactive);
        Assert.IsFalse(first.activeSelf, "Released instance should be inactive");
    }

    [Test]
    public void Get_AfterRelease_ReusesSameInstance()
    {
        var pool = PrefabPool.For(prefab);
        var first = pool.Get(Vector3.zero, Quaternion.identity);
        pool.Release(first);
        var second = pool.Get(Vector3.one, Quaternion.identity);

        Assert.AreSame(first, second,
            "After release, the next Get should hand back the same instance.");
    }
}
