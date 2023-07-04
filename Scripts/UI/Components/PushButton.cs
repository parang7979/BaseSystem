using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Parang.UI
{
    public class PushButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
    {
        public float PushThresholds = 0.5f;
        public float PushDelay = 0.5f;

        [Serializable]
        public class ButtonPushedEvent : UnityEvent<int> { }
        public ButtonPushedEvent OnButtonPush = new ButtonPushedEvent();

        private bool _isPressed;
        private bool _isPush;
        private float _elapsed;
        private int _counter;

        private void Awake()
        {
            _isPressed = false;
            _isPush = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            _isPush = false;
            _elapsed = 0f;
            _counter = 0;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.dragging) return;

            if (_isPressed && !_isPush)
                OnButtonPush?.Invoke(_counter);

            _isPressed = false;
            _isPush = false;
            _elapsed = 0f;
            _counter = 0;
        }

        public void OnDrag(PointerEventData eventData)
        {

        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isPressed && !_isPush)
                OnButtonPush?.Invoke(_counter);

            _isPressed = false;
            _isPush = false;
            _elapsed = 0f;
            _counter = 0;
        }

        private void OnDisable()
        {
            _isPressed = false;
            _isPush = false;
            _elapsed = 0f;
            _counter = 0;
        }

        private void Update()
        {
            if (_isPush)
            {
                if (_elapsed > PushDelay)
                {
                    _elapsed = 0f;
                    _counter++;
                    OnButtonPush.Invoke(_counter);
                }
                _elapsed += Time.deltaTime;
            }
            else if (_isPressed)
            {
                if (_elapsed > PushThresholds)
                {
                    _isPush = true;
                    _elapsed = 0f;
                    _counter = 0;
                }
                _elapsed += Time.deltaTime;
            }
        }
    }
}
