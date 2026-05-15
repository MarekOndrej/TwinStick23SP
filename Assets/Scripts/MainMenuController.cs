using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Attach to the root of the Main Menu canvas. Wire the buttons and the
// high-score label in the inspector; click handlers are bound at runtime.
public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] Button playButton;
    [SerializeField] Button quitButton;

    [Header("Labels")]
    [SerializeField] TMP_Text highScoreLabel;

    private void OnEnable()
    {
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
        RefreshHighScore();
    }

    private void OnDisable()
    {
        if (playButton != null) playButton.onClick.RemoveListener(OnPlayClicked);
        if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    private void RefreshHighScore()
    {
        if (highScoreLabel == null) return;
        highScoreLabel.text = $"High Score: {GameSession.Instance.HighScore}";
    }

    public void OnPlayClicked()
    {
        GameSession.Instance.StartNewGame();
    }

    public void OnQuitClicked()
    {
        GameSession.Instance.QuitGame();
    }
}
