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
        private MethodInfo _swipeMethod;
        private MethodInfo _tiltMethod;
        private MethodInfo _shakeMethod;

        private void Awake()
        {
            _modeMethod = typeof(Host).GetMethod("HandleModeChange", BindingFlags.NonPublic | BindingFlags.Instance);
            _tapMethod = typeof(Host).GetMethod("HandleTap", BindingFlags.NonPublic | BindingFlags.Instance);
            _swipeMethod = typeof(Host).GetMethod("HandleSwipe", BindingFlags.NonPublic | BindingFlags.Instance);
            _tiltMethod = typeof(Host).GetMethod("HandleTilt", BindingFlags.NonPublic | BindingFlags.Instance);
            _shakeMethod = typeof(Host).GetMethod("HandleShakes", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawDefaultInspector();

            GUILayout.Label("Modes");
            if (GUILayout.Button("Analysis Mode"))
            {
                var host = (Host)serializedObject.targetObject;
                _modeMethod.Invoke(host, new object[] { MenuMode.Analysis });
            }

            GUILayout.Label("Interaction");
            if (GUILayout.Button("Double Tap"))
            {
                var host = (Host)serializedObject.targetObject;
                _tapMethod.Invoke(host, new object[] { TapType.Double, 250, 250 });
            }

            if (GUILayout.Button("Swipe Left"))
            {
                var host = (Host)serializedObject.targetObject;
                _swipeMethod.Invoke(host, new object[] { false, 0, 150, 180 });
            }

            if (GUILayout.Button("Swipe Right"))
            {
                var host = (Host)serializedObject.targetObject;
                _swipeMethod.Invoke(host, new object[] { false, 500, 150, 0});
            }

            if (GUILayout.Button("Tilt left"))
            {
                var host = (Host)serializedObject.targetObject;
                _tiltMethod.Invoke(host, new object[] { true });
            }

            if (GUILayout.Button("Tilt right"))
            {
                var host = (Host)serializedObject.targetObject;
                _tiltMethod.Invoke(host, new object[] { false });
            }

            if (GUILayout.Button("Shake"))
            {
                var host = (Host)serializedObject.targetObject;
                _shakeMethod.Invoke(host, new object[] { 2 });
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}