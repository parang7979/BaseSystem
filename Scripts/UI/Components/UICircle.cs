using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Parang.UI
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class UICircle : UIBehaviour, ILayoutGroup, IDragHandler
    {
        public bool Clockwise { get { return _clockwise; } set { SetProperty(ref _clockwise, value); } }
        public float StartAngle { get { return _startAngle; } set { SetProperty(ref _startAngle, value); } }
        public float GapAngle { get { return _gapAngle; } set { SetProperty(ref _gapAngle, value); } }
        public float Range { get { return _range; } set { SetProperty(ref _range, value); } }

        protected RectTransform rectTransform
        {
            get
            {
                if (_rect == null)
                    _rect = GetComponent<RectTransform>();
                return _rect;
            }
        }
        protected DrivenRectTransformTracker _tracker;

        [SerializeField] private bool _clockwise = true;
        [SerializeField, Range(-360f, 360f)] private float _startAngle = 0f;
        [SerializeField, Range(0f, 360f)] private float _gapAngle = 10f;
        [SerializeField, Range(0f, 1f)] private float _range = 0.7f;
        [SerializeField] private bool _useRotate = true;
        private RectTransform _rect;

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // m_Tracker.Clear();
            // LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            SetDirty();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            base.OnDidApplyAnimationProperties();
            SetDirty();
        }

        public void SetLayoutHorizontal()
        {
            if (rectTransform.childCount == 0) return;

            var r = _rect.sizeDelta.x / 2f * _range;
            var start = (90f + (_clockwise ? -_startAngle : _startAngle)) * Mathf.Deg2Rad;
            var gap = (_clockwise ? -_gapAngle : _gapAngle) * Mathf.Deg2Rad;
            var angle = 0f;
            for (int i = 0; i < rectTransform.childCount; i++)
            {
                var rect = rectTransform.GetChild(i) as RectTransform;
                if (rect == null || !rect.gameObject.activeInHierarchy)
                    continue;

                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                var a = start + angle;
                var pos = new Vector2(r * Mathf.Cos(a), r * Mathf.Sin(a));
                rect.anchoredPosition = pos;
                angle += gap;
            }
        }

        public void SetLayoutVertical()
        {

        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            SetDirty();
        }

        protected virtual void OnTransformChildrenChanged()
        {
            SetDirty();
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            SetDirty();
        }

        protected virtual void SetupChildTracker()
        {
            _tracker.Clear();
            for (int i = 0; i < rectTransform.childCount; i++)
            {
                var rect = rectTransform.GetChild(i) as RectTransform;
                if (rect == null) continue;
                _tracker.Add(this, rect,
                    DrivenTransformProperties.AnchoredPosition |
                    DrivenTransformProperties.Pivot |
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.Rotation);
            }
        }

        /// <summary>
        /// Helper method used to set a given property if it has changed.
        /// </summary>
        /// <param name="currentValue">A reference to the member value.</param>
        /// <param name="newValue">The new value.</param>
        protected void SetProperty<T>(ref T currentValue, T newValue)
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return;
            currentValue = newValue;
            SetDirty();
        }

        /// <summary>
        /// Mark the LayoutGroup as dirty.
        /// </summary>
        protected void SetDirty()
        {
            if (!IsActive())
                return;

            SetupChildTracker();

            if (!CanvasUpdateRegistry.IsRebuildingLayout())
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            else
                StartCoroutine(DelayedSetDirty(rectTransform));
        }

        IEnumerator DelayedSetDirty(RectTransform rectTransform)
        {
            yield return null;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        private void CalculateChildRotation()
        {
            for (int i = 0; i < rectTransform.childCount; i++)
            {
                var rect = rectTransform.GetChild(i) as RectTransform;
                if (rect == null)
                    continue;

                var rotation = rect.localRotation.eulerAngles;
                rotation.z = -rectTransform.localRotation.eulerAngles.z;
                rect.localRotation = Quaternion.Euler(rotation);
            }
        }

        virtual protected void Update()
        {
            if (rectTransform.hasChanged)
            {
                CalculateChildRotation();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_useRotate && eventData.dragging && rectTransform.childCount > 0)
            {
                float delta;
                if (Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y))
                    delta = -eventData.delta.x;
                else
                    delta = eventData.delta.y;
                delta *= 0.5f;
                var rot = rectTransform.localRotation.eulerAngles;
                rot.z = (rot.z + delta) % 360f;
                // Locking rotate
                // rot.z = Mathf.Clamp(rot.z, 0f, _gapAngle * (rectTransform.childCount - 1) - 90f);
                rectTransform.localRotation = Quaternion.Euler(rot);
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Range = 1f;
        }

        public void Hide()
        {
            rectTransform.localRotation = Quaternion.Euler(Vector3.zero);
            Range = 0f;
            gameObject.SetActive(false);
        }

        public void Toggle()
        {
            if (gameObject.activeInHierarchy)
                Hide();
            else
                Show();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }
#endif
    }
}
