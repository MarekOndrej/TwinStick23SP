using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// Cross-scene singleton: owns the current run's score, persists the high score,
// and exposes the scene-flow operations (start / restart / quit-to-menu / quit).
public class GameSession : MonoBehaviour
{
    [SerializeField] string mainMenuSceneName = "MainMenu";
    [SerializeField] string gameSceneName = "SampleScene";

    public int CurrentScore { get; private set; }
    public int HighScore { get; private set; }

    public string MainMenuSceneName => mainMenuSceneName;
    public string GameSceneName => gameSceneName;

    const string SaveFileName = "save.json";

    static GameSession _instance;
    EventManagerSO eventManager;

    // Reset the static instance pointer at the start of every play session so
    // disabling Domain Reload doesn't leave us pointing at a destroyed object.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        _instance = null;
    }

    public static GameSession Instance
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = FindFirstObjectByType<GameSession>();
            if (_instance == null)
            {
                var go = new GameObject("GameSession (auto)");
                _instance = go.AddComponent<GameSession>();
            }
            return _instance;
        }
    }

    [System.Serializable]
    class SaveData
    {
        public int highScore;
    }

    string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        eventManager = Resources.Load<EventManagerSO>("EventManager");
        LoadHighScore();
    }

    private void OnEnable()
    {
        if (eventManager != null)
            eventManager.onEnemyDefeated += HandleEnemyDefeated;
    }

    private void OnDisable()
    {
        if (eventManager != null)
            eventManager.onEnemyDefeated -= HandleEnemyDefeated;
    }

    private void HandleEnemyDefeated(int scoreValue)
    {
        CurrentScore += scoreValue;
        if (CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
            SaveHighScore();
        }

        // Notify listeners (HUD, etc.) only AFTER CurrentScore has been updated,
        // so subscriber ordering on onEnemyDefeated doesn't matter.
        if (eventManager != null) eventManager.ScoreChanged(CurrentScore);
    }

    public void StartNewGame()
    {
        CurrentScore = 0;
        SceneManager.LoadScene(gameSceneName);
    }

    public void RestartGame()
    {
        CurrentScore = 0;
        SceneManager.LoadScene(gameSceneName);
    }

    public void ReturnToMainMenu()
    {
        CurrentScore = 0;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void LoadHighScore()
    {
        try
        {
            if (!File.Exists(SavePath)) return;
            string json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data != null) HighScore = Mathf.Max(0, data.highScore);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"GameSession: failed to load high score — {e.Message}");
        }
    }

    private void SaveHighScore()
    {
        try
        {
            var data = new SaveData { highScore = HighScore };
            File.WriteAllText(SavePath, JsonUtility.ToJson(data));
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"GameSession: failed to save high score — {e.Message}");
        }
    }
}
