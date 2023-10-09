using UnityEditor;
using UnityEngine;

namespace Snapshots
{
    [CustomEditor(typeof(SnapshotManager))]
    public class SnapshotManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawDefaultInspector();

            if (GUILayout.Button("Toggle Alignment"))
            {
                var sm = (SnapshotManager)serializedObject.targetObject;
                sm.ToggleSnapshotAlignment();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}