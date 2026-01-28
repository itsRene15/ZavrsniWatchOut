using System.Collections;
using UnityEngine;

public class PopupSpikes : MonoBehaviour
{
    [Header("Movement (Transform Y)")]
    [SerializeField] private float riseDistance = 1f; 
    [SerializeField] private float riseDuration = 0.15f;
    [SerializeField] private float lowerDuration = 0.4f;

    [Header("Behavior")]
    [SerializeField] private bool autoRetract = true;
    [SerializeField] private float retractDelay = 1.5f;
    
    [Header("Player Shrink Effect")]
    [SerializeField] private float shrinkDuration = 0.2f;

    [Header("Damage")]
    [SerializeField] private bool useTriggerForDamage = true;
    [SerializeField] private float deathDelay = 0.25f;

    [Header("Optional SFX/VFX")]
    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioClip deathClip;
    public float deathVolume = 1f;

    private Coroutine retractRoutine;
    private Coroutine moveRoutine;
    private bool hasTriggeredDeath = false;
    private Vector3 initialPos;
    private Vector3 targetPos;

    // Cache start/target positions
    private void Start()
    {
        initialPos = transform.localPosition;
        targetPos = initialPos + new Vector3(0f, riseDistance, 0f);
    }

    private void OnEnable()
    {
        hasTriggeredDeath = false;
    }

    // Raise spikes
    public void Raise()
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        if (retractRoutine != null) StopCoroutine(retractRoutine);

        moveRoutine = StartCoroutine(MoveSpikes(targetPos, riseDuration));
        if (autoRetract)
        {
            retractRoutine = StartCoroutine(RetractAfterDelay());
        }
    }

    // Lower spikes
    public void Lower()
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        if (retractRoutine != null) StopCoroutine(retractRoutine);
        moveRoutine = StartCoroutine(MoveSpikes(initialPos, lowerDuration));
    }

    // Move transform towards target linearly
    private IEnumerator MoveSpikes(Vector3 destination, float duration)
    {
        Vector3 startPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localPosition = Vector3.LerpUnclamped(startPos, destination, t);
            yield return null;
        }

        transform.localPosition = destination;
        moveRoutine = null;
    }

    // Wait then lower back
    private IEnumerator RetractAfterDelay()
    {
        yield return new WaitForSeconds(retractDelay);
        Lower();
        retractRoutine = null;
    }

    // Called by zone when player enters
    public void OnZoneEntered(GameObject player)
    {
        Raise();
    }

    // Kill player on contact
    private void TryKill(Collider2D col)
    {
        if (hasTriggeredDeath) return; 
        if (!col.CompareTag(GameConstants.Tags.Player)) return;

        hasTriggeredDeath = true;

        var pde = col.GetComponent<PlayerDeathEffects>();
        if (pde != null)
        {
            pde.TriggerDeathEffects(col.transform.position);
        }
        else if (sfx && deathClip)
        {
            sfx.PlayOneShot(deathClip, deathVolume);
        }

        if (GameManager.Instance != null) GameManager.Instance.AddDeath(1);

        StartCoroutine(ChangePlayerSize(col.transform)); 
        StartCoroutine(RestartAfterDelay());
    }

    // Restart level after delay
    private IEnumerator RestartAfterDelay()
    {
        float waitTime = Mathf.Max(deathDelay, shrinkDuration, 0f);
        if (waitTime > 0f) yield return new WaitForSecondsRealtime(waitTime);

        if (GameManager.Instance != null) GameManager.Instance.RestartLevel();
        else UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTriggerForDamage) return;
        TryKill(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (useTriggerForDamage) return;
        TryKill(collision.collider);
    }

    // Shrink player over time linearly
    private IEnumerator ChangePlayerSize(Transform playerTransform)
    {
        if (playerTransform == null) yield break;
        
        Vector3 originalScale = playerTransform.localScale;
        float dur = Mathf.Max(0f, shrinkDuration);

        if (dur > 0f)
        {
            float t = 0f;
            while (t < 1f)
            {
                if (playerTransform == null) yield break;

                t += Time.unscaledDeltaTime / dur;
                float k = Mathf.Clamp01(t);
                playerTransform.localScale = Vector3.LerpUnclamped(originalScale, Vector3.zero, k);
                yield return null;
            }
        }

        if (playerTransform != null) playerTransform.localScale = Vector3.zero;
    }
}

