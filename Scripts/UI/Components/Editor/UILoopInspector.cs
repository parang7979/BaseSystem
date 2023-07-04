using System;
using UnityEditor;
using UnityEngine;

namespace Parang.UI
{
    [CustomEditor(typeof(UILoop))]
    public class UILoopInspector : Editor
    {
        private SerializedProperty initOnStart;
        private SerializedProperty cell;
        private SerializedProperty cellGap;
        private SerializedProperty page;
        private SerializedProperty direction;
        private SerializedProperty alignCenter;
        private SerializedProperty bufferNo;
        private SerializedProperty startOffset;
        private SerializedProperty endOffset;
        private SerializedProperty emtpyGo;

        private void OnEnable()
        {
            try
            {
                initOnStart = serializedObject.FindProperty("m_InitOnStart");
                cell = serializedObject.FindProperty("m_Cell");
                cellGap = serializedObject.FindProperty("m_CellGap");
                page = serializedObject.FindProperty("m_Page");
                direction = serializedObject.FindProperty("m_Direction");
                alignCenter = serializedObject.FindProperty("m_AlignCenter");
                bufferNo = serializedObject.FindProperty("m_BufferNo");
                startOffset = serializedObject.FindProperty("m_StartOffset");
                endOffset = serializedObject.FindProperty("m_EndOffset");
                emtpyGo = serializedObject.FindProperty("m_EmptyGo");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(initOnStart);
            EditorGUILayout.PropertyField(cell);
            EditorGUILayout.PropertyField(cellGap);
            EditorGUILayout.PropertyField(page);
            EditorGUILayout.PropertyField(direction);
            EditorGUILayout.PropertyField(alignCenter);
            EditorGUILayout.PropertyField(bufferNo);
            EditorGUILayout.PropertyField(startOffset);
            EditorGUILayout.PropertyField(endOffset);
            EditorGUILayout.PropertyField(emtpyGo);
            if (GUILayout.Button("UpdateLayout"))
            {
                var uiLoop = target as UILoop;
                uiLoop.RefreshChildItems();
            }
            serializedObject.ApplyModifiedProperties();
        }

    }
}
