using Constants;
using Exploration;
using Interaction;
using Unity.Netcode;
using UnityEngine;

namespace Networking
{
    public class Host : NetworkBehaviour
    {
        [SerializeField] private HostReferencesManager refMan;

        [SerializeField] private NetworkingCommunicator comm;
        
        private MenuMode _menuMode;

        private readonly Slicer _slicer;
        private GameObject _selected;

        public GameObject Highlighted { get; set; }

        private void OnEnable()
        {
            comm.ModeChanged += HandleModeChange;
            comm.ShakeCompleted += HandleShakes;
            comm.Tilted += HandleTilt;
            comm.Tapped += HandleTab;
            comm.Swiped += HandleSwipe;
            comm.Scaled += HandleScaling;
            comm.Rotated += HandleRotation;
            comm.TextReceived += HandleText;
            refMan.ray.SetActive(false);
        }

        private void OnDisable()
        {
            comm.ModeChanged -= HandleModeChange;
            comm.ShakeCompleted -= HandleShakes;
            comm.Tilted -= HandleTilt;
            comm.Tapped -= HandleTab;
            comm.Swiped -= HandleSwipe;
            comm.Scaled -= HandleScaling;
            comm.Rotated -= HandleRotation;
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
                        _slicer.ActivateTemporaryCuttingPlane(false);
                    }
                    else
                    {
                        ResetFromSelectionMode();
                    }

                    refMan.ui.SetHUD(StringConstants.MainModeInfo);
                    refMan.ui.SetCenterText(StringConstants.MainModeInfo);
                    break;
                case MenuMode.Selection:
                    refMan.ui.SetHUD(StringConstants.SelectionModeInfo);
                    refMan.ui.SetCenterText(StringConstants.SelectionModeInfo);
                    refMan.ray.SetActive(true);
                    break;
                case MenuMode.Selected:
                    isSnapshotSelected = refMan.snapshotHandler.IsSnapshot(_selected);
                    break;
                case MenuMode.Analysis:
                    refMan.ui.SetHUD(StringConstants.ExplorationModeInfo);
                    refMan.ui.SetCenterText(StringConstants.ExplorationModeInfo);
                    _slicer.ActivateTemporaryCuttingPlane(true);
                    break;
                case MenuMode.Mapping:
                default:
                    Debug.Log($"HandleModeChange() received unhandled mode: {mode}");
                    break;
            }

            refMan.ui.SetMode(mode, isSnapshotSelected);
        }
        
        private void HandleShakes(int shakeCount)
        {
            if (shakeCount < 1) // one shake can happen unintentionally
            {
                return;
            }

            var hasDeleted = refMan.snapshotHandler.DeleteSnapshotsIfExist(_selected.GetComponent<Snapshot>(), shakeCount);
            if (!hasDeleted && shakeCount > 1)
            {
                refMan.analysis.ResetModel();
            }

            HandleModeChange(MenuMode.None);
            comm.MenuModeClientRpc(MenuMode.None);
        }

        private void HandleTilt(bool isLeft)
        {
            if (_menuMode == MenuMode.Selected)
            {
                refMan.snapshotHandler.GetNeighbour(isLeft, _selected);
            }
        }

        private void HandleTab(TabType type)
        {
            switch(type)
            {
                case TabType.Single:
                    break;
                case TabType.Double:
                    if (_menuMode == MenuMode.Selection && Highlighted != null)
                    {
                        _selected = Highlighted;
                        if (_selected.TryGetComponent(out Selectable select))
                        {
                            select.SetToSelected();
                        }

                        refMan.ray.SetActive(false);
                        Highlighted = null;

                        if (_selected.TryGetComponent(out Snapshot snap))
                        {
                            snap.SetSelected(true);
                        }
                        
                        comm.MenuModeClientRpc(MenuMode.Selected);
                    }
                    else if (_menuMode == MenuMode.Analysis)
                    {
                        _slicer.TriggerSlicing();
                    }
                    break;
                case TabType.HoldStart:
                    refMan.spatialHandler.StartMapping(_selected);
                    break;
                case TabType.HoldEnd:
                    refMan.spatialHandler.StopMapping(_selected);
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
                refMan.snapshotHandler.HandleSnapshotCreation(angle);
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
                refMan.snapshotHandler.AlignOrMisAlignSnapshots();
            }
        }

        private void HandleRotation(float rotationRadDelta) => refMan.spatialHandler.HandleRotation(rotationRadDelta, _selected);

        private void ResetFromSelectionMode()
        {
            refMan.ray.SetActive(false);

            if (Highlighted is not null || _selected is not null)
            {
                UnselectObject();
                refMan.snapshotHandler.CleanUpNeighbours();
                refMan.snapshotHandler.DeactivateAllSnapshots();
            }
        }

        private void UnselectObject()
        {
            var activeObject = Highlighted ? Highlighted : _selected;
            var selectable = activeObject.GetComponent<Selectable>();
            if (selectable)
            {
                selectable.SetToDefault();
                _selected = null;
                Highlighted = null;
            }

            if (activeObject.TryGetComponent(out Snapshot snap))
            {
                snap.SetSelected(false);
            }
            refMan.mainRenderer.material.mainTexture = null;
        }

        public void ChangeSelectedObject(GameObject newObject)
        {
            UnselectObject();
            _selected = newObject;
        }
        #endregion //input handling
        
        private void HandleText(string text) => Debug.Log($"Text received: {text}");
    }
}
