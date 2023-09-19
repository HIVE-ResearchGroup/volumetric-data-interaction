using Extensions;
using Model;
using Slicing;
using Snapshots;
using UI;
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
        public GameObject ray;
        [SerializeField]
        private Slicer slicer;
        [SerializeField]
        private GameObject tracker;
        [SerializeField]
        private NetworkManager netMan;

        private Player _player;
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
                if (value == null)
                {
                    _selSelectable = null;
                    _selSnapshot = null;
                    return;
                }
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
                if (value == null)
                {
                    _highlightedSelectable = null;
                    _highlightedSnapshot = null;
                    return;
                }
                _highlightedSelectable = _highlighted.TryGetComponent(out Selectable selectable) ? selectable : null;
                _highlightedSnapshot = _highlighted.TryGetComponent(out Snapshot snapshot) ? snapshot : null;
            }
        }

        private void OnEnable()
        {
            PlayerConnectedNotifier.OnPlayerConnected += HandlePlayerConnected;
            netMan.StartHost();
            ray.SetActive(false);

            Selected = ModelManager.Instance.CurrentModel.gameObject;
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
            _player.TextReceived -= HandleText;
        }

        #region Input Handling
        private void HandleModeChange(MenuMode mode)
        {
            Debug.Log($"Changing Menu Mode to \"{mode}\"");
            if (_menuMode == mode)
            {
                return;
            }

            var isSnapshotSelected = false;
            switch (mode)
            {
                case MenuMode.None:
                    if (_menuMode == MenuMode.Analysis)
                    {
                        slicer.DeactivateTemporaryCuttingPlane();
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
                    isSnapshotSelected = Selected.IsSnapshot();
                    break;
                case MenuMode.Analysis:
                    slicer.ActivateTemporaryCuttingPlane();
                    break;
                case MenuMode.Mapping:
                default:
                    Debug.Log($"{nameof(HandleModeChange)}() received unhandled mode: {mode}");
                    break;
            }

            ui.SetMode(mode, isSnapshotSelected);
            _menuMode = mode;
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
                SnapshotManager.Instance.GetNeighbour(isLeft, Selected);
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
                        if (_selSelectable != null)
                        {
                            _selSelectable.Select();
                        }
                        if (_selSnapshot != null)
                        {
                            _selSnapshot.Selected = true;
                        }

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
                    break;
                case TapType.HoldEnd:
                    Debug.Log($"Tap Hold End received at: ({x},{y})");
                    break;
                default:
                    Debug.Log($"{nameof(HandleTap)}() received unhandled tap type: {type}");
                    break;
            }
        }

        private void HandleSwipe(bool isSwipeInward, float endX, float endY, float angle)
        {
            // ignore inward swiped, outward swipes are used to create snapshots
            if (isSwipeInward)
            {
                return;
            }

            if (_menuMode == MenuMode.Analysis)
            {
                SnapshotManager.Instance.CreateSnapshot(angle);
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
                SnapshotManager.Instance.ToggleSnapshotAlignment();
            }
        }

        private void HandleRotation(float rotationRadDelta)
        {
            if (!Selected)
            {
                return;
            }

            var trackerTransform = tracker.transform;
            var threshold = 20.0f;
            var downAngle = 90.0f;

            if (trackerTransform.eulerAngles.x <= downAngle + threshold && trackerTransform.eulerAngles.x >= downAngle - threshold)
            {
                Selected.transform.Rotate(0.0f, rotationRadDelta * Mathf.Rad2Deg, 0.0f);
                return;
            }

            if (trackerTransform.rotation.x <= 30f && 0f <= trackerTransform.rotation.x ||
                trackerTransform.rotation.x <= 160f && 140f <= trackerTransform.rotation.x)
            {
                Selected.transform.Rotate(Vector3.up, -rotationRadDelta * Mathf.Rad2Deg);
            }
            else
            {
                Selected.transform.Rotate(Vector3.forward, rotationRadDelta * Mathf.Rad2Deg);
            }
        }

        private static void HandleText(string text) => Debug.Log($"Text received: {text}");
        
        #endregion //input handling

        private void HandlePlayerConnected(Player p)
        {
            // don't register itself
            if (p.IsLocalPlayer)
            {
                return;
            }

            if (_player != null)
            {
                Debug.LogWarning("Another player tried to register itself! There should only be one further player!");
                return;
            }
            Debug.Log("New player connected");
            _player = p;
            _player.ModeChanged += HandleModeChange;
            _player.ShakeCompleted += HandleShakes;
            _player.Tilted += HandleTilt;
            _player.Tapped += HandleTap;
            _player.Swiped += HandleSwipe;
            _player.Scaled += HandleScaling;
            _player.Rotated += HandleRotation;
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
                if (_highlightedSelectable != null)
                {
                    _highlightedSelectable.Unselect();
                }
                if (_highlightedSnapshot != null)
                {
                    _highlightedSnapshot.Selected = false;
                }
            }
            else if (Selected != null)
            {
                if (_selSelectable != null)
                {
                    _selSelectable.Select();
                }
                if (_selSnapshot != null)
                {
                    _selSnapshot.Selected = false;
                }
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
