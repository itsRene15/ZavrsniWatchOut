using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatformTrap : MonoBehaviour
{
    [Header("Points")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Motion")]
    public float speed = 2f;
    public bool alwaysMoving = true;
    public float waitAtEnds = 0.2f;
    public bool startAtEnd = false;

    [Header("Passenger Handling")]
    public bool parentPlayer = true;

    private Rigidbody2D rb;
    private Transform from;
    private Transform to;
    private Coroutine moveRoutine;

    // Set up rigidbody
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    // Initialize position and optionally start moving
    private void Start()
    {
        if (startPoint == null || endPoint == null)
        {
            Debug.LogWarning("[MovingPlatformTrap] Assign startPoint and endPoint.");
            enabled = false;
            return;
        }

        transform.position = startAtEnd ? endPoint.position : startPoint.position;
        from = startAtEnd ? endPoint : startPoint;
        to = startAtEnd ? startPoint : endPoint;

        if (alwaysMoving)
        {
            moveRoutine = StartCoroutine(MoveLoop());
        }
    }

    // Trigger a single move segment
    public void StartMovingOnce()
    {
        if (moveRoutine != null) return;
        moveRoutine = StartCoroutine(MoveOnce());
    }

    // Loop platform movement back and forth
    private IEnumerator MoveLoop()
    {
        while (true)
        {
            yield return MoveSegment(from.position, to.position);
            if (waitAtEnds > 0f) yield return new WaitForSeconds(waitAtEnds);
            var tmp = from; from = to; to = tmp;
        }
    }

    // Move once from current from->to
    private IEnumerator MoveOnce()
    {
        yield return MoveSegment(from.position, to.position);
        moveRoutine = null;
    }

    // Move from A to B linearly
    private IEnumerator MoveSegment(Vector3 from, Vector3 to)
    {
        float dist = Vector3.Distance(from, to);
        float spd = Mathf.Max(0.0001f, speed);
        float duration = dist / spd;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.fixedDeltaTime / duration;
            float k = Mathf.Clamp01(t);
            Vector2 target = Vector2.LerpUnclamped(from, to, k);
            rb.MovePosition(target);
            yield return new WaitForFixedUpdate();
        }
        rb.MovePosition(to);
    }

    // Parent player while on platform
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!parentPlayer) return;
        if (collision.collider != null && collision.collider.CompareTag(GameConstants.Tags.Player))
        {
            collision.collider.transform.SetParent(transform);
        }
    }

    // Unparent when leaving platform
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!parentPlayer) return;
        if (collision.collider != null && collision.collider.CompareTag(GameConstants.Tags.Player))
        {
            if (collision.collider.transform.parent == transform)
                collision.collider.transform.SetParent(null);
        }
    }
}
