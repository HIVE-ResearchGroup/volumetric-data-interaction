#nullable enable

using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;

/// <summary>
/// Allows to switch between a classic and a HMD display, supporting both VR and AR modes
/// </summary>
public class ViewModeSetter : MonoBehaviour
{
    public static ViewModeSetter Instance { get; private set; } = null!;
    
    /// <summary>
    /// Holds a viewing mode
    /// </summary>
    public enum ViewMode
    {
        Display = 0,
        VR = 1,
        AR = 2
    }

    [SerializeField]
    private Camera displayCamera = null!;

    [SerializeField]
    private Camera xrCamera = null!;
    
    [SerializeField]
    private ViewMode viewMode = ViewMode.Display;

    [Header("Regular Display")]
    [SerializeField]
    private List<GameObject> displayObjects = new();

    [Header("VR")]
    [SerializeField]
    private List<GameObject> vrObjects = new();

    [Header("AR")]
    [SerializeField]
    private List<GameObject> arObjects = new();
        
    public ViewMode CurrentViewMode
    {
        get => viewMode;
        set
        {
            if (value == viewMode)
            {
                return;
            }
            viewMode = value;
            RefreshViewMode();
        }
    }

    public Camera Camera => CurrentViewMode == ViewMode.Display ? displayCamera : xrCamera;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
            // don't initialize, XR will autoinitialize if set in "Project Settings" -> "XR Plug-in Management"
            //StartCoroutine(XRGeneralSettings.Instance.Manager.InitializeLoader());
            RefreshViewMode();
        }
        else
        {
            Destroy(this);
        }
    }

    private void RefreshViewMode()
    {
        switch (CurrentViewMode)
        {
            case ViewMode.Display:
                /*if (XRGeneralSettings.Instance.Manager != null
                    && XRGeneralSettings.Instance.Manager.isInitializationComplete)
                {
                    StopXR();
                }*/
                vrObjects.Concat(arObjects).ForEach(go => go.SetActive(false));
                displayObjects.ForEach(go => go.SetActive(true));
                break;
            case ViewMode.VR:
                /*if (XRGeneralSettings.Instance.Manager == null
                    || !XRGeneralSettings.Instance.Manager.isInitializationComplete)
                {
                    yield return StartXR();
                }*/
                displayObjects.Concat(arObjects).ForEach(go => go.SetActive(false));
                vrObjects.ForEach(go => go.SetActive(true));
                break;
            case ViewMode.AR:
                /*if (XRGeneralSettings.Instance.Manager == null
                    || !XRGeneralSettings.Instance.Manager.isInitializationComplete)
                {
                    yield return StartXR();
                }*/
                displayObjects.Concat(vrObjects).ForEach(go => go.SetActive(false));
                arObjects.ForEach(go => go.SetActive(true));
                break;
            default:
                displayObjects.Concat(vrObjects).Concat(arObjects).ForEach(go => go.SetActive(false));
                Debug.LogWarning($"Unknown ViewMode entered: {viewMode}");
                break;
        }
    }

    /*
    private static IEnumerator StartXR()
    {
        Debug.Log("Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
        }
        else
        {
            Debug.Log("Starting XR...");
            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }
    }

    private static void StopXR()
    {
        Debug.Log("Stopping XR...");
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        Debug.Log("XR stopped completely.");
    }
    */
}
