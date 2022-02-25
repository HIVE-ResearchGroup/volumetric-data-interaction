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
    public Text Log;

    private TapGestureRecognizer tapGesture;
    private TapGestureRecognizer doubleTapGesture;
    private TapGestureRecognizer tripleTapGesture;
    private SwipeGestureRecognizer swipeGesture;
    private ScaleGestureRecognizer scaleGesture;
    private RotateGestureRecognizer rotateGesture;
    private LongPressGestureRecognizer longPressGesture;

    private GameObject draggingAsteroid;

    private float outterAreaSize = 0.2f;
    private Vector2 outterSwipeAreaBottomLeft;
    private Vector2 outterSwipeAreaTopRight;

    private void SendToClient(NetworkMessage message)
    {
        var client = GameObject.Find(StringConstants.Client)?.GetComponent<Client>();
        client?.SendServer(message);
    }

    private void BeginDrag(float screenX, float screenY)
    {
        Vector3 pos = new Vector3(screenX, screenY, 0.0f);
        pos = Camera.main.ScreenToWorldPoint(pos);

        //remove?
            longPressGesture.Reset();
    }

    private void EndDrag(float velocityXScreen, float velocityYScreen)
    {

        Vector3 origin = Camera.main.ScreenToWorldPoint(Vector3.zero);
        Vector3 end = Camera.main.ScreenToWorldPoint(new Vector3(velocityXScreen, velocityYScreen, 0.0f));
        Vector3 velocity = (end - origin);
        draggingAsteroid.GetComponent<Rigidbody2D>().velocity = velocity;
        draggingAsteroid.GetComponent<Rigidbody2D>().angularVelocity = UnityEngine.Random.Range(5.0f, 45.0f);
        draggingAsteroid = null;

        SendToClient(new TextMessage("end of drag - long tab flick velocity"));
    }

    private void TapGestureCallback(GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Ended)
        {
            SendToClient(new TabMessage(TabType.Single));
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
            SendToClient(new TabMessage(TabType.Double));
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
                SendToClient(swipeMessage);
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
            SendToClient(new ScaleMessage(scaleGesture.ScaleMultiplier));
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
            SendToClient(new RotationMessage(rotateGesture.RotationRadiansDelta * -1));
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
            BeginDrag(gesture.FocusX, gesture.FocusY);
        }
        else if (gesture.State == GestureRecognizerState.Ended)
        {
            SendToClient(new TabMessage(TabType.Hold));
            EndDrag(longPressGesture.VelocityX, longPressGesture.VelocityY);
        }
    }

    private void CreateLongPressGesture()
    {
        longPressGesture = new LongPressGestureRecognizer();
        longPressGesture.MaximumNumberOfTouchesToTrack = 1;
        longPressGesture.StateUpdated += LongPressGestureCallback;
        FingersScript.Instance.AddGesture(longPressGesture);
    }

    private static bool? CaptureGestureHandler(GameObject obj)
    {
        // I've named objects PassThrough* if the gesture should pass through and NoPass* if the gesture should be gobbled up, everything else gets default behavior
        if (obj.name.StartsWith("PassThrough"))
        {
            // allow the pass through for any element named "PassThrough*"
            return false;
        }
        else if (obj.name.StartsWith("NoPass"))
        {
            // prevent the gesture from passing through, this is done on some of the buttons and the bottom text so that only
            // the triple tap gesture can tap on it
            return true;
        }

        // fall-back to default behavior for anything else
        return null;
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

        // prevent the one special no-pass button from passing through,
        //  even though the parent scroll view allows pass through (see FingerScript.PassThroughObjects)
        FingersScript.Instance.CaptureGestureHandler = CaptureGestureHandler;
    }

    private void LateUpdate()
    {       
        int touchCount = Input.touchCount;
        if (FingersScript.Instance.TreatMousePointerAsFinger && Input.mousePresent)
        {
            touchCount += (Input.GetMouseButton(0) ? 1 : 0);
            touchCount += (Input.GetMouseButton(1) ? 1 : 0);
            touchCount += (Input.GetMouseButton(2) ? 1 : 0);
        }
        string touchIds = string.Empty;
        int gestureTouchCount = 0;
        foreach (GestureRecognizer g in FingersScript.Instance.Gestures)
        {
            gestureTouchCount += g.CurrentTrackedTouches.Count;
            if (gestureTouchCount > 0)
            {
                Debug.Log(gestureTouchCount);
            }
        }
        foreach (GestureTouch t in FingersScript.Instance.CurrentTouches)
        {
            touchIds += ":" + t.Id + ":";
        }
    }

}
