using UnityEngine;
using TMPro;

public class LevelSelectController : MonoBehaviour
{
    [Header("UI: Death Counter")]
    public TextMeshProUGUI deathCounterText;

    // Refresh buttons and death counter on start
    private void Start()
    {
        RefreshAll();
        RefreshDeathCounter();
    }

    // Refresh when enabled and listen for death updates
    private void OnEnable()
    {
        RefreshAll();
        RefreshDeathCounter();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDeathsChanged ??= new UnityEngine.Events.UnityEvent<int>();
            GameManager.Instance.OnDeathsChanged.AddListener(HandleDeathsChanged);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null && GameManager.Instance.OnDeathsChanged != null)
        {
            GameManager.Instance.OnDeathsChanged.RemoveListener(HandleDeathsChanged);
        }
    }

    // Refresh all world buttons
    public void RefreshAll()
    {
        var buttons = GetComponentsInChildren<WorldButton>(includeInactive: true);
        foreach (var wb in buttons)
        {
            wb.Refresh();
        }
        RefreshDeathCounter();
    }

    // Handle death counter change
    private void HandleDeathsChanged(int total)
    {
        RefreshDeathCounter();
    }

    // Update the on-screen total deaths
    public void RefreshDeathCounter()
    {
        if (deathCounterText == null) return;
        int total = GameManager.Instance != null ? GameManager.Instance.GetTotalDeaths() : 0;
        deathCounterText.text = total.ToString();
    }
}
