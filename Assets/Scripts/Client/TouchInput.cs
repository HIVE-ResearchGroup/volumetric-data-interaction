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
        [SerializeField]
        private Networking.Client client;

        private TapGestureRecognizer tapGesture;
        private TapGestureRecognizer doubleTapGesture;
        private SwipeGestureRecognizer swipeGesture;
        private ScaleGestureRecognizer scaleGesture;
        private RotateGestureRecognizer rotateGesture;
        private LongPressGestureRecognizer longPressGesture;

        private float outterAreaSize = 0.2f;
        private Vector2 outterSwipeAreaBottomLeft;
        private Vector2 outterSwipeAreaTopRight;

        private void Start()
        {
            var areaWidth = Screen.width * outterAreaSize;
            var areaHeight = Screen.height * outterAreaSize;
            outterSwipeAreaBottomLeft = new Vector2(areaWidth, areaHeight);
            outterSwipeAreaTopRight = new Vector2(Screen.width - areaWidth, Screen.height - areaHeight);

            doubleTapGesture = new TapGestureRecognizer();
            doubleTapGesture.NumberOfTapsRequired = 2;
            doubleTapGesture.StateUpdated += DoubleTapGestureCallback;
            
            tapGesture = new TapGestureRecognizer();
            tapGesture.RequireGestureRecognizerToFail = doubleTapGesture;
            tapGesture.StateUpdated += TapGestureCallback;

            swipeGesture = new SwipeGestureRecognizer
            {
                Direction = SwipeGestureRecognizerDirection.Any,
                DirectionThreshold = 1.0f // allow a swipe, regardless of slope
            };
            swipeGesture.StateUpdated += SwipeGestureCallback;
            
            scaleGesture = new ScaleGestureRecognizer();
            scaleGesture.StateUpdated += ScaleGestureCallback;
            
            rotateGesture = new RotateGestureRecognizer();
            rotateGesture.StateUpdated += RotateGestureCallback;
            
            longPressGesture = new LongPressGestureRecognizer();
            longPressGesture.MaximumNumberOfTouchesToTrack = 1;
            longPressGesture.StateUpdated += LongPressGestureCallback;
            
            FingersScript.Instance.AddGesture(doubleTapGesture);
            FingersScript.Instance.AddGesture(tapGesture);
            FingersScript.Instance.AddGesture(swipeGesture);
            FingersScript.Instance.AddGesture(scaleGesture);
            FingersScript.Instance.AddGesture(rotateGesture);
            FingersScript.Instance.AddGesture(longPressGesture);

            scaleGesture.AllowSimultaneousExecution(rotateGesture);
        }

        private void TapGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                client.SendTapMessage(TapType.Single, gesture.FocusX, gesture.FocusY);
            }
        }

        private void DoubleTapGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                client.SendTapMessage(TapType.Double, gesture.FocusX, gesture.FocusY);
            }
        }

        private void SwipeGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                var isStartEdgeArea = IsWithinEdgeArea(swipeGesture.StartFocusX, swipeGesture.StartFocusY);
                var isEndEdgeArea = IsWithinEdgeArea(gesture.FocusX, gesture.FocusY);

                if (isStartEdgeArea || isEndEdgeArea)
                {
                    var isInwardSwipe = IsInwardSwipe(swipeGesture.StartFocusX, swipeGesture.StartFocusY, gesture.FocusX, gesture.FocusY);

                    var angle = Math.Atan2(Screen.height / 2.0 - gesture.FocusY, gesture.FocusX -  Screen.width / 2.0) * Mathf.Rad2Deg;
                    client.SendSwipeMessage(isInwardSwipe, gesture.FocusX, gesture.FocusY, (float)angle);
                }
            }
        }

        private void ScaleGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                client.SendScaleMessage(scaleGesture.ScaleMultiplier);
            }
        }

        private void RotateGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                client.SendRotateMessage(rotateGesture.RotationRadiansDelta * -1);
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
                    client.SendTapMessage(TapType.HoldStart, xUV, yUV);
                    break;
                case GestureRecognizerState.Ended:
                    client.SendTapMessage(TapType.HoldEnd, xUV, yUV);
                    break;
            }
        }

        /// <summary>
        /// There is a small area on the edge of the touchscreen
        /// Swipes can only be executed in this area
        /// </summary>
        private bool IsWithinEdgeArea(float x, float y)
        {
            if (x > outterSwipeAreaBottomLeft.x && x < outterSwipeAreaTopRight.x &&
                y > outterSwipeAreaBottomLeft.y && y < outterSwipeAreaTopRight.y)
            {
                return false;
            }

            return x > 0 && x < Screen.width &&
                   y > 0 && y < Screen.height;
        }

        /// <summary>
        /// Check if start or end of swipe is further away from screen center
        /// This allows to specify if the swipe was inward or outward
        /// </summary>
        private bool IsInwardSwipe(float startX, float startY, float endX, float endY)
        {
            var screenCenter = new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);

            var distanceStartMiddle = Mathf.Abs(Vector2.Distance(new Vector2(startX, startY), screenCenter));
            var distanceEndMiddle = Mathf.Abs(Vector2.Distance(new Vector2(endX, endY), screenCenter));

            return distanceStartMiddle > distanceEndMiddle;
        }
    }
}
