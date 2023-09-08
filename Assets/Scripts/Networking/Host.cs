using Exploration;
using Interaction;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;

namespace Networking
{
    public class Host : MonoBehaviour
    {
        [SerializeField]
        public MeshRenderer mainRenderer;
        [SerializeField]
        public InterfaceVisualisation ui;
        [SerializeField]
        public SpatialInteraction spatialHandler;
        [SerializeField]
        public SnapshotManager snapshotHandler;
        [SerializeField]
        public GameObject ray;
        [SerializeField]
        private Slicer slicer;
        [SerializeField]
        private NetworkManager netMan;
        [SerializeField]
        private PlayerEventEmitter playerEventEmitter;
        [SerializeField]
        private GameObject preSelected;

        [CanBeNull] private Player _player;
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
            playerEventEmitter.PlayerConnected += OnPlayerConnected;
            netMan.StartHost();
            ray.SetActive(false);

            Selected = preSelected;
        }

        private void OnDisable()
        {
            if (_player == null)
            {
                return;
            }
            _player.ModeChanged -= HandleModeChange;
            _player.ShakeCompleted -= HandleShakes;
            _player.Tilted -= HandleTilt;
            _player.Tapped -= HandleTap;
            _player.Swiped -= HandleSwipe;
            _player.Scaled -= HandleScaling;
            _player.Rotated -= HandleRotation;
            _player.RotatedAll -= HandleRotationFull;
            _player.Transform -= HandleTransform;
            _player.TextReceived -= HandleText;
        }

        #region Input Handling
        private void HandleModeChange(MenuMode mode)
        {
            if (_menuMode == mode)
            {
                return;
            }
            _menuMode = mode;

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
                    isSnapshotSelected = SnapshotManager.IsSnapshot(Selected);
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

            var hasDeleted = SnapshotManager.Instance.DeleteSnapshotsIfExist(_selSnapshot, shakeCount);
            if (!hasDeleted && shakeCount > 1)
            {
                ModelManager.Instance.ResetModel();
            }

            HandleModeChange(MenuMode.None);
            if (_player != null) _player.MenuModeClientRpc(MenuMode.None);
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
                    if (_menuMode == MenuMode.Selection && Highlighted != null)
                    {
                        Selected = Highlighted;
                        _selSelectable.Select();
                        _selSnapshot.Selected = true;

                        ray.SetActive(false);
                        Highlighted = null;

                        if (_player != null) _player.MenuModeClientRpc(MenuMode.Selected);
                    }
                    else if (_menuMode == MenuMode.Analysis)
                    {
                        slicer.Slice();
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
                snapshotHandler.CreateSnapshot(angle);
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
            else if (Selected == null)
            {
                snapshotHandler.ToggleSnapshotAlignment();
            }
        }

        private void HandleRotation(float rotationRadDelta) => spatialHandler.HandleRotation(rotationRadDelta, Selected);

        private void HandleRotationFull(Quaternion rotation) => spatialHandler.HandleRotation(rotation, Selected);
        private void HandleTransform(Vector3 offset) => spatialHandler.HandleTransform(offset, Selected);

        private void HandleText(string text) => Debug.Log($"Text received: {text}");
        
        #endregion //input handling

        private void OnPlayerConnected(Player p)
        {
            _player = p;
            if (_player == null)
            {
                return;
            }
            
            _player.ModeChanged += HandleModeChange;
            _player.ShakeCompleted += HandleShakes;
            _player.Tilted += HandleTilt;
            _player.Tapped += HandleTap;
            _player.Swiped += HandleSwipe;
            _player.Scaled += HandleScaling;
            _player.Rotated += HandleRotation;
            _player.RotatedAll += HandleRotationFull;
            _player.Transform += HandleTransform;
            _player.TextReceived += HandleText;
        }
        
        private void ResetFromSelectionMode()
        {
            ray.SetActive(false);

            if (Highlighted != null || Selected != null)
            {
                return;
            }
            Unselect();
            SnapshotManager.Instance.CleanUpNeighbours();
            SnapshotManager.Instance.DeactivateAllSnapshots();
        }

        private void Unselect()
        {
            if (Highlighted != null)
            {
                _highlightedSelectable.Unselect();
                _highlightedSnapshot.Selected = false;
            }
            else if (Selected != null)
            {
                _selSelectable.Unselect();
                _selSnapshot.Selected = false;
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
