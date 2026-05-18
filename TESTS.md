# Tests in TwinStick23SP

⚠ **Status: Not yet wired up. See "Why deferred" below.**

The plan is to keep a small edit-mode test suite under `Assets/Tests/EditMode/`
covering the most-shared foundation code:

- `EventManagerSOTests.cs` — covers the ScriptableObject event bus: no-throw
  on empty events, single + multiple subscriber dispatch, unsubscribe,
  `ClearAllSubscribers()` correctness, and a reflection-based assertion that
  every public event is declared (so if you add a new event, you're forced
  to update this list and remember to wire it into `ClearAllSubscribers`).
- `PrefabPoolTests.cs` — covers the object pool: factory returns the same
  pool for the same prefab, `Get` produces an active instance at the right
  pose, `Release` returns the instance and deactivates it, and the next
  `Get` reuses the released instance.

Both files exist in git history (commit `b5dca39`, then reverted) and can
be brought back once the asmdef issue is properly solved.

## Why deferred

Unity test assemblies live in their own `.asmdef`. That asmdef needs to be
able to see the game scripts (`EventManagerSO`, `PrefabPool`, etc.) which
currently live in `Assembly-CSharp` (the default catch-all assembly because
the game scripts don't have their own `.asmdef`).

**Unity does not allow an asmdef to reference `Assembly-CSharp` directly.**
The canonical solution is to wrap the game scripts in their own asmdef
(e.g. `Gameplay.asmdef`), then have the test asmdef reference that.

I attempted to shortcut this by listing `"Assembly-CSharp"` in the test
asmdef's references — Unity rejected it with a compile error that put the
whole project into Safe Mode. The tests + asmdef have been removed to
restore compilation.

## How to set up properly (next time we revisit)

1. **Wrap the game in an asmdef.**
   Create `Assets/Scripts/Gameplay.asmdef` with appropriate references
   (UnityEngine.UI, TMPro, Unity.Cinemachine, Unity.InputSystem, etc.).
   Pick references carefully — all script dependencies must be listed.

2. **Recreate the test asmdef** under `Assets/Tests/EditMode/`:
   ```json
   {
       "name": "Tests.EditMode",
       "references": [
           "UnityEngine.TestRunner",
           "UnityEditor.TestRunner",
           "Gameplay"
       ],
       "includePlatforms": ["Editor"],
       "overrideReferences": true,
       "precompiledReferences": ["nunit.framework.dll"],
       "autoReferenced": false,
       "defineConstraints": ["UNITY_INCLUDE_TESTS"]
   }
   ```

3. **Restore the test files** from commit `b5dca39` (before the deletion):
   ```
   git checkout b5dca39 -- Assets/Tests/EditMode/EventManagerSOTests.cs
   git checkout b5dca39 -- Assets/Tests/EditMode/PrefabPoolTests.cs
   ```
   Then `git add` + commit.

4. **Open the project in Unity, verify it compiles, run Window → General → Test Runner**.

## How to run (once tests are back)

In the Unity Editor: **Window → General → Test Runner → EditMode tab → Run All**.

From the command line:
```
Unity.exe -batchmode -nographics -runTests -testPlatform EditMode \
  -projectPath . -testResults TestResults.xml -logFile -
```

## What I'd test next (rough priority)

1. **`LevelManager` state transitions** (play-mode) — would catch the
   `resumed` enum value bug class.
2. **`GameSession` score accumulation** (edit-mode).
3. **`Enemy.IsOutOfBounds` grace period** (play-mode) — guards the audit
   fix we did during ranged-enemy debugging.
4. **`Projectile.OnTakenFromPool` reset** — guards the pool contract.

Each is 15-30 lines; the test surface can grow organically.
