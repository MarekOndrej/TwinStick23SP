using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Managers/EventManager", fileName = "EventManager")]
public class EventManagerSO : ScriptableObject
{
    // === EVENT MANAGER ===
    // Relays messages between scripts



    // == Event actions (the messages) ==
    public event Action onZoneTriggered;
    public event Action onGameOver;
    public event Action<float, float> onPlayerHealthChanged;
    public event Action onGamePaused;
    public event Action onGameResumed;
    public event Action onGunFired;
    public event Action<int> onEnemyDefeated;
    public event Action<int> onScoreChanged;
    public event Action<int> onWaveStarted;
    public event Action<int> onWaveCleared;
    public event Action onAllWavesCleared;


    // == Methods (sending of messages) ==
    public void ZoneTriggered()
    {
        Debug.Log("Zone triggered somewhere!");
        onZoneTriggered?.Invoke();
    }

    public void GameOver()
    {
        Debug.Log("The game is over!");
        onGameOver?.Invoke();
    }

    public void PlayerHealthChanged(float playerCurrentHealth, float playerMaxHealth)
    {
        onPlayerHealthChanged?.Invoke(playerCurrentHealth, playerMaxHealth);
    }

    public void GamePaused()
    {
        onGamePaused?.Invoke();
    }

    public void GameResumed()
    {
        onGameResumed?.Invoke();
    }

    public void GunFired()
    {
        onGunFired?.Invoke();
    }

    public void EnemyDefeated(int scoreValue)
    {
        onEnemyDefeated?.Invoke(scoreValue);
    }

    public void ScoreChanged(int newScore)
    {
        onScoreChanged?.Invoke(newScore);
    }

    public void WaveStarted(int waveNumber)
    {
        Debug.Log($"Wave {waveNumber} started!");
        onWaveStarted?.Invoke(waveNumber);
    }

    public void WaveCleared(int waveNumber)
    {
        Debug.Log($"Wave {waveNumber} cleared!");
        onWaveCleared?.Invoke(waveNumber);
    }

    public void AllWavesCleared()
    {
        Debug.Log("All waves cleared!");
        onAllWavesCleared?.Invoke();
    }

    // Clear all event delegates. Useful because ScriptableObject state can survive
    // between play-mode sessions when Domain Reload is disabled — stale subscribers
    // would otherwise be invoked and NRE.
    public void ClearAllSubscribers()
    {
        onZoneTriggered = null;
        onGameOver = null;
        onPlayerHealthChanged = null;
        onGamePaused = null;
        onGameResumed = null;
        onGunFired = null;
        onEnemyDefeated = null;
        onScoreChanged = null;
        onWaveStarted = null;
        onWaveCleared = null;
        onAllWavesCleared = null;
    }

    // Runs once at the start of every play session, before any scene loads.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        var instance = Resources.Load<EventManagerSO>("EventManager");
        if (instance != null) instance.ClearAllSubscribers();
    }
}
