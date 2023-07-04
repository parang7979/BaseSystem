using Parang.Util;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Parang
{
    public class MoveCamera : MonoBehaviour
    {
        public GameObject Mover;
        public Camera Camera;
        public Rect MoveRect;

        public float MaxZoomIn = 5f;
        public float MaxZoomOut = 10f;
        public float ScreenMoveSensetive = 0.0025f;
        public float PinchZoomSensetive = 0.1f;

        [Serializable]
        public class ScreenInputEvent : UnityEvent<Ray> { }
        public ScreenInputEvent OnScreenClick = new ScreenInputEvent();
        public ScreenInputEvent OnScreenDown = new ScreenInputEvent();

        [Serializable]
        public class ScreenDragEvent : UnityEvent<Vector3> { }
        public ScreenDragEvent OnScreenDrag = new ScreenDragEvent();

        public bool Lock { get; set; }
        public bool MoveLock { get; set; }
        public bool ZoomLock { get; set; }

        private int _rotate;

        private bool _down;
        private bool _drag;
        private Vector2 _prevDrag;

        private bool _pinch;
        private float _prevLength;
        private float _baseSize;

        private Transform _followTarget;
        private bool _follow;

        private void Awake()
        {
            if (Camera == null)
                Camera = GetComponentInChildren<Camera>();

            ZoomLock = false;
            _followTarget = null;
            _follow = false;
        }

        public void PositionReset()
        {
            Mover.transform.localPosition = Vector3.zero;
        }

        public bool Raycast(out RaycastHit hit, int layerMask)
        {
            var pos = Input.touchCount > 0 ? Input.touches[0].position : (Vector2)Input.mousePosition;
            var ray = Camera.ScreenPointToRay(pos);
            return Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
        }

        public bool Raycast(out RaycastHit hit, string layer)
        {
            var pos = Input.touchCount > 0 ? Input.touches[0].position : (Vector2)Input.mousePosition;
            var ray = Camera.ScreenPointToRay(pos);
            return Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(layer));
        }

        public bool Raycast(string layer)
        {
            var pos = Input.touchCount > 0 ? Input.touches[0].position : (Vector2)Input.mousePosition;
            var ray = Camera.ScreenPointToRay(pos);
            return Physics.Raycast(ray, Mathf.Infinity, LayerMask.GetMask(layer));
        }

        public bool Raycast(Vector3 screenPos, out RaycastHit hit, int layerMask)
        {
            var ray = Camera.ScreenPointToRay(screenPos);
            return Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
        }

        public Vector2 GetScreenPoint(Vector3 position)
        {
            return Camera.WorldToScreenPoint(position);
        }

        public void Zoom(float delta)
        {
            if (Camera.orthographic)
                Camera.orthographicSize = Mathf.Clamp(delta, MaxZoomIn, MaxZoomOut);
            else
                Camera.fieldOfView = Mathf.Clamp(delta, MaxZoomIn, MaxZoomOut);
        }

        public void OverZoom(float delta)
        {
            if (Camera.orthographic)
                Camera.orthographicSize = Mathf.Clamp(delta, 0, MaxZoomOut);
            else
                Camera.fieldOfView = Mathf.Clamp(delta, 0, MaxZoomOut);
        }

        public void Move(Vector3 delta)
        {
            var move = Mover.transform.rotation * new Vector3(delta.x, 0f, delta.y);
            var pos = Mover.transform.position;
            Mover.transform.position = MoveRect.Clamp(pos + move);
        }

        public void TurnLeft()
        {
            _rotate++;
            transform.localRotation = Quaternion.Euler(0f, _rotate * 90f, 0f);
        }

        public void TurnRight()
        {
            _rotate--;
            transform.localRotation = Quaternion.Euler(0f, _rotate * 90f, 0f);
        }

        private void UpdateForMouse()
        {
            var pos = (Vector2)Input.mousePosition;

            if (Input.GetMouseButtonUp(0))
            {
                if (_down && !_drag)
                    OnScreenClick.Invoke(Camera.ScreenPointToRay(pos));
                _down = false;
            }

            if (_down && Input.GetMouseButton(0))
            {
                if (_drag)
                {
                    var delta = (_prevDrag - pos) * ScreenMoveSensetive * Camera.orthographicSize / QualitySettings.resolutionScalingFixedDPIFactor;
                    Move(delta);
                    OnScreenDrag.Invoke(delta);
                    _prevDrag = pos;
                }
                else if ((_prevDrag - pos).sqrMagnitude > 5)
                {
                    if (!MoveLock)
                    {
                        _drag = true;
                    }
                }
            }

            if (!ZoomLock)
            {
                Camera.orthographicSize = Mathf.Clamp(Camera.orthographicSize - Input.mouseScrollDelta.y, MaxZoomIn, MaxZoomOut);
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1))
                return;

            if (Input.GetMouseButtonDown(0))
            {
                _followTarget = null;
                OnScreenDown.Invoke(Camera.ScreenPointToRay(pos));
                _prevDrag = pos;
                _drag = false;
                _down = true;
            }
        }

        private void UpdateForTouch()
        {
            if (Input.touchCount == 1)
            {
                var t = Input.touches[0];
                var pos = t.position;

                if (t.phase == TouchPhase.Ended)
                {
                    if (_down && !_drag)
                        OnScreenClick.Invoke(Camera.ScreenPointToRay(pos));
                    _down = false;
                    return;
                }

                if (_down && t.phase == TouchPhase.Moved)
                {
                    if (_drag)
                    {
                        var delta = (_prevDrag - pos) * ScreenMoveSensetive * Camera.orthographicSize / QualitySettings.resolutionScalingFixedDPIFactor;
                        Move(delta);
                        OnScreenDrag.Invoke(delta);
                        _prevDrag = pos;
                    }
                    else if ((_prevDrag - pos).sqrMagnitude > 5)
                    {
                        if (!MoveLock)
                            _drag = true;
                    }
                }

                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(0))
                    return;

                if (t.phase == TouchPhase.Began)
                {
                    _followTarget = null;
                    OnScreenDown.Invoke(Camera.ScreenPointToRay(pos));
                    _prevDrag = pos;
                    _drag = false;
                    _down = true;
                }
            }

            if (!ZoomLock)
            {
                if (Input.touchCount == 2)
                {
                    var t1 = Input.touches[0];
                    var t2 = Input.touches[1];
                    if (_pinch)
                    {
                        if (t1.phase == TouchPhase.Moved || t2.phase == TouchPhase.Moved)
                        {
                            var length = (t1.position - t2.position).magnitude;
                            var delta = _baseSize - (length - _prevLength) * PinchZoomSensetive / QualitySettings.resolutionScalingFixedDPIFactor;
                            Zoom(delta);
                        }
                    }
                    else
                    {
                        _prevLength = (t1.position - t2.position).magnitude;
                        if (Camera.orthographic)
                            _baseSize = Camera.orthographicSize;
                        else
                            _baseSize = Camera.fieldOfView;
                        _pinch = true;
                    }

                    if (t1.phase == TouchPhase.Ended)
                        _prevDrag = t2.position;

                    if (t2.phase == TouchPhase.Ended)
                        _prevDrag = t1.position;

                }
                else
                {
                    _pinch = false;
                }
            }
        }

        private void Update()
        {
            Mover.transform.position = MoveRect.Clamp(Mover.transform.position);
            // Shader.SetGlobalVector("_CameraPos", Camera.transform.position);
            Shader.SetGlobalFloat("_ProjectionSize", Camera.orthographicSize / MaxZoomOut);
            if (_follow && _followTarget != null)
            {
                Mover.transform.position = _followTarget.position;
            }
            if (Lock) return;

#if UNITY_EDITOR || UNITY_STANDALONE
            UpdateForMouse();
#else
            UpdateForTouch();
#endif
        }

        public void RemoveLayer(int mask)
        {
            Camera.cullingMask &= ~mask;
        }

        public void AddLayer(int mask)
        {
            Camera.cullingMask |= mask;
        }

        private void OnDrawGizmos()
        {
            var center = new Vector3(MoveRect.center.x, 0f, MoveRect.center.y);
            var size = new Vector3(MoveRect.size.x, 0f, MoveRect.size.y);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
