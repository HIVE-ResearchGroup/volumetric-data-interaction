using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Networking.Tablet.Editor
{
    [CustomEditor(typeof(TabletServer))]
    public class TabletServerEditor : UnityEditor.Editor
    {
        private MethodInfo onSelection;
        private MethodInfo onSlicing;
        private MethodInfo onSelect;
        private MethodInfo onDeselect;
        private MethodInfo onSlice;
        private MethodInfo onRemoveSnapshot;
        private MethodInfo onToggleAttached;
        private MethodInfo onHoldBegin;
        private MethodInfo onHoldEnd;
        private MethodInfo onSendToScreen;

        private void Awake()
        {
            onSelection = typeof(TabletServer).GetMethod("OnSelectionMode", BindingFlags.NonPublic | BindingFlags.Instance);
            onSlicing = typeof(TabletServer).GetMethod("OnSlicingMode", BindingFlags.NonPublic | BindingFlags.Instance);
            onSelect = typeof(TabletServer).GetMethod("OnSelect", BindingFlags.NonPublic | BindingFlags.Instance);
            onDeselect = typeof(TabletServer).GetMethod("OnDeselect", BindingFlags.NonPublic | BindingFlags.Instance);
            onSlice = typeof(TabletServer).GetMethod("OnSlice", BindingFlags.NonPublic | BindingFlags.Instance);
            onRemoveSnapshot = typeof(TabletServer).GetMethod("OnRemoveSnapshot", BindingFlags.NonPublic | BindingFlags.Instance);
            onToggleAttached = typeof(TabletServer).GetMethod("OnToggleAttached", BindingFlags.NonPublic | BindingFlags.Instance);
            onHoldBegin = typeof(TabletServer).GetMethod("OnHoldBegin", BindingFlags.NonPublic | BindingFlags.Instance);
            onHoldEnd = typeof(TabletServer).GetMethod("OnHoldEnd", BindingFlags.NonPublic | BindingFlags.Instance);
            onSendToScreen = typeof(TabletServer).GetMethod("OnSendToScreen", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override async void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawDefaultInspector();
            
            var host = (TabletServer)serializedObject.targetObject;

            if (GUILayout.Button("OnSelectionMode"))
            {
                onSelection.Invoke(host, null);
            }
            if (GUILayout.Button("OnSlicingMode"))
            {
                onSlicing.Invoke(host, null);
            }
            if (GUILayout.Button("OnSelect"))
            {
                onSelect.Invoke(host, null);
            }
            if (GUILayout.Button("OnDeselect"))
            {
                onDeselect.Invoke(host, null);
            }
            if (GUILayout.Button("OnSlice"))
            {
                onSlice.Invoke(host, null);
            }
            if (GUILayout.Button("OnRemoveSnapshot"))
            {
                onRemoveSnapshot.Invoke(host, null);
            }
            if (GUILayout.Button("OnToggleAttached"))
            {
                onToggleAttached.Invoke(host, null);
            }
            if (GUILayout.Button("OnHoldBegin"))
            {
                onHoldBegin.Invoke(host, null);
            }
            if (GUILayout.Button("OnHoldEnd"))
            {
                onHoldEnd.Invoke(host, null);
            }
            if (GUILayout.Button("OnSendToScreen"))
            {
                await (Task)onSendToScreen.Invoke(host, null);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}