using UnityEngine;

//
//   var safeRect = panelGO.AddComponent<SortingSafeAreaFitter>();
//   safeRect.ApplyNow();
//
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public sealed class SortingSafeAreaFitter : MonoBehaviour
{
    [SerializeField] private bool applyTop = true;
    [SerializeField] private bool applyBottom = true;
    [SerializeField] private bool applyLeft = true;
    [SerializeField] private bool applyRight = true;
    [SerializeField] private float pollInterval = 0.5f;

    private RectTransform rectTransform;
    private Rect lastSafeArea;
    private Vector2Int lastScreenSize;
    private float nextPollTime;

    private void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplyNow();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextPollTime) return;
        nextPollTime = Time.unscaledTime + pollInterval;

        Rect current = Screen.safeArea;
        Vector2Int size = new Vector2Int(Screen.width, Screen.height);
        if (current == lastSafeArea && size == lastScreenSize) return;
        ApplyNow();
    }

    public void ApplyNow()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return;

        Rect safe = Screen.safeArea;
        Vector2Int size = new Vector2Int(Mathf.Max(1, Screen.width), Mathf.Max(1, Screen.height));

        Vector2 anchorMin = safe.position;
        Vector2 anchorMax = safe.position + safe.size;
        anchorMin.x /= size.x;
        anchorMin.y /= size.y;
        anchorMax.x /= size.x;
        anchorMax.y /= size.y;

        if (!applyLeft)   anchorMin.x = 0f;
        if (!applyBottom) anchorMin.y = 0f;
        if (!applyRight)  anchorMax.x = 1f;
        if (!applyTop)    anchorMax.y = 1f;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        lastSafeArea = safe;
        lastScreenSize = size;
    }
}
