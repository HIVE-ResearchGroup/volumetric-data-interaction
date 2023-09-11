using Networking;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Interaction
{
    [CustomEditor(typeof(Selectable))]
    public class SelectableEditor : Editor
    {
        private SerializedProperty _hostProp;
        private MethodInfo _method;

        private void OnEnable()
        {
            _hostProp = serializedObject.FindProperty("host");
            _method = typeof(Host).GetMethod("HandleTap", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            if (GUILayout.Button("Select"))
            {
                var host = (Host)_hostProp.objectReferenceValue;
                host.Highlighted = ((Selectable)serializedObject.targetObject).gameObject;
                _method.Invoke(host, new object[] { TapType.Double, 0.0f, 0.0f });
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
