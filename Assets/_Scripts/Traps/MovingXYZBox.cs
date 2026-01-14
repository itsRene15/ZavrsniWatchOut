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

    private readonly HashSet<int> ids = new HashSet<int>();

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

        moveRoutine = StartCoroutine(MoveCoroutine(displacement));
    }

    // Move over time linearly
    private System.Collections.IEnumerator MoveCoroutine(Vector3 displacement)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + displacement;

        float distance = Mathf.Max(0.0001f, displacement.magnitude);
        float speed = Mathf.Max(0.0001f, moveUnitsPerSecond);
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
