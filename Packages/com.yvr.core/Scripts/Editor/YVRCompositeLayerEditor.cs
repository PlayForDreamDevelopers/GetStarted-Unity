using UnityEditor;

namespace YVR.Core
{
    [CustomEditor(typeof(YVRCompositeLayer))]
    public class YVRCompositeLayerEditor : UnityEditor.Editor
    {
        private SerializedProperty m_Script;
        private SerializedProperty m_Shape;
        private SerializedProperty m_CircleSegments;
        private SerializedProperty m_CylinderAngle;
        //private SerializedProperty m_Equirect2Angle;
        private SerializedProperty m_Radius;
        private SerializedProperty m_LeftEyeTexture;
        private SerializedProperty m_RightEyeTexture;
        private SerializedProperty m_SourceRectLeft;
        private SerializedProperty m_DestRectLeft;
        private SerializedProperty m_SourceRectRight;
        private SerializedProperty m_DestRectRight;

        private void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
            m_Shape = serializedObject.FindProperty("m_Shape");
            m_CircleSegments = serializedObject.FindProperty("m_CircleSegments");
            m_CylinderAngle = serializedObject.FindProperty("m_CylinderAngle");
            m_Radius = serializedObject.FindProperty("m_Radius");
            m_LeftEyeTexture = serializedObject.FindProperty("texture");
            m_RightEyeTexture = serializedObject.FindProperty("rightEyeTexture");
            m_SourceRectLeft = serializedObject.FindProperty("sourceRectLeft");
            m_DestRectLeft = serializedObject.FindProperty("destRectLeft");
            m_SourceRectRight = serializedObject.FindProperty("sourceRectRight");
            m_DestRectRight = serializedObject.FindProperty("destRectRight");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Shape);
            YVRRenderLayerType layerShape = (YVRRenderLayerType)m_Shape.intValue;
            if (layerShape == YVRRenderLayerType.Cylinder)
            {
                EditorGUILayout.PropertyField(m_CylinderAngle);
                EditorGUILayout.PropertyField(m_CircleSegments);
            }
            else if (layerShape == YVRRenderLayerType.Equirect)
            {
                EditorGUILayout.PropertyField(m_Radius);
            }

            EditorGUILayout.PropertyField(m_LeftEyeTexture);
            EditorGUILayout.PropertyField(m_SourceRectLeft);
            if (layerShape == YVRRenderLayerType.Quad || layerShape == YVRRenderLayerType.Equirect)
            {
                EditorGUILayout.PropertyField(m_DestRectLeft);
            }
            EditorGUILayout.PropertyField(m_RightEyeTexture);
            EditorGUILayout.PropertyField(m_SourceRectRight);
            if (layerShape == YVRRenderLayerType.Quad || layerShape == YVRRenderLayerType.Equirect)
            {
                EditorGUILayout.PropertyField(m_DestRectRight);
            }

            DrawPropertiesExcluding(serializedObject, m_Script.propertyPath,
                m_LeftEyeTexture.propertyPath, m_RightEyeTexture.propertyPath, m_SourceRectLeft.propertyPath, m_DestRectLeft.propertyPath, m_SourceRectRight.propertyPath, m_DestRectRight.propertyPath,
                m_Shape.propertyPath, m_CircleSegments.propertyPath, m_CylinderAngle.propertyPath,
                m_Radius.propertyPath);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
