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
}
