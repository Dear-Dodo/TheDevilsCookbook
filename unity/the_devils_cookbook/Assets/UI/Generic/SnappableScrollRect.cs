using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TDC.UI.Generic
{
    public class SnappableScrollRect : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [HideInInspector]
        public ScrollRect ScrollRect;

        public float DecelerationRate;
        public float FastSwipeThresholdSpeed;
        public int FastSwipeThresholdDistance;
        public int StartingElementIndex;

        public Button NextElementElement;
        public Button PreviousElementElement;

        public Scrollbar HoriztonalScrollbar;
        public Scrollbar VerticalScrollbar;

        public event Action<int, int> onElementChanged;

        public event Action<Vector2> onDrag;

        private float _FastSwipeThresholdMaxLimit;

        private RectTransform _ScrollRectRect;
        private RectTransform _Container;

        private Scrollbar _Scrollbar;

        // Data
        private List<GameObject> _Data;

        private List<Vector2> _ElementPositions;

        // Selection
        private int _CurrentSelection;

        // Direction
        private bool _Horizontal;

        // Dragging
        private bool _IsDragging;

        private bool _ButtonPressed;

        private float _DragStartTime;
        private Vector2 _DragStartPosition;

        // Intepolation
        private Vector2 _Target;

        private bool _Lerping;

        private bool _Initialized = false;

        public void Start()
        {
            Initialize();
        }

        public void Update()
        {
            if (_Lerping)
            {
                float deceleration = Mathf.Min(DecelerationRate * Time.deltaTime, 1.0f);
                _Container.anchoredPosition = Vector2.Lerp(_Container.anchoredPosition, _Target, deceleration);
                if (Vector2.SqrMagnitude(_Container.anchoredPosition - _Target) < 0.25f)
                {
                    _Container.anchoredPosition = _Target;
                    ScrollRect.velocity = Vector2.zero;
                    _Lerping = false;
                    _ButtonPressed = false;
                }
            }
        }

        public void SetData(List<GameObject> objects)
        {
            _Data = objects;

            if (_Scrollbar)
            {
                _Scrollbar.numberOfSteps = objects.Count;
                _Scrollbar.size = 1.0f / objects.Count;
            }

            SetElementPositions();
            SetElement(StartingElementIndex);
        }

        private int GetNearestElement()
        {
            Vector2 currentPosition = _Container.anchoredPosition;

            float distance = float.MaxValue;
            int nearestElement = _CurrentSelection;

            for (int i = 0; i < _ElementPositions.Count; i++)
            {
                float testDistance = Vector2.SqrMagnitude(currentPosition - _ElementPositions[i]);
                if (testDistance < distance)
                {
                    distance = testDistance;
                    nearestElement = i;
                }
            }

            return nearestElement;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _Lerping = false;
            _IsDragging = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_IsDragging)
            {
                _IsDragging = true;
                _DragStartTime = Time.unscaledTime;
                _DragStartPosition = _Container.anchoredPosition;
            }

            onDrag?.Invoke(eventData.delta);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Vector2 difference = _DragStartPosition - _Container.anchoredPosition;
            float differenceFloat = _Horizontal ? difference.x : difference.y;

            if (Time.unscaledTime - _DragStartTime < FastSwipeThresholdSpeed &&
                Mathf.Abs(differenceFloat) > FastSwipeThresholdDistance &&
                Mathf.Abs(differenceFloat) < _FastSwipeThresholdMaxLimit)
            {
                if (differenceFloat > 0)
                {
                    MoveScreen(1);
                }
                else
                {
                    MoveScreen(-1);
                }
            }
            else
            {
                LerpToElement(GetNearestElement());
            }

            _IsDragging = false;
        }

        private void SetElementPositions()
        {
            int width = _Horizontal ? (int)_ScrollRectRect.rect.width : 0;
            int height = _Horizontal ? 0 : (int)_ScrollRectRect.rect.height;
            int offsetX = _Horizontal ? width / 2 : 0;
            int offsetY = _Horizontal ? 0 : height / 2;
            int containerWidth = _Horizontal ? width * _Data.Count : 0;
            int containerHeight = _Horizontal ? 0 : height * _Data.Count;
            _FastSwipeThresholdMaxLimit = _Horizontal ? width : height;

            Vector2 newSize = new Vector2(containerWidth, containerHeight);
            Vector2 newPosition = new Vector2(containerWidth / 2.0f, containerHeight / 2.0f);

            _Container.sizeDelta = newSize;
            _Container.anchoredPosition = newPosition;
            _ElementPositions.Clear();

            for (int i = 0; i < _Data.Count; i++)
            {
                RectTransform child = _Container.GetChild(i).GetComponent<RectTransform>();
                Vector2 childPosition = _Horizontal ? new Vector2(i * width - containerWidth / 2.0f + offsetX, 0.0f) : new Vector2(0, -(i * height - containerHeight / 2.0f + offsetY));
                child.anchoredPosition = childPosition;
                _ElementPositions.Add(-childPosition);
            }
        }

        private int SetIndex(int index)
        {
            int previous = _CurrentSelection;
            index = Mathf.Clamp(index, 0, _Data.Count - 1);
            _CurrentSelection = index;
            onElementChanged?.Invoke(previous, _CurrentSelection);

            if (_IsDragging || _ButtonPressed)
            {
                _Scrollbar.value = index / (_Data.Count - 1.0f);
            }

            return index;
        }

        private void SetElement(int index)
        {
            index = SetIndex(index);
            _Container.anchoredPosition = _ElementPositions[index];
            _CurrentSelection = index;
        }

        private void LerpToElement(int index)
        {
            index = SetIndex(index);
            _Target = _ElementPositions[index];
            _Lerping = true;
        }

        private void MoveScreen(int direction, bool buttonPress = false)
        {
            LerpToElement(_CurrentSelection + direction);
        }

        public void Initialize()
        {
            if (_Initialized)
                return;

            ScrollRect = this.GetComponent<ScrollRect>();
            _ScrollRectRect = ScrollRect.GetComponent<RectTransform>();
            _Container = ScrollRect.content;
            _Horizontal = ScrollRect.horizontal && !ScrollRect.vertical;
            _Lerping = false;
            _ElementPositions = new List<Vector2>();

            _Scrollbar = HoriztonalScrollbar;

            NextElementElement?.onClick.AddListener(() =>
            {
                _ButtonPressed = true;
                MoveScreen(1);
            });
            PreviousElementElement?.onClick.AddListener(() =>
            {
                _ButtonPressed = true;
                MoveScreen(-1);
            });

            _Scrollbar?.onValueChanged.AddListener((amt) =>
            {
                if (!_ButtonPressed && !_IsDragging)
                {
                    if (_Data != null)
                    {
                        float step = 1.0f / _Data.Count;
                        int index = Mathf.FloorToInt(amt / step);
                        LerpToElement(index);
                    }
                }
            });

            _Initialized = true;
        }
    }
}