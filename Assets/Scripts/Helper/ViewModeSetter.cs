using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
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
            Display = 0,
            VR = 1,
            AR = 2
        }

        [SerializeField] private ViewMode viewMode = ViewMode.Display;

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
            get => viewMode;
            set
            {
                if (value != viewMode)
                {
                    viewMode = value;
                    RefreshViewMode(viewMode);
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
                    /*if (XRGeneralSettings.Instance.Manager != null
                        && XRGeneralSettings.Instance.Manager.isInitializationComplete)
                    {
                        StopXR();
                    }*/
                    m_VRObjects.Concat(m_ARObjects).ForEach(go => go.SetActive(false));
                    m_2DObjects.ForEach(go => go.SetActive(true));
                    break;
                case ViewMode.VR:
                    /*if (XRGeneralSettings.Instance.Manager == null
                        || !XRGeneralSettings.Instance.Manager.isInitializationComplete)
                    {
                        yield return StartXR();
                    }*/
                    m_2DObjects.Concat(m_ARObjects).ForEach(go => go.SetActive(false));
                    m_VRObjects.ForEach(go => go.SetActive(true));
                    break;
                case ViewMode.AR:
                    /*if (XRGeneralSettings.Instance.Manager == null
                        || !XRGeneralSettings.Instance.Manager.isInitializationComplete)
                    {
                        yield return StartXR();
                    }*/
                    m_2DObjects.Concat(m_VRObjects).ForEach(go => go.SetActive(false));
                    m_ARObjects.ForEach(go => go.SetActive(true));
                    break;
                default:
                    m_2DObjects.Concat(m_VRObjects).Concat(m_ARObjects).ForEach(go => go.SetActive(false));
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
}
