#nullable enable

using System.Reflection;
using Networking.Tablet;
using UnityEditor;
using UnityEngine;

namespace Selection.Editor
{
    [CustomEditor(typeof(Selectable))]
    public class SelectableEditor : UnityEditor.Editor
    {
        private MethodInfo? selectMethod;

        private void OnEnable()
        {
            selectMethod = typeof(TabletServer).GetMethod("OnSelect", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            if (GUILayout.Button("Select"))
            {
                TabletServer.Instance.Highlighted = (Selectable)serializedObject.targetObject;
                selectMethod?.Invoke(TabletServer.Instance, null);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
