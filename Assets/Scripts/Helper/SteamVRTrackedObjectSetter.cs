using UnityEngine;
using Valve.VR;

namespace Assets.Scripts.Helper
{
    public class SteamVRTrackedObjectSetter : MonoBehaviour
    {
        public SteamVR_TrackedObject trackedSteamVRObject;

        public void Start()
        {
            SetActiveTrackingDevice();
        }

        public void SetActiveTrackingDevice()
        {
            var indexThreshold = 25;
            var index = 0;

            while (OpenVR.System.GetTrackedDeviceClass((uint)index) != ETrackedDeviceClass.GenericTracker && index < indexThreshold)
            {
                index++;
            }

            if (index >= indexThreshold)
            {
                Debug.Log("No controller found.");
            }

            trackedSteamVRObject.SetDeviceIndex(index);
        }
    }
}
