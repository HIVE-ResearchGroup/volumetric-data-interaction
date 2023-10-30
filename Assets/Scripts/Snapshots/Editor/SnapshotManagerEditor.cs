using UnityEditor;
using UnityEngine;

namespace Snapshots.Editor
{
    [CustomEditor(typeof(SnapshotManager))]
    public class SnapshotManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawDefaultInspector();

            if (GUILayout.Button("Toggle Alignment"))
            {
                var sm = (SnapshotManager)serializedObject.targetObject;
                sm.ToggleSnapshotsAttached();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}