using System.Reflection;
using Networking;
using UnityEditor;
using UnityEngine;

namespace Model
{
    [CustomEditor(typeof(Selectable))]
    public class SelectableEditor : Editor
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
                Host.Instance.Highlighted = ((Selectable)serializedObject.targetObject).gameObject;
                _method.Invoke(Host.Instance, new object[] { TapType.Double, 0.0f, 0.0f });
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
