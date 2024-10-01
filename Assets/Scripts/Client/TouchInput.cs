#nullable enable

using System;
using DigitalRubyShared;
using UnityEngine;

namespace Client
{
    /// <summary>
    /// Derived from the DemoScript of FingerLite
    /// https://github.com/DigitalRuby/FingersGestures/blob/master/Assets/FingersLite/Demo/DemoScript.cs
    /// </summary>
    public class TouchInput : MonoBehaviour
    {
        public event Action<bool, float, float, float>? Swiped;
        public event Action<float>? Scaled;
        public event Action<TapType, float, float>? Tapped;

        private TapGestureRecognizer _tapGesture = null!;
        private TapGestureRecognizer _doubleTapGesture = null!;
        private SwipeGestureRecognizer _swipeGesture = null!;
        private ScaleGestureRecognizer _scaleGesture = null!;
        private LongPressGestureRecognizer _longPressGesture = null!;

        // private const float OutterAreaSize = 0.2f;
        // private Vector2 outterSwipeAreaBottomLeft;
        // private Vector2 outterSwipeAreaTopRight;

        private void Awake()
        {
            // var areaWidth = Screen.width * OutterAreaSize;
            // var areaHeight = Screen.height * OutterAreaSize;
            // outterSwipeAreaBottomLeft = new Vector2(areaWidth, areaHeight);
            // outterSwipeAreaTopRight = new Vector2(Screen.width - areaWidth, Screen.height - areaHeight);

            _doubleTapGesture = new TapGestureRecognizer();
            _doubleTapGesture.NumberOfTapsRequired = 2;
            _doubleTapGesture.StateUpdated += DoubleTapGestureCallback;
            
            _tapGesture = new TapGestureRecognizer();
            _tapGesture.RequireGestureRecognizerToFail = _doubleTapGesture;
            _tapGesture.StateUpdated += TapGestureCallback;

            _swipeGesture = new SwipeGestureRecognizer
            {
                Direction = SwipeGestureRecognizerDirection.Any,
                DirectionThreshold = 1.0f // allow a swipe, regardless of slope
            };
            _swipeGesture.StateUpdated += SwipeGestureCallback;
            
            _scaleGesture = new ScaleGestureRecognizer();
            _scaleGesture.StateUpdated += ScaleGestureCallback;
            
            _longPressGesture = new LongPressGestureRecognizer();
            _longPressGesture.MaximumNumberOfTouchesToTrack = 1;
            _longPressGesture.StateUpdated += LongPressGestureCallback;
        }

        private void Start()
        {
            FingersScript.Instance.AddGesture(_doubleTapGesture);
            FingersScript.Instance.AddGesture(_tapGesture);
            FingersScript.Instance.AddGesture(_swipeGesture);
            FingersScript.Instance.AddGesture(_scaleGesture);
            FingersScript.Instance.AddGesture(_longPressGesture);
        }

        private void TapGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                Tapped?.Invoke(TapType.Single, gesture.FocusX, gesture.FocusY);
            }
        }

        private void DoubleTapGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                Tapped?.Invoke(TapType.Double, gesture.FocusX, gesture.FocusY);
            }
        }

        private void SwipeGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                //var isStartEdgeArea = IsWithinEdgeArea(_swipeGesture.StartFocusX, _swipeGesture.StartFocusY);
                //var isEndEdgeArea = IsWithinEdgeArea(gesture.FocusX, gesture.FocusY);

                //if (isStartEdgeArea || isEndEdgeArea)
                //{
                    var isInwardSwipe = IsInwardSwipe(_swipeGesture.StartFocusX, _swipeGesture.StartFocusY, gesture.FocusX, gesture.FocusY);

                    // instead of calculating the angle from the center of the screen, start from the first touch point
                    //var angle = Math.Atan2(Screen.height / 2.0 - gesture.FocusY, gesture.FocusX -  Screen.width / 2.0) * Mathf.Rad2Deg;
                    var angle = Mathf.Atan2(gesture.StartFocusY - gesture.FocusY, gesture.FocusX - gesture.StartFocusX) * Mathf.Rad2Deg;
                    Debug.Log($"Swipe angle: {angle}, is inward: {isInwardSwipe}");
                    Swiped?.Invoke(isInwardSwipe, gesture.FocusX, gesture.FocusY, angle);
                //}
            }
        }

        private void ScaleGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                Scaled?.Invoke(_scaleGesture.ScaleMultiplier);
            }
        }

        private void LongPressGestureCallback(GestureRecognizer gesture)
        {
            // calculate the horizontal orientation by hand!!!
            // the phone could be vertical -> width and height are oriented differently
            var sides = Screen.width > Screen.height ? (Screen.width, Screen.height) : (Screen.height, Screen.width);

            var xUV = gesture.FocusX / sides.Item1;
            var yUV = gesture.FocusY / sides.Item2;
            switch (gesture.State)
            {
                case GestureRecognizerState.Began:
                    Tapped?.Invoke(TapType.HoldBegin, xUV, yUV);
                    break;
                case GestureRecognizerState.Ended:
                    Tapped?.Invoke(TapType.HoldEnd, xUV, yUV);
                    break;
            }
        }

        // /// <summary>
        // /// There is a small area on the edge of the touchscreen
        // /// Swipes can only be executed in this area
        // /// </summary>
        // private bool IsWithinEdgeArea(float x, float y)
        // {
        //     if (x > outterSwipeAreaBottomLeft.x && x < outterSwipeAreaTopRight.x &&
        //         y > outterSwipeAreaBottomLeft.y && y < outterSwipeAreaTopRight.y)
        //     {
        //         return false;
        //     }
        //
        //     return x > 0 && x < Screen.width &&
        //            y > 0 && y < Screen.height;
        // }

        /// <summary>
        /// Check if start or end of swipe is further away from screen center
        /// This allows to specify if the swipe was inward or outward
        /// </summary>
        private static bool IsInwardSwipe(float startX, float startY, float endX, float endY)
        {
            var screenCenter = new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);

            var distanceStartMiddle = Mathf.Abs(Vector2.Distance(new Vector2(startX, startY), screenCenter));
            var distanceEndMiddle = Mathf.Abs(Vector2.Distance(new Vector2(endX, endY), screenCenter));

            return distanceStartMiddle > distanceEndMiddle;
        }
    }
}
