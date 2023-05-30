using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;
using UnityEngine.XR.Management;

namespace Helper
{
    /// <summary>
    /// Allows to switch between a classic and a HMD display, supporting both VR and AR modes
    /// </summary>
    public class ViewModeSetter : MonoBehaviour
    {
        /// <summary>
        /// Holds a viewing mode
        /// </summary>
        public enum ViewMode
        {
            None = -1,
            Display = 0,
            VR = 1,
            AR = 2
        }

        [SerializeField] private ViewMode _viewMode = ViewMode.Display;

        [Header("Regular Display")]
        [SerializeField] private List<GameObject> m_2DObjects = new List<GameObject>();

        [Header("VR")]
        [SerializeField] private List<GameObject> m_VRObjects = new List<GameObject>();

        [Header("AR")]
        [SerializeField] private List<GameObject> m_ARObjects = new List<GameObject>();


        /// <summary>
        /// Gets or sets the current view mode
        /// </summary>
        public ViewMode CurrentViewMode
        {
            get => _viewMode;
            set
            {
                if (value != _viewMode)
                {
                    _viewMode = value;
                    RefreshViewMode(_viewMode);
                }
            }
        }

        /// <inheritdoc cref="DocStringBehaviour.Start"/>
        protected void Start()
        {
            // don't initialize, XR will autoinitialize if set in "Project Settings" -> "XR Plug-in Management"
            //StartCoroutine(XRGeneralSettings.Instance.Manager.InitializeLoader());
            RefreshViewMode(CurrentViewMode);
        }

        private void RefreshViewMode(ViewMode viewMode)
        {
            switch (viewMode)
            {
                case ViewMode.Display:
                    if (XRGeneralSettings.Instance.Manager != null
                        && XRGeneralSettings.Instance.Manager.isInitializationComplete)
                    {
                        XRGeneralSettings.Instance.Manager.StopSubsystems();
                    }
                    m_VRObjects.Concat(m_ARObjects).ForEach(go => go.SetActive(false));
                    m_2DObjects.ForEach(go => go.SetActive(true));
                    break;
                case ViewMode.VR:
                    if (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
                    {
                        Debug.LogWarning("XR Initialization is not yet complete!");
                        return;
                    }
                    XRGeneralSettings.Instance.Manager.StartSubsystems();
                    m_2DObjects.Concat(m_ARObjects).ForEach(go => go.SetActive(false));
                    m_VRObjects.ForEach(go => go.SetActive(true));
                    break;
                case ViewMode.AR:
                    if (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
                    {
                        Debug.LogWarning("XR Initialization is not yet complete!");
                        return;
                    }
                    XRGeneralSettings.Instance.Manager.StartSubsystems();
                    m_2DObjects.Concat(m_VRObjects).ForEach(go => go.SetActive(false));
                    m_ARObjects.ForEach(go => go.SetActive(true));
                    break;
                default:
                    m_2DObjects.Concat(m_VRObjects).Concat(m_ARObjects).ForEach(go => go.SetActive(false));
                    Debug.LogWarning($"Unknown ViewMode entered: {viewMode}");
                    break;
            }
        }
    }
}
