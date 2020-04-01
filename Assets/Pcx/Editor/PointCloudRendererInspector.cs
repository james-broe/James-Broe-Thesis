// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx

using UnityEngine;
using UnityEditor;

namespace Pcx
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PointCloudRenderer))]
    public class PointCloudRendererInspector : Editor
    {
        SerializedProperty _sourceData;
        SerializedProperty _pointTint;
        SerializedProperty _pointSize;
        SerializedProperty _corner1;
        SerializedProperty _corner2;

        void OnEnable()
        {
            _sourceData = serializedObject.FindProperty("_sourceData");
            _pointTint = serializedObject.FindProperty("_pointTint");
            _pointSize = serializedObject.FindProperty("_pointSize");
            _corner1 = serializedObject.FindProperty("_corner1");
            _corner2 = serializedObject.FindProperty("_corner2");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_sourceData);
            EditorGUILayout.PropertyField(_pointTint);
            EditorGUILayout.PropertyField(_pointSize);
            EditorGUILayout.PropertyField(_corner1);
            EditorGUILayout.PropertyField(_corner2);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
