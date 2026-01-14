using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class PopupSpikesZone : MonoBehaviour
{
    public PopupSpikes spikes;

    // Set trigger and try to find spikes
    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
        if (spikes == null) spikes = GetComponentInParent<PopupSpikes>();
    }

    // Raise spikes when player enters
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(GameConstants.Tags.Player)) return;
        if (spikes != null) spikes.OnZoneEntered(other.gameObject);
    }
}
