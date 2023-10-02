using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Networking
{
    [CustomEditor(typeof(Host))]
    public class HostEditor : Editor
    {
        private MethodInfo _modeMethod;
        private MethodInfo _tapMethod;

        private void Awake()
        {
            _modeMethod = typeof(Host).GetMethod("HandleModeChange", BindingFlags.NonPublic | BindingFlags.Instance);
            _tapMethod = typeof(Host).GetMethod("HandleTap", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawDefaultInspector();

            if (GUILayout.Button("Analysis Mode"))
            {
                var host = (Host)serializedObject.targetObject;
                _modeMethod.Invoke(host, new object[] { MenuMode.Analysis });
            }

            if (GUILayout.Button("Double Tap"))
            {
                var host = (Host)serializedObject.targetObject;
                _tapMethod.Invoke(host, new object[] { TapType.Double, 250, 250 });
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}