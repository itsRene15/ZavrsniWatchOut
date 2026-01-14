using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuScript : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider volumeSlider;

    // Set up slider and load saved volume
    private void Start()
    {
        if (volumeSlider == null)
        {
            Debug.LogWarning("SettingsMenu: Volume Slider is not assigned in the Inspector!");
            return;
        }

        float currentVolume = 1f;
        if (GameManager.Instance != null)
        {
            currentVolume = GameManager.Instance.GetMasterVolume();
        }

        volumeSlider.value = currentVolume;
        volumeSlider.onValueChanged.AddListener(HandleVolumeChange);
    }

    // Apply volume change
    private void HandleVolumeChange(float value)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMasterVolume(value);
        }
    }
}