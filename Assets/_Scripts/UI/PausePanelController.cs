using UnityEngine;
using UnityEngine.InputSystem;

public class PausePanelController : MonoBehaviour
{
    [Header("Panel Root")]
    public GameObject panelRoot;

    [Header("Options")]
    public bool disablePlayerInputWhenPaused = true;

    private PlayerInput[] inputs;

    // Hide panel on start
    private void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    // Toggle pause state
    public void TogglePause()
    {
        if (panelRoot == null) return;
        bool show = !panelRoot.activeSelf;
        SetPause(show);
    }

    // Open pause panel
    public void OpenPause()
    {
        SetPause(true);
    }

    // Continue game
    public void Continue()
    {
        SetPause(false);
    }

    // Go back to level select
    public void BackToLevelSelect()
    {
        if (GameManager.Instance != null)
        {
            SetPause(false);
            GameManager.Instance.LoadLevelSelect();
        }
    }

    // Restart current level
    public void ResetLevel()
    {
        if (GameManager.Instance != null)
        {
            SetPause(false);
            GameManager.Instance.RestartLevel();
        }
    }

    // Apply pause and manage input
    private void SetPause(bool paused)
    {
        if (panelRoot != null) panelRoot.SetActive(paused);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPaused(paused);
        }

        if (disablePlayerInputWhenPaused)
        {
            if (inputs == null || inputs.Length == 0)
                inputs = UnityEngine.Object.FindObjectsByType<PlayerInput>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var pi in inputs)
            {
                if (pi == null) continue;
                pi.enabled = !paused;
            }
        }
    }
}
