using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;

    [SerializeField] private Slider volumeSlider;

    // Set up UI and volume slider
    private void Start()
    {
        if (pausePanel != null) 
            pausePanel.SetActive(false);

        if (volumeSlider != null && GameManager.Instance != null)
        {
            volumeSlider.value = GameManager.Instance.GetMasterVolume();
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    // Toggle pause with Escape
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TogglePause();
                UpdateUI(GameManager.Instance.IsPaused);
            }
        }
    }

    // Resume from pause
    public void ResumeGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPaused(false);
            UpdateUI(false);
        }
    }

    // Quit the game
    public void QuitGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitToDesktop();
        }
    }

    // Open pause menu from UI button
    public void OpenPauseMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPaused(true);
            UpdateUI(true);
        }
    }

    // Called when volume slider changes
    private void OnVolumeChanged(float value)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMasterVolume(value);
        }
    }

    // Show or hide pause UI
    private void UpdateUI(bool isPaused)
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }
    }
}