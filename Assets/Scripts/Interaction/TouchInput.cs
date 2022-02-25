using DigitalRubyShared;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// get height/width of screen
// define 5% area from edge - this is used for inward/outward swipes
public class TouchInput : MonoBehaviour
{
    public Text Log;

    private TapGestureRecognizer tapGesture;
    private TapGestureRecognizer doubleTapGesture;
    private TapGestureRecognizer tripleTapGesture;
    private SwipeGestureRecognizer swipeGesture;
    private PanGestureRecognizer panGesture;
    private ScaleGestureRecognizer scaleGesture;
    private RotateGestureRecognizer rotateGesture;
    private LongPressGestureRecognizer longPressGesture;

    private float nextAsteroid = float.MinValue;
    private GameObject draggingAsteroid;

    private readonly List<Vector3> swipeLines = new List<Vector3>();

    private void DebugText(string text, params object[] format)
    {
        Debug.Log(string.Format(text, format));
    }

    private void SendTabMessage(TabType type)
    {
        var msg = new TabMessage(type);
        SendToClient(msg);
    }

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

    private void DragTo(float screenX, float screenY)
    {
        //Vector3 pos = new Vector3(screenX, screenY, 0.0f);
        //pos = Camera.main.ScreenToWorldPoint(pos);
        //draggingAsteroid.GetComponent<Rigidbody2D>().MovePosition(pos);
    }

    private void EndDrag(float velocityXScreen, float velocityYScreen)
    {

        Vector3 origin = Camera.main.ScreenToWorldPoint(Vector3.zero);
        Vector3 end = Camera.main.ScreenToWorldPoint(new Vector3(velocityXScreen, velocityYScreen, 0.0f));
        Vector3 velocity = (end - origin);
        draggingAsteroid.GetComponent<Rigidbody2D>().velocity = velocity;
        draggingAsteroid.GetComponent<Rigidbody2D>().angularVelocity = UnityEngine.Random.Range(5.0f, 45.0f);
        draggingAsteroid = null;

        DebugText("Long tap flick velocity: {0}", velocity);
    }

    //swipe - get direction, inward, outward
    private void HandleSwipe(float endX, float endY)
    {
        Vector2 start = new Vector2(swipeGesture.StartFocusX, swipeGesture.StartFocusY);
        Vector3 startWorld = Camera.main.ScreenToWorldPoint(start);
        Vector3 endWorld = Camera.main.ScreenToWorldPoint(new Vector2(endX, endY));
        float distance = Vector3.Distance(startWorld, endWorld);
        startWorld.z = endWorld.z = 0.0f;

        swipeLines.Add(startWorld);
        swipeLines.Add(endWorld);

        if (swipeLines.Count > 4)
        {
            swipeLines.RemoveRange(0, swipeLines.Count - 4);
        }


        Vector3 origin = Camera.main.ScreenToWorldPoint(Vector3.zero);
        Vector3 end = Camera.main.ScreenToWorldPoint(new Vector3(swipeGesture.VelocityX, swipeGesture.VelocityY, Camera.main.nearClipPlane));
        Vector3 velocity = (end - origin);

        // TODO
        // check if within side-border
        // check if inward or outward
    }

    private void TapGestureCallback(GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Ended)
        {
            DebugText("Tapped at {0}, {1}", gesture.FocusX, gesture.FocusY);
            SendTabMessage(TabType.Single);
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
            DebugText("Double tapped at {0}, {1}", gesture.FocusX, gesture.FocusY);
            SendTabMessage(TabType.Double);
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
            HandleSwipe(gesture.FocusX, gesture.FocusY);
            DebugText("Swiped from {0},{1} to {2},{3}; velocity: {4}, {5}", gesture.StartFocusX, gesture.StartFocusY, gesture.FocusX, gesture.FocusY, swipeGesture.VelocityX, swipeGesture.VelocityY);
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

    private void PanGestureCallback(GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Executing)
        {
            DebugText("Panned, Location: {0}, {1}, Delta: {2}, {3}", gesture.FocusX, gesture.FocusY, gesture.DeltaX, gesture.DeltaY);

            float deltaX = panGesture.DeltaX / 25.0f;
            float deltaY = panGesture.DeltaY / 25.0f;
            //Vector3 pos = Earth.transform.position;
            //pos.x += deltaX;
            //pos.y += deltaY;
            //Earth.transform.position = pos;
        }
    }

