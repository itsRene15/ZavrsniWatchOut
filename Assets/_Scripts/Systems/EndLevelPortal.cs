using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class EndLevelPortal : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator animator;

    [SerializeField] private string animTriggerName = "Activate";

    [SerializeField] private float animationDuration = 1.0f;

    [Header("Player Disappear (Entrance)")]
    [SerializeField] private bool movePlayerToPortal = true;
    [SerializeField] private bool parentPlayerToPortal = true;
    [SerializeField] private float shrinkDuration = 0.2f;
    [SerializeField] private AnimationCurve shrinkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip portalSound;

    private bool fired;

    // Try to auto-find animator reference in editor
    private void Reset()
    {
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    // Handle player entering portal trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (fired) return;
        if (!other.CompareTag(GameConstants.Tags.Player)) return;

        fired = true; 

        if (audioSource != null && portalSound != null)
        {
            audioSource.PlayOneShot(portalSound);
        }

        if (animator != null)
        {
            animator.SetTrigger(animTriggerName);
        }

        StartCoroutine(EnterPortalSequence(other.transform));
    }

    // Shrink player and then finish the level
    private IEnumerator EnterPortalSequence(Transform player)
    {
        if (player != null)
        {
            Vector3 startScale = player.localScale;

            if (movePlayerToPortal)
            {
                player.position = transform.position;
            }

            Transform originalParent = player.parent;
            if (parentPlayerToPortal)
            {
                player.SetParent(transform, worldPositionStays: true);
            }

            float dur = Mathf.Max(0f, shrinkDuration);
            if (dur > 0f)
            {
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.unscaledDeltaTime / dur;
                    float k = Mathf.Clamp01(t);
                    float eased = shrinkCurve != null ? shrinkCurve.Evaluate(k) : k;
                    player.localScale = Vector3.LerpUnclamped(startScale, Vector3.zero, eased);
                    yield return null;
                }
            }
            player.localScale = Vector3.zero;
        }

        yield return new WaitForSeconds(animationDuration);

        if (GameManager.Instance != null)
            GameManager.Instance.CompleteLevel();
        else
            Debug.LogWarning("[EndLevelPortal] GameManager.Instance not found.");
    }
}