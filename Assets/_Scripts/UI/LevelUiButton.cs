using UnityEngine;

public class LevelUIButton : MonoBehaviour
{
    // Go back to level select
    public void OnBackButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPaused(false);
            GameManager.Instance.LoadLevelSelect();
        }
        else
        {
            Debug.LogError("GameManager not found! Start from MainMenu.");
        }
    }
}