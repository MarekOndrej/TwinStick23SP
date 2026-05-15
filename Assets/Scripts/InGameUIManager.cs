using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    [Header("Hud Screen")]
    [SerializeField] GameObject hudScreen;
    [SerializeField] HealthBar healthBar;
    [SerializeField] TMP_Text scoreLabel;

    [Header("Pause Screen")]
    [SerializeField] GameObject pauseScreen;
    [SerializeField] Button pauseResumeButton;
    [SerializeField] Button pauseMainMenuButton;

    [Header("Game Over Screen")]
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] TMP_Text gameOverScoreLabel;
    [SerializeField] Button gameOverRestartButton;
    [SerializeField] Button gameOverMainMenuButton;


    EventManagerSO eventManager;

    private void Awake()
    {
        eventManager = Resources.Load<EventManagerSO>("EventManager");

        // Force GameSession to materialize and subscribe FIRST so its handler
        // updates CurrentScore before our handler reads it.
        _ = GameSession.Instance;
    }

    private void OnEnable()
    {
        eventManager.onPlayerHealthChanged += UpdateHealthBar;
        eventManager.onGameOver += GameOver;
        eventManager.onGamePaused += PauseGame;
        eventManager.onGameResumed += ResumeGame;
        eventManager.onScoreChanged += HandleScoreChanged;

        if (pauseResumeButton != null) pauseResumeButton.onClick.AddListener(OnResumeClicked);
        if (pauseMainMenuButton != null) pauseMainMenuButton.onClick.AddListener(OnQuitToMenuClicked);
        if (gameOverRestartButton != null) gameOverRestartButton.onClick.AddListener(OnRestartClicked);
        if (gameOverMainMenuButton != null) gameOverMainMenuButton.onClick.AddListener(OnQuitToMenuClicked);
    }

    private void OnDisable()
    {
        eventManager.onPlayerHealthChanged -= UpdateHealthBar;
        eventManager.onGameOver -= GameOver;
        eventManager.onGamePaused -= PauseGame;
        eventManager.onGameResumed -= ResumeGame;
        eventManager.onScoreChanged -= HandleScoreChanged;

        if (pauseResumeButton != null) pauseResumeButton.onClick.RemoveListener(OnResumeClicked);
        if (pauseMainMenuButton != null) pauseMainMenuButton.onClick.RemoveListener(OnQuitToMenuClicked);
        if (gameOverRestartButton != null) gameOverRestartButton.onClick.RemoveListener(OnRestartClicked);
        if (gameOverMainMenuButton != null) gameOverMainMenuButton.onClick.RemoveListener(OnQuitToMenuClicked);
    }

    private void Start()
    {
        DisplayScreen(hudScreen);
        HideScreen(pauseScreen);
        HideScreen(gameOverScreen);
        RefreshScoreLabel();
    }

    private void UpdateHealthBar(float current, float max)
    {
        if (healthBar != null) healthBar.UpdateHealthBar(current, max);
    }

    private void HandleScoreChanged(int newScore)
    {
        if (scoreLabel != null) scoreLabel.text = $"Score: {newScore}";
    }

    private void RefreshScoreLabel()
    {
        if (scoreLabel != null)
            scoreLabel.text = $"Score: {GameSession.Instance.CurrentScore}";
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
        HideScreen(pauseScreen);

        if (gameOverScoreLabel != null)
            gameOverScoreLabel.text = $"Score: {GameSession.Instance.CurrentScore}\nHigh Score: {GameSession.Instance.HighScore}";
    }

    // Button handlers
    private void OnResumeClicked()
    {
        eventManager.GameResumed();
    }

    private void OnRestartClicked()
    {
        GameSession.Instance.RestartGame();
    }

    private void OnQuitToMenuClicked()
    {
        GameSession.Instance.ReturnToMainMenu();
    }


    private void DisplayScreen(GameObject screen)
    {
        if (screen != null) screen.SetActive(true);
    }

    private void HideScreen(GameObject screen)
    {
        if (screen != null) screen.SetActive(false);
    }
}
