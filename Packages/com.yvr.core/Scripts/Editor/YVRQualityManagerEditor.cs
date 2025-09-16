using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace YVR.Core.XR
{
    [CustomPropertyDrawer(typeof(YVRQualityManager))]
    public class YVRQualityManagerDrawer : PropertyDrawer
    {
        private SerializedProperty _enableDynamicResolution;

        bool foldout = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_enableDynamicResolution == null)
            {
                _enableDynamicResolution = property.FindPropertyRelative("_enableDynamicResolution");
            }

            //base.OnGUI(position, property, label);
            EditorGUI.indentLevel = 0;
            EditorGUI.BeginProperty(position, label, property);
            foldout = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), foldout, label, true);

            if (foldout)
            {
                EditorGUI.indentLevel = 1;
                DrawSingleLine(ref position, property.FindPropertyRelative("_vSyncCount"));
                DrawSingleLine(ref position, property.FindPropertyRelative("_fixedFoveatedRenderingLevel"));
                DrawSingleLine(ref position, property.FindPropertyRelative("_fixedFoveatedRenderingDynamic"));
                DrawSingleLine(ref position, property.FindPropertyRelative("_sharpenType"));
                DrawSingleLine(ref position, property.FindPropertyRelative("useRecommendedMSAALevel"));
                DrawSingleLine(ref position, _enableDynamicResolution);
                if (_enableDynamicResolution.boolValue)
                {
                    DrawSingleLine(ref position, property.FindPropertyRelative("_powerSetting"));
                    DrawSingleLine(ref position, property.FindPropertyRelative("_minDynamicResolutionScale"));
                    DrawSingleLine(ref position, property.FindPropertyRelative("_maxDynamicResolutionScale"));
                }
                EditorGUI.indentLevel = 0;
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (_enableDynamicResolution == null)
            {
                _enableDynamicResolution = property.FindPropertyRelative("_enableDynamicResolution");
            }

            if (foldout)
            {
                return EditorGUIUtility.singleLineHeight * (10 - 3 * (_enableDynamicResolution.boolValue ? 0 : 1));
            }
            else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        private void DrawSingleLine(ref Rect position, SerializedProperty property)
        {
            position.y += EditorGUIUtility.singleLineHeight;
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property);
        }
    }
}
