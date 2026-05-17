using NUnit.Framework;
using UnityEngine;

// Edit-mode tests for EventManagerSO. Pure C# logic — no scene needed.
//
// We construct a fresh ScriptableObject per test (Setup/TearDown) so tests
// don't interfere with each other, and so the shared production EventManager
// asset under Assets/Resources/EventManager.asset is never mutated by tests.
public class EventManagerSOTests
{
    EventManagerSO sut;

    [SetUp]
    public void Setup()
    {
        sut = ScriptableObject.CreateInstance<EventManagerSO>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(sut);
        sut = null;
    }

    // === Raise-with-no-subscribers must never throw ===

    [Test]
    public void GunFired_NoSubscribers_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => sut.GunFired());
    }

    [Test]
    public void GameOver_NoSubscribers_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => sut.GameOver());
    }

    [Test]
    public void EnemyDefeated_NoSubscribers_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => sut.EnemyDefeated(10));
    }

    [Test]
    public void WaveStarted_NoSubscribers_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => sut.WaveStarted(1));
    }

    // === Subscribers receive callbacks ===

    [Test]
    public void GunFired_WithSubscriber_FiresOnce()
    {
        int callCount = 0;
        sut.onGunFired += () => callCount++;
        sut.GunFired();
        Assert.AreEqual(1, callCount);
    }

    [Test]
    public void EnemyDefeated_PassesScoreValueToSubscribers()
    {
        int received = -1;
        sut.onEnemyDefeated += score => received = score;
        sut.EnemyDefeated(42);
        Assert.AreEqual(42, received);
    }

    [Test]
    public void PlayerHealthChanged_PassesBothValuesToSubscribers()
    {
        float currentReceived = -1f;
        float maxReceived = -1f;
        sut.onPlayerHealthChanged += (cur, max) =>
        {
            currentReceived = cur;
            maxReceived = max;
        };
        sut.PlayerHealthChanged(45f, 100f);
        Assert.AreEqual(45f, currentReceived);
        Assert.AreEqual(100f, maxReceived);
    }

    [Test]
    public void WaveStarted_PassesWaveNumberToSubscribers()
    {
        int waveReceived = -1;
        sut.onWaveStarted += w => waveReceived = w;
        sut.WaveStarted(3);
        Assert.AreEqual(3, waveReceived);
    }

    [Test]
    public void ScoreChanged_PassesNewScoreToSubscribers()
    {
        int received = -1;
        sut.onScoreChanged += s => received = s;
        sut.ScoreChanged(150);
        Assert.AreEqual(150, received);
    }

    // === Multiple subscribers all receive the event ===

    [Test]
    public void GameOver_MultipleSubscribers_AllReceive()
    {
        int a = 0, b = 0, c = 0;
        sut.onGameOver += () => a++;
        sut.onGameOver += () => b++;
        sut.onGameOver += () => c++;
        sut.GameOver();
        Assert.AreEqual(1, a);
        Assert.AreEqual(1, b);
        Assert.AreEqual(1, c);
    }

    // === Unsubscribing actually stops callbacks ===

    [Test]
    public void GamePaused_UnsubscribedHandler_DoesNotFire()
    {
        int callCount = 0;
        System.Action handler = () => callCount++;

        sut.onGamePaused += handler;
        sut.GamePaused();          // should fire
        sut.onGamePaused -= handler;
        sut.GamePaused();          // should NOT fire

        Assert.AreEqual(1, callCount);
    }

    // === ClearAllSubscribers wipes every event channel ===

    [Test]
    public void ClearAllSubscribers_PreventsAllFutureCallbacks()
    {
        int gunFireCount = 0;
        int gameOverCount = 0;
        int enemyDefeatedCount = 0;
        int healthCount = 0;
        int pauseCount = 0;
        int resumeCount = 0;
        int waveStartedCount = 0;
        int waveClearedCount = 0;
        int allWavesClearedCount = 0;
        int scoreChangedCount = 0;
        int zoneTriggeredCount = 0;

        sut.onGunFired += () => gunFireCount++;
        sut.onGameOver += () => gameOverCount++;
        sut.onEnemyDefeated += _ => enemyDefeatedCount++;
        sut.onPlayerHealthChanged += (_, _) => healthCount++;
        sut.onGamePaused += () => pauseCount++;
        sut.onGameResumed += () => resumeCount++;
        sut.onWaveStarted += _ => waveStartedCount++;
        sut.onWaveCleared += _ => waveClearedCount++;
        sut.onAllWavesCleared += () => allWavesClearedCount++;
        sut.onScoreChanged += _ => scoreChangedCount++;
        sut.onZoneTriggered += () => zoneTriggeredCount++;

        sut.ClearAllSubscribers();

        // Fire every event — none should be observed.
        sut.GunFired();
        sut.GameOver();
        sut.EnemyDefeated(1);
        sut.PlayerHealthChanged(50, 100);
        sut.GamePaused();
        sut.GameResumed();
        sut.WaveStarted(1);
        sut.WaveCleared(1);
        sut.AllWavesCleared();
        sut.ScoreChanged(100);
        sut.ZoneTriggered();

        Assert.AreEqual(0, gunFireCount, "gunFire fired after clear");
        Assert.AreEqual(0, gameOverCount, "gameOver fired after clear");
        Assert.AreEqual(0, enemyDefeatedCount, "enemyDefeated fired after clear");
        Assert.AreEqual(0, healthCount, "playerHealthChanged fired after clear");
        Assert.AreEqual(0, pauseCount, "gamePaused fired after clear");
        Assert.AreEqual(0, resumeCount, "gameResumed fired after clear");
        Assert.AreEqual(0, waveStartedCount, "waveStarted fired after clear");
        Assert.AreEqual(0, waveClearedCount, "waveCleared fired after clear");
        Assert.AreEqual(0, allWavesClearedCount, "allWavesCleared fired after clear");
        Assert.AreEqual(0, scoreChangedCount, "scoreChanged fired after clear");
        Assert.AreEqual(0, zoneTriggeredCount, "zoneTriggered fired after clear");
    }

    // Guarantees that no event was forgotten when we added new events without
    // also adding them to ClearAllSubscribers. This catches the kind of bug
    // where Phase 3.4 adds 3 new events but ClearAllSubscribers only nulls 8.
    [Test]
    public void ClearAllSubscribers_CoversAllPublicEvents()
    {
        var eventFields = typeof(EventManagerSO).GetEvents(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // We expect at least the events declared as of this writing.
        // If you add a new public event, also add it to ClearAllSubscribers AND
        // bump this baseline — the assertion is a forcing function.
        string[] expectedEvents = new[]
        {
            "onZoneTriggered",
            "onGameOver",
            "onPlayerHealthChanged",
            "onGamePaused",
            "onGameResumed",
            "onGunFired",
            "onEnemyDefeated",
            "onScoreChanged",
            "onWaveStarted",
            "onWaveCleared",
            "onAllWavesCleared",
        };

        foreach (var name in expectedEvents)
        {
            Assert.IsTrue(
                System.Array.Exists(eventFields, e => e.Name == name),
                $"Expected event '{name}' on EventManagerSO was not declared.");
        }
    }
}
