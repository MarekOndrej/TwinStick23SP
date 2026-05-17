# Tests in TwinStick23SP

Unity-Test-Framework edit-mode tests live in `Assets/Tests/EditMode/` under
the `Tests.EditMode` assembly. Currently:

- `EventManagerSOTests.cs` — covers the ScriptableObject event bus: no-throw
  on empty events, single + multiple subscriber dispatch, unsubscribe,
  `ClearAllSubscribers()` correctness, and a reflection-based assertion that
  every public event is declared (so if you add a new event, you're forced
  to update this list and remember to wire it into `ClearAllSubscribers`).
- `PrefabPoolTests.cs` — covers the object pool: factory returns the same
  pool for the same prefab, `Get` produces an active instance at the right
  pose, `Release` returns the instance and deactivates it, and the next
  `Get` reuses the released instance (the whole point of pooling).

Each test builds its own `ScriptableObject.CreateInstance<EventManagerSO>()`
or `GameObject.CreatePrimitive(Cube)` so tests are isolated and never mutate
shared project assets.

## How to run

Inside the Unity Editor:

1. Open the project.
2. **Window → General → Test Runner**.
3. Click the **EditMode** tab at the top.
4. Click **Run All**, or click an individual test name to run just that one.

All green checkmarks → all tests passed.

From the command line (CI or quick sanity sweep):

```
Unity.exe -batchmode -nographics -runTests -testPlatform EditMode \
  -projectPath . -testResults TestResults.xml -logFile -
```

(Adjust `Unity.exe` to your install path; on Windows it's typically
`C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe`.)

## Adding more tests

For pure-C# / ScriptableObject logic → edit-mode is cheap and fast. Drop a
`*.cs` file into `Assets/Tests/EditMode/` with `[Test]` methods, and Test
Runner picks them up automatically.

For things that need scene lifecycle (Awake/OnEnable/Update sequencing, Time,
physics) → use play-mode tests. Create `Assets/Tests/PlayMode/` with its own
`.asmdef` (`includePlatforms` empty + nothing in `excludePlatforms` so it
runs in both Editor and standalone). Use `[UnityTest]` with `IEnumerator`
returns instead of `[Test]`.

## What I'd test next (roughly in priority order)

1. **`LevelManager` state transitions** — play-mode test that creates a fresh
   scene with an EventManagerSO and a LevelManager, fires Pause/Resume/Game
   Over via the event bus, and asserts `CurrentGameState`. Would catch the
   class of bug that bit us early on (`resumed` enum value used in code but
   nowhere else).
2. **`GameSession` score accumulation** — instantiate a GameSession via
   `GameObject.AddComponent`, subscribe it to a fresh event manager, raise
   `EnemyDefeated(10)` three times, assert `CurrentScore == 30` and that
   `HighScore` was bumped.
3. **`Enemy.IsOutOfBounds` grace period** — verify that a freshly-Awoken
   enemy doesn't self-destruct during the grace window even if no navmesh
   is present. (Would catch the bug we fixed in the audit pass.)
4. **`Projectile.OnTakenFromPool` reset** — assert that timeAlive,
   bounceCounter, and rb.linearVelocity all reset cleanly when the
   projectile is reacquired from the pool.

Each of these is ~15-30 lines, so the test surface can grow organically as
new systems land.
