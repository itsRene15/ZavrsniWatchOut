using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ResetButtonListener : MonoBehaviour
{
    // Hook up the reset button
    private void Start()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        
        btn.onClick.AddListener(OnClickReset);
    }

    // Reset saved progress
    private void OnClickReset()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetProgress();
            
            Debug.Log("Resetting Progress..."); 
        }
    }
}