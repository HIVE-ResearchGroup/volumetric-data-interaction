using System;
using Constants;
using DigitalRubyShared;
using Networking;
using UnityEngine;
using UnityEngine.Serialization;

namespace Interaction
{
    /// <summary>
    /// Derived from the DemoScript of FingerLite
    /// https://github.com/DigitalRuby/FingersGestures/blob/master/Assets/FingersLite/Demo/DemoScript.cs
    /// </summary>
    public class TouchInput : MonoBehaviour
    {
        [SerializeField] private NetworkingCommunicator comm;
        
        private TapGestureRecognizer tapGesture;
        private TapGestureRecognizer doubleTapGesture;
        private TapGestureRecognizer tripleTapGesture;
        private SwipeGestureRecognizer swipeGesture;
        private ScaleGestureRecognizer scaleGesture;
        private RotateGestureRecognizer rotateGesture;
        private LongPressGestureRecognizer longPressGesture;

        private float outterAreaSize = 0.2f;
        private Vector2 outterSwipeAreaBottomLeft;
        private Vector2 outterSwipeAreaTopRight;

        private Client client;

        private void TapGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                comm.TapServerRpc(TabType.Single);
                client.HandleTabMessage(TabType.Single);
            }
        }

        private void CreateTapGesture()
        {
            tapGesture = new TapGestureRecognizer();
            tapGesture.StateUpdated += TapGestureCallback;
            tapGesture.RequireGestureRecognizerToFail = doubleTapGesture;
            FingersScript.Instance.AddGesture(tapGesture);
        }

        private void DoubleTapGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                comm.TapServerRpc(TabType.Double);
                client.HandleTabMessage(TabType.Double);
            }
        }

        private void CreateDoubleTapGesture()
        {
            doubleTapGesture = new TapGestureRecognizer();
            doubleTapGesture.NumberOfTapsRequired = 2;
            doubleTapGesture.StateUpdated += DoubleTapGestureCallback;
            doubleTapGesture.RequireGestureRecognizerToFail = tripleTapGesture;
            FingersScript.Instance.AddGesture(doubleTapGesture);
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
                    comm.SwipeServerRpc(isInwardSwipe, gesture.FocusX, gesture.FocusY, (float)angle);
                    client.HandleSwipeMessage();
                }
            }
        }

        private void CreateSwipeGesture()
        {
            swipeGesture = new SwipeGestureRecognizer
            {
                Direction = SwipeGestureRecognizerDirection.Any,
                DirectionThreshold = 1.0f // allow a swipe, regardless of slope
            };
            swipeGesture.StateUpdated += SwipeGestureCallback;
            FingersScript.Instance.AddGesture(swipeGesture);
        }

        private void ScaleGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                comm.ScaleServerRpc(scaleGesture.ScaleMultiplier);
            }
        }

        private void CreateScaleGesture()
        {
            scaleGesture = new ScaleGestureRecognizer();
            scaleGesture.StateUpdated += ScaleGestureCallback;
            FingersScript.Instance.AddGesture(scaleGesture);
        }

        private void RotateGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                comm.RotateServerRpc(rotateGesture.RotationRadiansDelta * -1);
            }
        }

        private void CreateRotateGesture()
        {
            rotateGesture = new RotateGestureRecognizer();
            rotateGesture.StateUpdated += RotateGestureCallback;
            FingersScript.Instance.AddGesture(rotateGesture);
        }

        private void LongPressGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Began)
            {
                comm.TapServerRpc(TabType.HoldStart);
                client.HandleTabMessage(TabType.HoldStart);
            }
            else if (gesture.State == GestureRecognizerState.Ended)
            {
                comm.TapServerRpc(TabType.HoldEnd);
                client.HandleTabMessage(TabType.HoldEnd);
            }
        }

        private void CreateLongPressGesture()
        {
            longPressGesture = new LongPressGestureRecognizer();
            longPressGesture.MaximumNumberOfTouchesToTrack = 1;
            longPressGesture.StateUpdated += LongPressGestureCallback;
            FingersScript.Instance.AddGesture(longPressGesture);
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

        private void Start()
        {
            var obj = GameObject.Find(StringConstants.Client);
            if (obj.TryGetComponent(out Client client))
            {
                this.client = client;
            }
            
            var areaWidth = Screen.width * outterAreaSize;
            var areaHeight = Screen.height * outterAreaSize;
            outterSwipeAreaBottomLeft = new Vector2(areaWidth, areaHeight);
            outterSwipeAreaTopRight = new Vector2(Screen.width - areaWidth, Screen.height - areaHeight);

            CreateDoubleTapGesture();
            CreateTapGesture();
            CreateSwipeGesture();
            CreateScaleGesture();
            CreateRotateGesture();
            CreateLongPressGesture();

            scaleGesture.AllowSimultaneousExecution(rotateGesture);
        }
    }
}
