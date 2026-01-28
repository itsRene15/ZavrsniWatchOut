using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class DeathGround : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] private bool oneShot = true;

    [SerializeField] private float deathDelay = 0.15f;

    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private float deathVolume = 1f;

    private bool fired;

    // Make collider a trigger by default
    private void Reset()
    {
        var col2d = GetComponent<Collider2D>();
        if (col2d) col2d.isTrigger = true;
    }

    // Kill player and restart level after delay
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (oneShot && fired) return;
        if (!other.CompareTag(GameConstants.Tags.Player)) return;

        fired = true;

        var pde = other.GetComponent<PlayerDeathEffects>();
        if (pde != null)
        {
            pde.TriggerDeathEffects(other.transform.position);
        }
        else
        {
            if (sfx != null && deathClip != null)
            {
                sfx.PlayOneShot(deathClip, deathVolume);
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddDeath(1);
        }

        StartCoroutine(RestartAfterDelay());
    }

    // Wait then restart current level
    private System.Collections.IEnumerator RestartAfterDelay()
    {
        float delay = Mathf.Max(0f, deathDelay);
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
        RestartCurrentLevel();
    }

    // Restart helper
    private void RestartCurrentLevel()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartLevel();
        }
        else
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                SceneManager.LoadScene(scene.name, LoadSceneMode.Single);
            }
        }
    }
}
