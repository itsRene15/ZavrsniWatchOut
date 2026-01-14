
using UnityEngine;
using UnityEngine.UI;
public class WorldButton : MonoBehaviour
{
    public int worldIndex = 0;

    [Header("UI Refs")]
    public Button button;
    public Image targetImage;
    public Sprite lockedSprite;
    public Sprite unlockedSprite;

    // Try to set default references in editor
    private void Reset()
    {
        if (button == null) button = GetComponent<Button>();
        if (targetImage == null)
        {
            var img = TryGetButtonImage();
            if (img != null) targetImage = img;
            else targetImage = GetComponent<Image>();
        }
    }

    // Cache references and hook click
    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (targetImage == null)
        {
            var img = TryGetButtonImage();
            if (img != null) targetImage = img;
            else targetImage = GetComponent<Image>();
        }
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    // Refresh when enabled
    private void OnEnable()
    {
        Refresh();
    }

    // Update visuals and interactable state
    public void Refresh()
    {
        bool unlocked = GameManager.Instance != null && GameManager.Instance.IsWorldUnlocked(worldIndex);

        if (button != null)
        {
            button.interactable = unlocked;
        }

        if (targetImage != null)
        {
            if (unlocked && unlockedSprite != null)
                targetImage.sprite = unlockedSprite;
            else if (!unlocked && lockedSprite != null)
                targetImage.sprite = lockedSprite;
        }
    }

    // Handle click to load world
    private void OnClick()
    {
        if (GameManager.Instance == null) return;
        if (!GameManager.Instance.IsWorldUnlocked(worldIndex)) return;
        GameManager.Instance.LoadFirstSubLevelOfWorld(worldIndex);
    }

    // Get the button's image if available
    private Image TryGetButtonImage()
    {
        if (button != null)
        {
            var g = button.targetGraphic as Image;
            if (g != null) return g;
        }
        return null;
    }
}