    private void CreatePanGesture()
    {
        panGesture = new PanGestureRecognizer();
        panGesture.MinimumNumberOfTouchesToTrack = 2;
        panGesture.StateUpdated += PanGestureCallback;
        FingersScript.Instance.AddGesture(panGesture);
    }

    private void ScaleGestureCallback(GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Executing)
        {
            DebugText("Scaled: {0}, Focus: {1}, {2}", scaleGesture.ScaleMultiplier, scaleGesture.FocusX, scaleGesture.FocusY);
            //Earth.transform.localScale *= scaleGesture.ScaleMultiplier;
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
            //Earth.transform.Rotate(0.0f, 0.0f, rotateGesture.RotationRadiansDelta * Mathf.Rad2Deg);
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
            DebugText("Long press began: {0}, {1}", gesture.FocusX, gesture.FocusY);
            BeginDrag(gesture.FocusX, gesture.FocusY);
        }
        else if (gesture.State == GestureRecognizerState.Executing)
        {
            DebugText("Long press moved: {0}, {1}", gesture.FocusX, gesture.FocusY);
            DragTo(gesture.FocusX, gesture.FocusY);
        }
        else if (gesture.State == GestureRecognizerState.Ended)
        {
            DebugText("Long press end: {0}, {1}, delta: {2}, {3}", gesture.FocusX, gesture.FocusY, gesture.DeltaX, gesture.DeltaY);
            EndDrag(longPressGesture.VelocityX, longPressGesture.VelocityY);
            SendTabMessage(TabType.Hold);
        }
    }

    private void CreateLongPressGesture()
    {
        longPressGesture = new LongPressGestureRecognizer();
        longPressGesture.MaximumNumberOfTouchesToTrack = 1;
        longPressGesture.StateUpdated += LongPressGestureCallback;
        FingersScript.Instance.AddGesture(longPressGesture);
    }

    private void PlatformSpecificViewTapUpdated(GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Ended)
        {
            Debug.Log("You triple tapped the platform specific label!");
        }
    }

    //private void CreatePlatformSpecificViewTripleTapGesture()
    //{
    //    tripleTapGesture = new TapGestureRecognizer();
    //    tripleTapGesture.StateUpdated += PlatformSpecificViewTapUpdated;
    //    tripleTapGesture.NumberOfTapsRequired = 3;
    //    //tripleTapGesture.PlatformSpecificView = bottomLabel.gameObject;
    //    FingersScript.Instance.AddGesture(tripleTapGesture);
    //}

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

    private void Start()
    {
        //CreatePlatformSpecificViewTripleTapGesture();
        CreateDoubleTapGesture();
        CreateTapGesture();
        CreateSwipeGesture();
        CreatePanGesture();
        CreateScaleGesture();
        CreateRotateGesture();
        CreateLongPressGesture();

        // pan, scale and rotate can all happen simultaneously
        panGesture.AllowSimultaneousExecution(scaleGesture);
        panGesture.AllowSimultaneousExecution(rotateGesture);
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
            Debug.Log("touches!");
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

        //use this!
        //dpiLabel.text = "Dpi: " + DeviceInfo.PixelsPerInch + System.Environment.NewLine +
        //    "Width: " + Screen.width + System.Environment.NewLine +
        //    "Height: " + Screen.height + System.Environment.NewLine +
        //    "Touches: " + FingersScript.Instance.CurrentTouches.Count + " (" + gestureTouchCount + "), ids" + touchIds + System.Environment.NewLine;
    }

}
