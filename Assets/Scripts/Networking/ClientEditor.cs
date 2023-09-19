using UnityEditor;
using UnityEngine;

namespace Networking
{
    [CustomEditor(typeof(Client))]
    public class ClientEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var client = (Client)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Send Menu: Analysis"))
            {
                client.SendMenuChangedMessage(MenuMode.Analysis);
            }

            if (GUILayout.Button("Send Menu: Selection"))
            {
                client.SendMenuChangedMessage(MenuMode.Selection);
            }

            if (GUILayout.Button("Send Menu: Mapping"))
            {
                client.SendMenuChangedMessage(MenuMode.Mapping);
            }

            if (GUILayout.Button("Send Swipe"))
            {
                client.SendSwipeMessage(true, 250, 250, 0);
            }

            if (GUILayout.Button("Send Shake"))
            {
                client.SendShakeMessage(3);
            }

            if (GUILayout.Button("Send Double Tap"))
            {
                client.SendTapMessage(TapType.Double, 250, 250);
            }
        }
    }
}