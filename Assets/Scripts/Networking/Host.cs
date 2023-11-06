using Extensions;
using Model;
using Slicing;
using Selection;
using Snapshots;
using Unity.Netcode;
using UnityEngine;

namespace Networking
{
    public class Host : MonoBehaviour
    {
        public static Host Instance { get; private set; }
        
        [SerializeField]
        private InterfaceController ui;
        [SerializeField]
        private GameObject ray;
        [SerializeField]
        private Slicer slicer;
        [SerializeField]
        private GameObject tracker;
        [SerializeField]
        private NetworkManager netMan;

        private Player _player;
        private MenuMode _menuMode;
        
        private Selectable _selected;
        private Selectable _highlighted;

        public Selectable Selected
        {
            get => _selected;
            set
            {
                Unselect();
                _selected = value;
            }
        }

        public Selectable Highlighted
        {
            get => _highlighted;
            set
            {
                if (_highlighted != null)
                {
                    _highlighted.IsSelected = false;
                }

                _highlighted = value;
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }
        }

        private void OnEnable()
        {
            PlayerConnectedNotifier.OnPlayerConnected += HandlePlayerConnected;
        }

        private void Start()
        {
            netMan.StartHost();
            ray.SetActive(false);

            Selected = ModelManager.Instance.CurrentModel.Selectable;
        }

        private void OnDisable()
        {
            PlayerConnectedNotifier.OnPlayerConnected -= HandlePlayerConnected;
            DeregisterPlayer();
        }

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
            RegisterPlayer();
        }

        private void RegisterPlayer()
        {
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
            _player.TextReceived += HandleText;
        }

        private void DeregisterPlayer()
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
        
        #region Player Callbacks
        
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
                        SnapshotManager.Instance.DeactivateAllSnapshots();
                    }
                    else
                    {
                        ray.SetActive(false);

                        Unselect();
                        SnapshotManager.Instance.DeleteAllNeighbours();
                        SnapshotManager.Instance.DeactivateAllSnapshots();
                    }
                    break;
                case MenuMode.Selection:
                    ray.SetActive(true);
                    break;
                case MenuMode.Selected:
                    isSnapshotSelected = Selected.gameObject.IsSnapshot();
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
            if (shakeCount <= 1) // one shake can happen unintentionally
            {
                return;
            }

            if (Selected && Selected.TryGetComponent(out Snapshot snapshot))
            {
                SnapshotManager.Instance.DeleteSnapshot(snapshot);
            }
            else
            {
                var result = SnapshotManager.Instance.DeleteAllSnapshots();
                if (!result)
                {
                    ModelManager.Instance.CurrentModel.ResetModel();
                }
            }

            HandleModeChange(MenuMode.None);
            if (_player != null) _player.MenuModeClientRpc(MenuMode.None);
        }

        private void HandleTilt(bool isLeft)
        {
            if (_menuMode != MenuMode.Selected)
            {
                return;
            }
            
            if (Selected.TryGetComponent(out Snapshot snapshot))
            {
                SnapshotManager.Instance.CreateNeighbour(snapshot, isLeft);
            }
        }

        private void HandleTap(TapType type, float x, float y)
        {
            switch(type)
            {
                case TapType.Single:
                    Debug.Log($"Single Tap received at ({x},{y})");
                    break;
                case TapType.Double:
                    Debug.Log($"Double Tap received at ({x},{y})");
                    if (_menuMode == MenuMode.Selection && Highlighted != null)
                    {
                        Selected = Highlighted;
                        Selected.IsSelected = true;

                        ray.SetActive(false);
                        Highlighted = null;

                        if (_player != null) _player.MenuModeClientRpc(MenuMode.Selected);
                        HandleModeChange(MenuMode.Selected);
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
                // TODO check scaleMultiplier to identify attach and detach commands
                SnapshotManager.Instance.ToggleSnapshotsAttached();
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
        
        #endregion

        private void Unselect()
        {
            if (Highlighted != null)
            {
                Highlighted.IsSelected = false;
            }
            else if (Selected != null)
            {
                Selected.IsSelected = false;
            }

            // manually set to null, as "IsSelected = null" can cause stack overflows through the constant calls to Unselect()
            _selected = null;
            Highlighted = null;
            ui.SetMode(MenuMode.None);
        }
    }
}
