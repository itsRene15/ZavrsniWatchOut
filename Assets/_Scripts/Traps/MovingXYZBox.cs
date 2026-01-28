using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingXYZBox : MonoBehaviour
{
    [Header("Target Filter")]
    [SerializeField] private string requiredTag = GameConstants.Tags.Player;

    [Header("Position Offset On Enter")]
    [SerializeField] private Vector3 moveOffset = Vector3.zero;
    [SerializeField] private float moveSpeed = 1f;

    [Header("Movement over time")]
    [SerializeField] private float moveUnitsPerSecond = 15f;
    [SerializeField] private bool ignoreWhileMoving = true;

    [Header("Scale Change On Enter")]
    [SerializeField] private Vector3 scaleMultiplier = Vector3.one;

    [Header("One-Shot Control")]
    [SerializeField] private bool affectOncePerObject = true;

    [Header("Return Settings")]
    [SerializeField] private bool autoReturn = false;
    [SerializeField] private float returnDelay = 1.0f;
    [SerializeField] private float returnSpeedMultiplier = 1f;

    private readonly HashSet<int> ids = new HashSet<int>();
    private Vector3 initialPosition;
    private Vector3 initialScale;
    private Coroutine returnRoutine;

    private void Start()
    {
        initialPosition = transform.position;
        initialScale = transform.localScale;
    }

    // 2D trigger entry
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryAffect(other.gameObject);
    }

    // Try to move/scale this object
    private void TryAffect(GameObject obj)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !obj.CompareTag(requiredTag))
            return;

        if (affectOncePerObject)
        {
            int id = obj.GetInstanceID();
            if (ids.Contains(id))
                return;
            ids.Add(id);
        }

        var displacement = moveOffset * moveSpeed;
        StartSmoothMove(displacement);

        var ls = transform.localScale;
        ls = new Vector3(ls.x * scaleMultiplier.x, ls.y * scaleMultiplier.y, ls.z * scaleMultiplier.z);
        transform.localScale = ls;

        if (autoReturn)
        {
            if (returnRoutine != null) StopCoroutine(returnRoutine);
            returnRoutine = StartCoroutine(ReturnAfterDelay());
        }
    }

    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);
        
        // Return to initial scale
        transform.localScale = initialScale;

        // Return to initial position
        Vector3 currentPos = transform.position;
        Vector3 diff = initialPosition - currentPos;
        
        if (diff.sqrMagnitude > 0.0001f)
        {
            if (moveRoutine != null) StopCoroutine(moveRoutine);
            moveRoutine = StartCoroutine(MoveToTargetCoroutine(initialPosition, returnSpeedMultiplier));
        }

        returnRoutine = null;
    }

    private Coroutine moveRoutine;

    // Start a smooth move
    private void StartSmoothMove(Vector3 displacement)
    {
        if (displacement.sqrMagnitude <= 0f)
            return;

        if (moveRoutine != null)
        {
            if (ignoreWhileMoving)
                return;
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        Vector3 targetPos = transform.position + displacement;
        moveRoutine = StartCoroutine(MoveToTargetCoroutine(targetPos, 1f));
    }

    // Move to target over time linearly
    private IEnumerator MoveToTargetCoroutine(Vector3 endPos, float speedMultiplier)
    {
        Vector3 startPos = transform.position;
        float distance = Vector3.Distance(startPos, endPos);
        float speed = Mathf.Max(0.0001f, moveUnitsPerSecond * speedMultiplier);
        float duration = distance / speed;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = Mathf.Clamp01(t);
            transform.position = Vector3.LerpUnclamped(startPos, endPos, k);
            yield return null;
        }

        transform.position = endPos;
        moveRoutine = null;
    }

}
