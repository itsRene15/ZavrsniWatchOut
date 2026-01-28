using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Settings UI")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    private void Start()
    {
        // Ensure only main panel is active at start
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Initialize volume slider
        if (volumeSlider != null && GameManager.Instance != null)
        {
            volumeSlider.value = GameManager.Instance.GetMasterVolume();
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        // Initialize fullscreen toggle
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }
    }

    // --- Main Menu Actions ---

    public void StartGame()
    {
        if (GameManager.Instance != null)
        {
            // Usually we go to Level Select or the first level
            GameManager.Instance.LoadLevelSelect();
        }
        else
        {
            SceneManager.LoadScene(GameConstants.Scenes.LevelSelect);
        }
    }

    public void OpenSettings()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitToDesktop();
        }
        else
        {
            Application.Quit();
        }
    }

    // --- Settings Actions ---

    public void BackToMainMenu()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void OnVolumeChanged(float value)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMasterVolume(value);
        }
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}
