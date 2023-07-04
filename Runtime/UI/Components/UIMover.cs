using UnityEngine;

namespace Parang.UI
{
    public class UIMover : MonoBehaviour
    {
        private static readonly Vector2 center = new Vector2(0.5f, 0.5f);

        public float Space = 100;
        public float Size = 200;

        private Rect _rect;
        private RectTransform _parentsRect;
        private RectTransform _rectTransform;

        private MoveCamera _camera;
        private Transform _target;

        public void Setup(MoveCamera camera)
        {
            _camera = camera;
            _rect = new Rect(-Space, -Space, Screen.width + Space * 2, Screen.height + Space * 2);
            _parentsRect = transform.parent.transform as RectTransform;
            _rectTransform = transform as RectTransform;
            _target = null;
        }

        public bool FollowOnce(RectTransform target)
        {
            if (target == null) return false;
            var parents = _rectTransform.parent;
            _rectTransform.SetParent(target);
            _rectTransform.anchorMin = center;
            _rectTransform.anchorMax = center;
            _rectTransform.pivot = center;
            _rectTransform.anchoredPosition = Vector2.zero;
            // 사이즈 조절 필요할때
            _rectTransform.sizeDelta = target.sizeDelta;
            _rectTransform.SetParent(parents, true);
            return true;
        }

        public bool FollowOnce(Transform target)
        {
            if (target == null) return false;
            var screenPoint = _camera.GetScreenPoint(target.position);
            if (_rect.Contains(screenPoint))
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentsRect, screenPoint, null, out var point))
                {
                    _rectTransform.anchorMin = center;
                    _rectTransform.anchorMax = center;
                    _rectTransform.pivot = center;
                    _rectTransform.anchoredPosition = Vector2.zero;
                    // 사이즈 조절 필요할때
                    _rectTransform.sizeDelta = new Vector2(Size, Size);
                    _rectTransform.anchoredPosition = point;
                    return true;
                }
            }
            return false;
        }

        public void Follow(Transform target)
        {
            _target = target;
            FollowOnce(target);
        }

        private void Update()
        {
            if (_target != null)
            {
                FollowOnce(_target);
            }
        }
    }
}
