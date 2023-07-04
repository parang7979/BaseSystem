using UnityEngine;

namespace Parang.UI
{
    public class SafeArea : MonoBehaviour
    {
        private void OnEnable()
        {
            Rect safeArea = Screen.safeArea;

            // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            // 기존 anchor x 좌표 사용
            anchorMin.y /= Screen.height;
            anchorMax.y /= Screen.height;

            anchorMin.x /= Screen.width;
            anchorMax.x /= Screen.width;

            ((RectTransform)transform).anchorMin = anchorMin;
            ((RectTransform)transform).anchorMax = anchorMax;
        }
    }
}
