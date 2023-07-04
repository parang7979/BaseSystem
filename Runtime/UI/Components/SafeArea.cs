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

            // ���� anchor x ��ǥ ���
            anchorMin.y /= Screen.height;
            anchorMax.y /= Screen.height;

            anchorMin.x /= Screen.width;
            anchorMax.x /= Screen.width;

            ((RectTransform)transform).anchorMin = anchorMin;
            ((RectTransform)transform).anchorMax = anchorMax;
        }
    }
}
