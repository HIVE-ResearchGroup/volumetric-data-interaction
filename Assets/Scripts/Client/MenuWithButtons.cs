#nullable enable

using Networking.Tablet;
using TMPro;
using UnityEngine;

namespace Client
{
    public class MenuWithButtons : MonoBehaviour
    {
        [SerializeField]
        private TabletClient tabletClient = null!;

        [SerializeField]
        private TouchInput touchInput = null!;

        [SerializeField]
        private GameObject networkingPanel = null!;

        [SerializeField]
        private GameObject connectingPanel = null!;

        [SerializeField]
        private GameObject modePanel = null!;

        [SerializeField]
        private GameObject slicingPanel = null!;

        [SerializeField]
        private GameObject modelSelectedPanel = null!;

        [SerializeField]
        private GameObject snapshotSelectedPanel = null!;

        [SerializeField]
        private TMP_InputField ipInput = null!;

        private void OnEnable()
        {
            tabletClient.Connected += OnConnected;
            tabletClient.ModelSelected += OnModelSelected;
            tabletClient.SnapshotSelected += OnSnapshotSelected;
            tabletClient.SnapshotRemoved += OnSnapshotRemoved;
            // touchInput.Swiped += OnSwipe;
            touchInput.Scaled += OnScale;
        }

        private void OnDisable()
        {
            tabletClient.Connected -= OnConnected;
            tabletClient.ModelSelected -= OnModelSelected;
            tabletClient.SnapshotSelected -= OnSnapshotSelected;
            tabletClient.SnapshotRemoved -= OnSnapshotRemoved;
            // touchInput.Swiped -= OnSwipe;
            touchInput.Scaled -= OnScale;
        }

        public void ConnectClicked()
        {
            DeactivateAll();
            connectingPanel.SetActive(true);

            Debug.Log("Clicked");
            tabletClient.IP = ipInput.text.Trim();
            Debug.Log($"IP: {ipInput.text.Trim()}");
            tabletClient.Connect();
        }

        public void Slice() => tabletClient.Send(Categories.Slice);

        public void ToggleAttached() => tabletClient.Send(Categories.ToggleAttached);

        public void DetachAllSnapshots() => tabletClient.Send(Categories.DetachAllSnapshots);

        public void RemoveSnapshot() => tabletClient.Send(Categories.RemoveSnapshot);

        public void Select() => tabletClient.Send(Categories.Select);

        public void SlicingMode()
        {
            tabletClient.Send(Categories.SlicingMode);
            DeactivateAll();
            slicingPanel.SetActive(true);
        }

        public void SelectionBack()
        {
            tabletClient.Send(Categories.Deselect);
            SelectionMode();
        }

        public void SlicingBack()
        {
            SelectionMode();
        }
        
        public void OnPointerDown()
        {
            tabletClient.Send(Categories.HoldBegin);
        }

        public void OnPointerUp()
        {
            tabletClient.Send(Categories.HoldEnd);
        }

        public void SendToScreen()
        {
            tabletClient.Send(Categories.SendToScreen);
        }
        
        private void SelectionMode()
        {
            tabletClient.Send(Categories.SelectionMode);
            DeactivateAll();
            modePanel.SetActive(true);
        }

        private void OnConnected()
        {
            SelectionMode();
        }

        private void OnModelSelected()
        {
            DeactivateAll();
            modelSelectedPanel.SetActive(true);
        }

        private void OnSnapshotSelected()
        {
            DeactivateAll();
            snapshotSelectedPanel.SetActive(true);
        }

        private void OnSnapshotRemoved()
        {
            if (!snapshotSelectedPanel.activeInHierarchy)
            {
                return;
            }

            SelectionMode();
        }

        private void OnSwipe(bool inward, float endPointX, float endPointY, float angle)
        {
            if (!snapshotSelectedPanel.activeInHierarchy)
            {
                return;
            }

            var direction = DirectionMethods.GetDirectionDegree(angle);
            if (direction == Direction.Up)
            {
                tabletClient.Send(Categories.SendToScreen);
            }
        }

        private void OnScale(float scale)
        {
            if (modelSelectedPanel.activeInHierarchy)
            {
                tabletClient.Send(new ScaleCommand(scale));
            }
        }

        private void DeactivateAll()
        {
            networkingPanel.SetActive(false);
            connectingPanel.SetActive(false);
            modePanel.SetActive(false);
            slicingPanel.SetActive(false);
            modelSelectedPanel.SetActive(false);
            snapshotSelectedPanel.SetActive(false);
        }
    }
}