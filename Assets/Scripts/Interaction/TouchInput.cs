using DigitalRubyShared;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Derived from the DemoScript of FingerLite
/// https://github.com/DigitalRuby/FingersGestures/blob/master/Assets/FingersLite/Demo/DemoScript.cs
/// </summary>
public class TouchInput : MonoBehaviour
{
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

    private void SendToHost(NetworkMessage message)
    {
        var client = GameObject.Find(StringConstants.Client)?.GetComponent<Client>();
        client?.SendServer(message);
    }

    private void TapGestureCallback(GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Ended)
        {
            SendToHost(new TabMessage(TabType.Single));
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
            SendToHost(new TabMessage(TabType.Double));
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
                var swipeMessage = new SwipeMessage();
                swipeMessage.IsInwardSwipe = isInwardSwipe;
                swipeMessage.EndPointX = gesture.FocusX;
                swipeMessage.EndPointY = gesture.FocusY;
                SendToHost(swipeMessage);
            }
        }
    }

    private void CreateSwipeGesture()
    {
        swipeGesture = new SwipeGestureRecognizer();
        swipeGesture.Direction = SwipeGestureRecognizerDirection.Any;
        swipeGesture.StateUpdated += SwipeGestureCallback;
        swipeGesture.DirectionThreshold = 1.0f; // allow a swipe, regardless of slope
        FingersScript.Instance.AddGesture(swipeGesture);
    }

    private void ScaleGestureCallback(GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Executing)
        {
            SendToHost(new ScaleMessage(scaleGesture.ScaleMultiplier));
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
            SendToHost(new RotationMessage(rotateGesture.RotationRadiansDelta * -1));
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
            SendToHost(new TabMessage(TabType.HoldStart));
        }
        else if (gesture.State == GestureRecognizerState.Ended)
        {
            SendToHost(new TabMessage(TabType.HoldEnd));
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

        if (x > 0 && x < Screen.width &&
            y > 0 && y < Screen.height)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if start or end of swipe is further away from screen center
    /// This allows to specify if the swipe was inward or outward
    /// </summary>
    private bool IsInwardSwipe(float startX, float startY, float endX, float endY)
    {
        var screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        var distanceStartMiddle = Mathf.Abs(Vector2.Distance(new Vector2(startX, startY), screenCenter));
        var distanceEndMiddle = Mathf.Abs(Vector2.Distance(new Vector2(endX, endY), screenCenter));

        return distanceStartMiddle > distanceEndMiddle;
    }

    private void Start()
    {
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
