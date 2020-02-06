#if HDRP_AVAILABLE
using UnityEngine;

namespace UnityEditor.Recorder
{
    [CustomEditor(typeof(AOVRecorderSettings))]
    class AOVRecorderEditor : RecorderEditor
    {
        SerializedProperty m_OutputFormat;
        SerializedProperty m_AOVGSelection;
        SerializedProperty m_AOVCompression;

        static class Styles
        {
            internal static readonly GUIContent FormatLabel = new GUIContent("Format");
            internal static readonly GUIContent AOVGLabel = new GUIContent("Aov to Export");
            internal static readonly GUIContent AOVCLabel = new GUIContent("Compression");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (target == null)
                return;
            
            m_OutputFormat = serializedObject.FindProperty("outputFormat");
            m_AOVGSelection = serializedObject.FindProperty("AOVGSelection");
            m_AOVCompression = serializedObject.FindProperty("AOVCompression");
        }

        protected override void FileTypeAndFormatGUI()
        {
            EditorGUILayout.PropertyField(m_OutputFormat, Styles.FormatLabel);            
        }

        protected override void AOVGUI()
        {
            base.AOVGUI();
            EditorGUILayout.PropertyField(m_AOVGSelection, Styles.AOVGLabel);
            #if OIIO_AVAILABLE
            EditorGUILayout.PropertyField(m_AOVCompression, Styles.AOVCLabel);
            #endif
            DrawSeparator();
        }
    }
}
#endif
