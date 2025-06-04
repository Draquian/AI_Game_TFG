using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // Cached UI scale value (default 1 = 100%)
    private float uiScale = 1f;

    // Optionally reference all canvases to update scale automatically
    private Canvas[] canvases;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Find all canvases in the current scene to adjust their scale factor
        canvases = FindObjectsOfType<Canvas>(true);
        ApplyUIScale();
    }

    // Returns the current UI scale
    public float GetScale() => uiScale;

    // Adjusts the UI scale and updates all Canvas elements
    public void SetScale(float scale)
    {
        uiScale = scale;
        ApplyUIScale();
    }

    private void ApplyUIScale()
    {
        if (canvases == null) return;
        // Update each canvas's scale factor. This assumes you're using a CanvasScaler
        // where the scaleFactor can be manually controlled.
        foreach (Canvas canvas in canvases)
        {
            // You may need to adjust this approach based on your UI implementation.
            // For example, if using a CanvasScaler component, change its scaleFactor property.
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.scaleFactor = uiScale;
            }
        }
    }
}