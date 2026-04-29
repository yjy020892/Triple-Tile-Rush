using UnityEngine;

// Screen.safeArea 를 감시하면서 대상 RectTransform 의 anchor 를 안전 영역에 맞게 조정.
// 보통 전체 UI 의 부모 컨테이너에 붙여서 상단 노치/홈인디케이터와의 겹침을 방지한다.
//
// 사용법:
//   var safeRect = panelGO.AddComponent<SortingSafeAreaFitter>();
//   safeRect.ApplyNow();
//
// 회전/해상도 변경/스플릿 스크린 복귀 시 매 프레임 체크는 부담이므로 OnEnable + 주기적 폴링.
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
