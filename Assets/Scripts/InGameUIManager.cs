using UnityEngine;

public class InGameUIManager : MonoBehaviour
{
    [Header("Hud Screen")]
    [SerializeField] GameObject hudScreen;
    [SerializeField] HealthBar healthBar;

    [Header("Pause Screen")]
    [SerializeField] GameObject pauseScreen;

    [Header("Game Over Screen")]
    [SerializeField] GameObject gameOverScreen;


    EventManagerSO eventManager;

    private void Awake()
    {
        eventManager = Resources.Load<EventManagerSO>("EventManager");
    }

    private void OnEnable()
    {
        eventManager.onPlayerHealthChanged += UpdateHealthBar;
        eventManager.onGameOver += GameOver;
        eventManager.onGamePaused += PauseGame;
        eventManager.onGameResumed += ResumeGame;
    }

    private void OnDisable()
    {
        eventManager.onPlayerHealthChanged -= UpdateHealthBar;
        eventManager.onGameOver -= GameOver;
        eventManager.onGamePaused -= PauseGame;
        eventManager.onGameResumed -= ResumeGame;
    }

    private void Start()
    {
        DisplayScreen(hudScreen);
        HideScreen(pauseScreen);
        HideScreen(gameOverScreen);
    }

    private void UpdateHealthBar(float current, float max)
    {
        healthBar.UpdateHealthBar(current, max);
    }

    private void PauseGame()
    {
        HideScreen(hudScreen);
        DisplayScreen(pauseScreen);
    }

    private void ResumeGame()
    {
        HideScreen(pauseScreen);
        DisplayScreen(hudScreen);
    }

    private void GameOver()
    {

        DisplayScreen(gameOverScreen);
        HideScreen(hudScreen);
    }
    

    private void DisplayScreen(GameObject screen) => screen.SetActive(true);
    private void HideScreen(GameObject screen) => screen.SetActive(false);
    


}
