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
        [SerializeField]
        private GameObject preSelected;

        private MenuMode _menuMode;
        
        private GameObject _selected;
        private Selectable _selSelectable;
        private Snapshot _selSnapshot;

        public GameObject Selected
        {
            get => _selected;
            set
            {
                Unselect();
                _selected = value;
                _selSelectable = _selected.TryGetComponent(out Selectable selectable) ? selectable : null;
                _selSnapshot = _selected.TryGetComponent(out Snapshot snapshot) ? snapshot : null;
            }
        }

        private GameObject _highlighted;
        private Selectable _highlightedSelectable;
        private Snapshot _highlightedSnapshot;

        public GameObject Highlighted
        {
            get => _highlighted;
            set
            {
                _highlighted = value;
                _highlightedSelectable = _highlighted.TryGetComponent(out Selectable selectable) ? selectable : null;
                _highlightedSnapshot = _highlighted.TryGetComponent(out Snapshot snapshot) ? snapshot : null;
            }
        }

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
            comm.RotatedAll += HandleRotationFull;
            comm.Transform += HandleTransform;
            comm.TextReceived += HandleText;
            ray.SetActive(false);

            Selected = preSelected;
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
            comm.RotatedAll -= HandleRotationFull;
            comm.Transform -= HandleTransform;
            comm.TextReceived -= HandleText;
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
                    isSnapshotSelected = snapshotHandler.IsSnapshot(Selected);
                    break;
                case MenuMode.Analysis:
                    slicer.ActivateTemporaryCuttingPlane(true);
                    break;
                case MenuMode.Mapping:
                default:
                    Debug.Log($"{nameof(HandleModeChange)}() received unhandled mode: {mode}");
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

            var hasDeleted = snapshotHandler.DeleteSnapshotsIfExist(_selSnapshot, shakeCount);
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
                snapshotHandler.GetNeighbour(isLeft, Selected);
            }
        }

        private void HandleTap(TapType type, float x, float y)
        {
            switch(type)
            {
                case TapType.Single:
                    break;
                case TapType.Double:
                    if (_menuMode == MenuMode.Selection && Highlighted is not null)
                    {
                        Selected = Highlighted;
                        _selSelectable.SetToSelected();
                        _selSnapshot.SetSelected(true);

                        ray.SetActive(false);
                        Highlighted = null;
                        
                        comm.MenuModeClientRpc(MenuMode.Selected);
                    }
                    else if (_menuMode == MenuMode.Analysis)
                    {
                        slicer.TriggerSlicing();
                    }
                    break;
                case TapType.HoldStart:
                    Debug.Log($"Tap Hold Start received at: ({x},{y})");
                    if (x <= 0.5)
                    {
                        spatialHandler.RotationMapping = true;
                    }
                    else
                    {
                        spatialHandler.TransformMapping = true;
                    }
                    break;
                case TapType.HoldEnd:
                    Debug.Log($"Tap Hold End received at: ({x},{y})");
                    if (x <= 0.5)
                    {
                        spatialHandler.RotationMapping = false;
                    }
                    else
                    {
                        spatialHandler.TransformMapping = false;
                    }
                    break;
                default:
                    Debug.Log($"{nameof(HandleTap)}() received unhandled tap type: {type}");
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
                Selected.transform.localScale *= scaleMultiplier;
            }
            else if (Selected is null)
            {
                snapshotHandler.AlignOrMisAlignSnapshots();
            }
        }

        private void HandleRotation(float rotationRadDelta) => spatialHandler.HandleRotation(rotationRadDelta, Selected);

        private void HandleRotationFull(Quaternion rotation) => spatialHandler.HandleRotation(rotation, Selected);
        private void HandleTransform(Vector3 offset) => spatialHandler.HandleTransform(offset, Selected);

        private void HandleText(string text) => Debug.Log($"Text received: {text}");
        
        #endregion //input handling

        private void ResetFromSelectionMode()
        {
            ray.SetActive(false);

            if (Highlighted is not null || Selected is not null)
            {
                Unselect();
                snapshotHandler.CleanUpNeighbours();
                snapshotHandler.DeactivateAllSnapshots();
            }
        }

        private void Unselect()
        {
            if (Highlighted is not null)
            {
                _highlightedSelectable.SetToDefault();
                _highlightedSnapshot.SetSelected(false);
            }
            else if (Selected is not null)
            {
                _selSelectable.SetToDefault();
                _selSnapshot.SetSelected(false);
            }

            // manually set to null, as "Selected = null" can cause stack overflows through the constant calls to Unselect()
            _selected = null;
            _selSelectable = null;
            _selSnapshot = null;
            _highlighted = null;
            _highlightedSelectable = null;
            _highlightedSnapshot = null;
            mainRenderer.material.mainTexture = null;
        }
    }
}
