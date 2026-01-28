using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Points")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    [Header("Settings")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private bool loopMovement = true;
    [SerializeField] private bool pingPong = true;
    [SerializeField] private float waitAtPoints = 0.1f;

    [Header("Passenger Handling")]
    [SerializeField] private bool parentPlayer = true;

    private Rigidbody2D rb;
    private Vector2 lastPosition;
    private bool movingToEnd = true;
    private bool isWaiting = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Start()
    {
        if (startPoint == null || endPoint == null)
        {
            Debug.LogWarning($"[MovingPlatform] Missing points on {gameObject.name}");
            enabled = false;
            return;
        }

        transform.position = startPoint.position;
        lastPosition = rb.position;
    }

    private void FixedUpdate()
    {
        if (isWaiting) return;

        Vector2 currentPos = rb.position;
        Vector2 targetPos = movingToEnd ? endPoint.position : startPoint.position;
        
        Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, speed * Time.fixedDeltaTime);
        
        // Calculate delta for passengers
        Vector2 delta = newPos - currentPos;
        
        // Move platform
        rb.MovePosition(newPos);

        // Move passengers manually to ensure they stay with the platform even if they have their own velocity
        if (delta.sqrMagnitude > 0)
        {
            MovePassengers(delta);
        }

        // Check if reached target
        if (Vector2.Distance(newPos, targetPos) < 0.001f)
        {
            StartCoroutine(WaitAtPoint());
        }
    }

    private void MovePassengers(Vector2 delta)
    {
        // Find all children that have a Rigidbody2D and move them by the delta
        foreach (Transform child in transform)
        {
            if (child.CompareTag(GameConstants.Tags.Player))
            {
                Rigidbody2D childRb = child.GetComponent<Rigidbody2D>();
                if (childRb != null)
                {
                    childRb.position += delta;
                }
            }
        }
    }

    private IEnumerator WaitAtPoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitAtPoints);

        if (pingPong)
        {
            movingToEnd = !movingToEnd;
            
            // If we just returned to start and loop is off, stop movement
            if (movingToEnd && !loopMovement)
            {
                enabled = false;
            }
        }
        else
        {
            // One way movement
            if (!loopMovement)
            {
                enabled = false;
            }
            else
            {
                // If looping but not ping-ponging, snap back to start
                rb.position = startPoint.position;
                movingToEnd = true;
            }
        }

        isWaiting = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!parentPlayer) return;
        
        if (collision.collider.CompareTag(GameConstants.Tags.Player))
        {
            collision.collider.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!parentPlayer) return;

        if (collision.collider.CompareTag(GameConstants.Tags.Player))
        {
            if (collision.collider.transform.parent == transform)
            {
                collision.collider.transform.SetParent(null);
            }
        }
    }
}
