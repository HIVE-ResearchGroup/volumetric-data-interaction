using UnityEditor;
using UnityEngine;

namespace Networking.Tablet.Editor
{
    [CustomEditor(typeof(TabletClient))]
    public class TabletClientEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var client = (TabletClient)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Connect"))
            {
                client.Connect();
            }
        }
    }
}