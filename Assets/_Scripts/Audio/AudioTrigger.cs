using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    public AudioClip sceneMusic;

    // Start music for this scene
    void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(sceneMusic);
        }
    }
}