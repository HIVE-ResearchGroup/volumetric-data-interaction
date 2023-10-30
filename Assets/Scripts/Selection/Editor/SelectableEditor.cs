using System.Reflection;
using Networking;
using UnityEditor;
using UnityEngine;

namespace Selection.Editor
{
    [CustomEditor(typeof(Selectable))]
    public class SelectableEditor : UnityEditor.Editor
    {
        private MethodInfo _method;

        private void OnEnable()
        {
            _method = typeof(Host).GetMethod("HandleTap", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            if (GUILayout.Button("Select"))
            {
                Host.Instance.Highlighted = (Selectable)serializedObject.targetObject;
                _method.Invoke(Host.Instance, new object[] { TapType.Double, 0.0f, 0.0f });
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
