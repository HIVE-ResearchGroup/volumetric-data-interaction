using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

using Hand = Valve.VR.InteractionSystem.Hand;
using VRPlayer = Valve.VR.InteractionSystem.Player;

namespace Assets.Code.Logic
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

        private const GameObject AR_HAND_PREFAB = null;

        [SerializeField] private ViewMode initialViewMode = ViewMode.Display;

        [Header("Regular Display")]
        [SerializeField] private List<GameObject> m_2DObjects = new List<GameObject>();

        [Header("VR")]
        [Tooltip("The Steam VR VR camera can be disabled in AR to gain a little more performance")]
        [SerializeField] private Camera m_VRCamera = null;
        [SerializeField] private List<GameObject> m_VRObjects = new List<GameObject>();

        [Header("AR")]
        [SerializeField] private List<GameObject> m_ARObjects = new List<GameObject>();

        private ViewMode prevViewMode;
        private string deviceName;
        private Dictionary<SteamVR_Input_Sources, GameObject> handPrefabs;

        /// <summary>
        /// Gets or sets the current view mode
        /// </summary>
        public ViewMode CurrentViewMode
        {
            get => initialViewMode;
            set
            {
                if (value != initialViewMode)
                {
                    prevViewMode = initialViewMode;
                    initialViewMode = value;
                    StartCoroutine(RefreshViewMode(initialViewMode));
                }
            }
        }

        /// <summary>
        /// Gets whether or not the Steam VR hands are currently visible
        /// </summary>
        public bool HandsVisible
        {
            get => initialViewMode == ViewMode.VR;
        }

        /// <inheritdoc cref="DocStringBehaviour.Awake"/>
        protected void Awake()
        {
            prevViewMode = ViewMode.None;
            deviceName = XRSettings.loadedDeviceName;
            handPrefabs = new Dictionary<SteamVR_Input_Sources, GameObject>();
            StartCoroutine(RefreshViewMode(initialViewMode));
        }

        private IEnumerator RefreshViewMode(ViewMode viewMode)
        {
            IEnumerable<GameObject> toDisable;
            IEnumerable<GameObject> toEnable;
            bool isAR = false;

            switch (viewMode)
            {
                case ViewMode.Display:
                    toDisable = m_VRObjects.Concat(m_ARObjects);
                    toEnable = m_2DObjects;
                    break;
                case ViewMode.VR:
                    toDisable = m_2DObjects.Concat(m_ARObjects);
                    toEnable = m_VRObjects;
                    break;
                case ViewMode.AR:
                    toDisable = m_2DObjects.Concat(m_VRObjects);
                    toEnable = m_ARObjects;
                    isAR = true;
                    break;
                default:
                    toDisable = m_2DObjects.Concat(m_VRObjects).Concat(m_ARObjects);
                    toEnable = Enumerable.Empty<GameObject>();
                    break;
            }

            foreach (GameObject gameObject in toDisable)
            {
                gameObject.SetActive(false);
            }

            Debug.Log($"Switching view mode to {viewMode} [Device: '{deviceName}']");

            if (viewMode == ViewMode.Display) // shut off everything SteamVR related
            {
                Destroy(FindObjectOfType<SteamVR_Behaviour>());
                XRSettings.LoadDeviceByName(string.Empty);
                yield return null;

                XRSettings.enabled = false;

                foreach (GameObject gameObject in toEnable)
                {
                    gameObject.SetActive(true);
                }
            }
            else
            {
                if (!(prevViewMode == ViewMode.VR || prevViewMode == ViewMode.AR)) // init SteamVR as it is the base for both the VR and AR mode (only if it was previously shut off)
                {
                    if (string.Compare(XRSettings.loadedDeviceName, deviceName, true) != 0)
                    {
                        XRSettings.LoadDeviceByName(deviceName);
                        yield return null;
                    }
                    XRSettings.enabled = true;

                    yield return null;

                    foreach (GameObject gameObject in toEnable)
                    {
                        gameObject.SetActive(true);
                    }

                    SteamVR.Initialize(true);
                    yield return null;
                }
                else
                {
                    foreach (GameObject gameObject in toEnable)
                    {
                        gameObject.SetActive(true);
                    }
                }

                RefreshHandPrefabs();
                ToggleControllerVisibility(!isAR);
                if (m_VRCamera != null)
                {
                    m_VRCamera.enabled = !isAR;
                }
            }
        }

        private void ToggleControllerVisibility(bool visible)
        {
            VRPlayer player = VRPlayer.instance;
            foreach (Hand hand in player.hands)
            {
                if (hand != null)
                {
                    if (visible)
                    {
                        hand.SetRenderModel(handPrefabs[hand.handType]);
                    }
                    else
                    {
                        hand.SetRenderModel(AR_HAND_PREFAB);
                    }
                }
            }
        }

        private void RefreshHandPrefabs()
        {
            handPrefabs.Clear();
            VRPlayer player = VRPlayer.instance;

            foreach (Hand hand in player.hands)
            {
                if (hand != null)
                {
                    handPrefabs[hand.handType] = hand.renderModelPrefab;
                }
            }
        }
    }
}
