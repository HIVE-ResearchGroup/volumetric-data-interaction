using Exploration;
using Interaction;
using Unity.Netcode;
using UnityEngine;

namespace Networking
{
    public class Host : MonoBehaviour
    {
        [SerializeField]
        public MeshRenderer mainRenderer;
        [SerializeField]
        public Interaction.Exploration analysis;
        [SerializeField]
        public InterfaceVisualisation ui;
        [SerializeField]
        public SpatialInteraction spatialHandler;
        [SerializeField]
        public SnapshotInteraction snapshotHandler;
        [SerializeField]
        public GameObject ray;
        [SerializeField]
        private Slicer slicer;
        [SerializeField]
        private NetworkManager netMan;
        [SerializeField]
        private NetworkingCommunicatorProxy comm;

        private MenuMode _menuMode;
        private GameObject _selected;

        public GameObject Highlighted { get; set; }

        private void OnEnable()
        {
            netMan.StartHost();
            comm.ModeChanged += HandleModeChange;
            comm.ShakeCompleted += HandleShakes;
            comm.Tilted += HandleTilt;
            comm.Tapped += HandleTap;
            comm.Swiped += HandleSwipe;
            comm.Scaled += HandleScaling;
            comm.Rotated += HandleRotation;
            comm.TextReceived += HandleText;
            ray.SetActive(false);
        }

        private void OnDisable()
        {
            comm.ModeChanged -= HandleModeChange;
            comm.ShakeCompleted -= HandleShakes;
            comm.Tilted -= HandleTilt;
            comm.Tapped -= HandleTap;
            comm.Swiped -= HandleSwipe;
            comm.Scaled -= HandleScaling;
            comm.Rotated -= HandleRotation;
            comm.TextReceived -= HandleText;
        }
        
        public void Select(GameObject newObject)
        {
            UnselectObject();
            _selected = newObject;
        }

        #region Input Handling
        private void HandleModeChange(MenuMode mode)
        {
            if (_menuMode == mode)
            {
                return;
            }

            var isSnapshotSelected = false;
            switch(mode)
            {
                case MenuMode.None:
                    if (_menuMode == MenuMode.Analysis)
                    {
                        slicer.ActivateTemporaryCuttingPlane(false);
                    }
                    else
                    {
                        ResetFromSelectionMode();
                    }
                    break;
                case MenuMode.Selection:
                    ray.SetActive(true);
                    break;
                case MenuMode.Selected:
                    isSnapshotSelected = snapshotHandler.IsSnapshot(_selected);
                    break;
                case MenuMode.Analysis:
                    slicer.ActivateTemporaryCuttingPlane(true);
                    break;
                case MenuMode.Mapping:
                default:
                    Debug.Log($"HandleModeChange() received unhandled mode: {mode}");
                    break;
            }

            ui.SetMode(mode, isSnapshotSelected);
        }
        
        private void HandleShakes(int shakeCount)
        {
            if (shakeCount < 1) // one shake can happen unintentionally
            {
                return;
            }

            var hasDeleted = snapshotHandler.DeleteSnapshotsIfExist(_selected.GetComponent<Snapshot>(), shakeCount);
            if (!hasDeleted && shakeCount > 1)
            {
                analysis.ResetModel();
            }

            HandleModeChange(MenuMode.None);
            comm.MenuModeClientRpc(MenuMode.None);
        }

        private void HandleTilt(bool isLeft)
        {
            if (_menuMode == MenuMode.Selected)
            {
                snapshotHandler.GetNeighbour(isLeft, _selected);
            }
        }

        private void HandleTap(TapType type)
        {
            switch(type)
            {
                case TapType.Single:
                    break;
                case TapType.Double:
                    if (_menuMode == MenuMode.Selection && Highlighted != null)
                    {
                        _selected = Highlighted;
                        if (_selected.TryGetComponent(out Selectable select))
                        {
                            select.SetToSelected();
                        }

                        ray.SetActive(false);
                        Highlighted = null;

                        if (_selected.TryGetComponent(out Snapshot snap))
                        {
                            snap.SetSelected(true);
                        }
                        
                        comm.MenuModeClientRpc(MenuMode.Selected);
                    }
                    else if (_menuMode == MenuMode.Analysis)
                    {
                        slicer.TriggerSlicing();
                    }
                    break;
                case TapType.HoldStart:
                    spatialHandler.StartMapping(_selected);
                    break;
                case TapType.HoldEnd:
                    spatialHandler.StopMapping(_selected);
                    break;
                default:
                    Debug.Log($"HandleTab() received unhandled tab type: {type}");
                    break;
            }
        }

        private void HandleSwipe(bool isSwipeInward, float endX, float endY, float angle)
        {
            if (isSwipeInward)
            {
                return;
            }

            if (_menuMode == MenuMode.Analysis)
            {
                snapshotHandler.HandleSnapshotCreation(angle);
            }
        }

        /// <summary>
        /// Depending on mode, the scale input is used for resizing object or recognised as a grab gesture
        /// </summary>
        private void HandleScaling(float scaleMultiplier)
        {
            if(_menuMode == MenuMode.Selected)
            {
                _selected.transform.localScale *= scaleMultiplier;
            }
            else if (_selected is null)
            {
                snapshotHandler.AlignOrMisAlignSnapshots();
            }
        }

        private void HandleRotation(float rotationRadDelta) => spatialHandler.HandleRotation(rotationRadDelta, _selected);

        private void HandleText(string text) => Debug.Log($"Text received: {text}");
        
        #endregion //input handling

        private void ResetFromSelectionMode()
        {
            ray.SetActive(false);

            if (Highlighted is not null || _selected is not null)
            {
                UnselectObject();
                snapshotHandler.CleanUpNeighbours();
                snapshotHandler.DeactivateAllSnapshots();
            }
        }

        private void UnselectObject()
        {
            var activeObject = Highlighted ? Highlighted : _selected;
            if (activeObject.TryGetComponent(out Selectable selectable)) 
            {
                selectable.SetToDefault();
                _selected = null;
                Highlighted = null;
            }

            if (activeObject.TryGetComponent(out Snapshot snap))
            {
                snap.SetSelected(false);
            }
            mainRenderer.material.mainTexture = null;
        }
    }
}
