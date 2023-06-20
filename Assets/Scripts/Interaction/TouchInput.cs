using System;
using DigitalRubyShared;
using Networking;
using UnityEngine;

namespace Interaction
{
    /// <summary>
    /// Derived from the DemoScript of FingerLite
    /// https://github.com/DigitalRuby/FingersGestures/blob/master/Assets/FingersLite/Demo/DemoScript.cs
    /// </summary>
    public class TouchInput : MonoBehaviour
    {
        [SerializeField]
        private Client client;

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
                client.HandleTapMessage(TapType.Single);
            }
        }

        private void DoubleTapGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                client.HandleTapMessage(TapType.Double);
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
                    client.HandleSwipeMessage(isInwardSwipe, gesture.FocusX, gesture.FocusY, (float)angle);
                }
            }
        }

        private void ScaleGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                client.HandleScaleMessage(scaleGesture.ScaleMultiplier);
            }
        }

        private void RotateGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                client.HandleRotateMessage(rotateGesture.RotationRadiansDelta * -1);
            }
        }

        private void LongPressGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Began)
            {
                client.HandleTapMessage(TapType.HoldStart);
            }
            else if (gesture.State == GestureRecognizerState.Ended)
            {
                client.HandleTapMessage(TapType.HoldEnd);
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
