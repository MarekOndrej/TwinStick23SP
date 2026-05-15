using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameState CurrentGameState => currentGameState;
    [SerializeField] GameState currentGameState;

    EventManagerSO eventManager;

    private void Awake()
    {
        eventManager = Resources.Load<EventManagerSO>("EventManager");
    }

    private void OnEnable()
    {
        eventManager.onGameOver += GameOver;
        eventManager.onGamePaused += GamePaused;
        //eventManager.onGameResumed += GameResumed;
    }
    private void OnDisable()
    {
        eventManager.onGameOver -= GameOver;
        eventManager.onGamePaused -= GamePaused;
        //eventManager.onGameResumed -= GameResumed;
    }

    private void Start()
    {
        currentGameState = GameState.running; 
    }
    private void GamePaused()
    {
        currentGameState = GameState.paused;
    }
    /*private void GameResumed()
    {
        currentGameState = GameState.resumed;
    }*/
    private void GameOver()
    {
        currentGameState = GameState.gameOver;
    }
}
