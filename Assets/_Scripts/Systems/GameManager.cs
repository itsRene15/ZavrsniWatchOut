using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scenes")] 
    [SerializeField] private List<string> levelScenes = new List<string>();
    
    [Header("Audio")]
    [SerializeField] private AudioMixer mainMixer;
    
    [Header("Progression")]
    [SerializeField] private bool autoLoadNextOnLevelComplete = true;
    [SerializeField] private bool autoReturnToLevelSelectOnWorldComplete = true;

    public const int SubLevelsPerWorld = 5;

    [Header("Pause")]
    public UnityEvent<bool> OnPauseChanged;

    [Header("Stats")]
    public UnityEvent<int> OnDeathsChanged;

    public bool IsPaused { get; private set; }
    public int CurrentLevelIndex { get; private set; } = -1;
    public int MaxUnlockedLevel { get; private set; } = 0;
    public int TotalDeaths { get; private set; } = 0;

    // Set up singleton and load saved data
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadProgress();
        LoadAudioSettings();
        LoadDeaths();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Unsubscribe when destroyed
    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // Update current level index and clear pause on scene load
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var n = scene.name;
        CurrentLevelIndex = levelScenes.IndexOf(n);
        if (IsPaused) SetPaused(false);
    }

  
    public void LoadMainMenu()
    {
        LoadByNameSafe(GameConstants.Scenes.MainMenu);
    }

  
    public void LoadSettings()
    {
        LoadByNameSafe(GameConstants.Scenes.Settings);
    }


    public void LoadLevelSelect()
    {
        LoadByNameSafe(GameConstants.Scenes.LevelSelect);
    }

    // Get number of worlds
    public int GetWorldCount()
    {
        if (SubLevelsPerWorld <= 0) return 0;
        return (levelScenes.Count + (SubLevelsPerWorld - 1)) / SubLevelsPerWorld;
    }

    // Get the first level index of a world
    public int GetWorldStartIndex(int worldIndex)
    {
        return Mathf.Clamp(worldIndex, 0, Mathf.Max(0, GetWorldCount() - 1)) * SubLevelsPerWorld;
    }

    // Get the last level index (inclusive) of a world
    public int GetWorldEndIndexInclusive(int worldIndex)
    {
        int start = worldIndex * SubLevelsPerWorld;
        int endExclusive = Mathf.Min(levelScenes.Count, start + SubLevelsPerWorld);
        return Mathf.Max(0, endExclusive - 1);
    }

    // Check if a world is unlocked
    public bool IsWorldUnlocked(int worldIndex)
    {
        if (worldIndex < 0) return false;
        int start = worldIndex * SubLevelsPerWorld;
        return start <= MaxUnlockedLevel && start < levelScenes.Count;
    }

    // Check if a sub-level is unlocked
    public bool IsSubLevelUnlocked(int worldIndex, int subLevelIndex)
    {
        if (worldIndex < 0 || subLevelIndex < 0 || subLevelIndex >= SubLevelsPerWorld) return false;
        int linear = worldIndex * SubLevelsPerWorld + subLevelIndex;
        if (!IsValidLevelIndex(linear)) return false;
        return linear <= MaxUnlockedLevel;
    }

    // Load first sub-level of a world
    public void LoadFirstSubLevelOfWorld(int worldIndex)
    {
        int start = worldIndex * SubLevelsPerWorld;
        if (!IsValidLevelIndex(start))
        {
            Debug.LogWarning($"[GameManager] World {worldIndex} has no valid starting level.");
            return;
        }
        if (!IsWorldUnlocked(worldIndex))
        {
            Debug.LogWarning($"[GameManager] World {worldIndex} is locked.");
            return;
        }
        LoadLevelByIndex(start);
    }

    // Load a sub-level by world and sub index
    public void LoadSubLevel(int worldIndex, int subLevelIndex)
    {
        int linear = worldIndex * SubLevelsPerWorld + subLevelIndex;
        LoadLevelByIndex(linear);
    }

    // Load a level by its linear index
    public void LoadLevelByIndex(int index)
    {
        if (!IsValidLevelIndex(index))
        {
            Debug.LogWarning($"[GameManager] Invalid level index {index}");
            return;
        }
        if (index > MaxUnlockedLevel)
        {
            Debug.LogWarning($"[GameManager] Level {index} is locked (max unlocked {MaxUnlockedLevel})");
            return;
        }
        LoadByNameSafe(levelScenes[index]);
    }

    // Restart current level
    public void RestartLevel() 
    {
        if (CurrentLevelIndex >= 0 && IsValidLevelIndex(CurrentLevelIndex))
            LoadByNameSafe(levelScenes[CurrentLevelIndex]);
    }

    // Load the next level or go to select/menu
    public void LoadNextLevel()
    {
        if (CurrentLevelIndex < 0) return;
        int next = CurrentLevelIndex + 1;
        if (IsValidLevelIndex(next))
        {
            LoadByNameSafe(levelScenes[next]);
        }
        else
        {
            if (!string.IsNullOrEmpty(GameConstants.Scenes.LevelSelect)) LoadLevelSelect();
            else LoadMainMenu();
        }
    }


    public void QuitToDesktop()
    {
        Application.Quit();
    }

    // Mark level complete and unlock next
    public void CompleteLevel()
    {
        if (CurrentLevelIndex >= 0)
        {
            if (CurrentLevelIndex > MaxUnlockedLevel)
            {
                MaxUnlockedLevel = CurrentLevelIndex;
                SaveProgress();
            }
            int unlockNext = CurrentLevelIndex + 1;
            if (IsValidLevelIndex(unlockNext) && unlockNext > MaxUnlockedLevel)
            {
                MaxUnlockedLevel = unlockNext;
                SaveProgress();
            }
        }

        if (CurrentLevelIndex >= 0)
        {
            int worldIndex = CurrentLevelIndex / SubLevelsPerWorld;
            int worldEnd = GetWorldEndIndexInclusive(worldIndex);
            bool finishedWorld = CurrentLevelIndex >= worldEnd;

            if (finishedWorld)
            {
                if (autoReturnToLevelSelectOnWorldComplete)
                {
                    LoadLevelSelect();
                    return;
                }
            }
        }

        if (autoLoadNextOnLevelComplete)
        {
            LoadNextLevel();
        }
    }

   
    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }
    
    public void SetPaused(bool paused)
    {
        if (IsPaused == paused) return;
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        OnPauseChanged?.Invoke(IsPaused);
    }


    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(GameConstants.Prefs.MasterVol, 1f);
    }

 
    public void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0.0001f, 1f);
        float db = Mathf.Log10(volume) * 20;

        if (mainMixer != null)
        {
            mainMixer.SetFloat("MasterVolume", db);
        }

        // Also update AudioListener volume for immediate effect if mixer is not used or to ensure global volume
        AudioListener.volume = volume;

        PlayerPrefs.SetFloat(GameConstants.Prefs.MasterVol, volume);
        PlayerPrefs.Save();
    }


    private void LoadAudioSettings()
    {
        float volume = GetMasterVolume();
        AudioListener.volume = volume;

        if (mainMixer != null)
        {
            float db = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20;
            mainMixer.SetFloat("MasterVolume", db);
        }
    }

    // Reset all progress and deaths
    public void ResetProgress()
    {
        MaxUnlockedLevel = 0;
        PlayerPrefs.DeleteKey(GameConstants.Prefs.MaxUnlocked);
        ResetDeaths();
        PlayerPrefs.Save();
        Debug.Log("Progress and death counter reset");
    }

    // Save max unlocked level
    private void SaveProgress()
    {
        PlayerPrefs.SetInt(GameConstants.Prefs.MaxUnlocked, Mathf.Clamp(MaxUnlockedLevel, 0, Mathf.Max(0, levelScenes.Count - 1)));
        PlayerPrefs.Save();
    }

    // Load max unlocked level
    private void LoadProgress()
    {
        MaxUnlockedLevel = PlayerPrefs.GetInt(GameConstants.Prefs.MaxUnlocked, 0);
        MaxUnlockedLevel = Mathf.Clamp(MaxUnlockedLevel, 0, Mathf.Max(0, levelScenes.Count - 1));
    }

    // Add to total deaths and notify
    public void AddDeath(int amount = 1)
    {
        if (amount <= 0) return;
        TotalDeaths = Mathf.Max(0, TotalDeaths + amount);
        SaveDeaths();
        OnDeathsChanged?.Invoke(TotalDeaths);
    }

    // Reset deaths to zero and notify
    public void ResetDeaths()
    {
        TotalDeaths = 0;
        SaveDeaths();
        OnDeathsChanged?.Invoke(TotalDeaths);
    }

    public int GetTotalDeaths() => TotalDeaths;

    // Save total deaths
    private void SaveDeaths()
    {
        PlayerPrefs.SetInt(GameConstants.Prefs.TotalDeaths, Mathf.Max(0, TotalDeaths));
        PlayerPrefs.Save();
    }

    // Load total deaths
    private void LoadDeaths()
    {
        TotalDeaths = PlayerPrefs.GetInt(GameConstants.Prefs.TotalDeaths, 0);
        TotalDeaths = Mathf.Max(0, TotalDeaths);
    }

    // Check if level index is valid
    private bool IsValidLevelIndex(int index)
    {
        return index >= 0 && index < levelScenes.Count && !string.IsNullOrEmpty(levelScenes[index]);
    }

    // Load a scene by name if available
    private void LoadByNameSafe(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[GameManager] Scene name is empty");
            return;
        }
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogWarning($"[GameManager] Scene '{sceneName}' is not in Build Settings.");
            return;
        }
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
