#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Core.Abstractions.Validation;

namespace MissingReferencesVisualizer
{
    [CustomPropertyDrawer(typeof(ValidateMissingRefsAttribute))]
    public class ValidateMissingRefsDrawer : PropertyDrawer
    {
        private static readonly Color MissingRefBackgroundColor = new Color(1f, 0.3f, 0.3f, 0.2f);
        private static readonly Color MissingRefBorderColor = new Color(1f, 0f, 0f, 0.8f);
        private static readonly Color ValidBackgroundColor = new Color(0.2f, 0.6f, 1f, 0.08f);
        private static readonly Color ValidBorderColor = new Color(0.2f, 0.6f, 1f, 0.4f);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as ValidateMissingRefsAttribute;

            bool isMissing = property.propertyType == SerializedPropertyType.ObjectReference &&
                           InspectorMissingRefsHighlighter.IsEnabled() &&
                           InspectorMissingRefsHighlighter.IsMissingReference(property);

            if (isMissing || (property.objectReferenceValue != null && property.propertyType == SerializedPropertyType.ObjectReference))
            {
                var bgColor = isMissing ? MissingRefBackgroundColor : ValidBackgroundColor;
                var borderColor = isMissing ? MissingRefBorderColor : ValidBorderColor;

                EditorGUI.DrawRect(position, bgColor);

                var borderRect = new Rect(position.x, position.y, 3f, position.height);
                EditorGUI.DrawRect(borderRect, borderColor);

                if (isMissing && attr.ShowWarningIcon)
                {
                    var iconRect = new Rect(position.xMax - 20f, position.y + 2f, 18f, 18f);
                    var warningIcon = EditorGUIUtility.IconContent("console.warnicon");
                    if (warningIcon != null && warningIcon.image != null)
                    {
                        var prevColor = GUI.color;
                        GUI.color = new Color(1f, 0.2f, 0.2f, 1f);
                        GUI.DrawTexture(iconRect, warningIcon.image);
                        GUI.color = prevColor;
                    }
                }
                else if (!isMissing && property.objectReferenceValue != null)
                {
                    var iconRect = new Rect(position.xMax - 20f, position.y + 2f, 18f, 18f);
                    var checkIcon = EditorGUIUtility.IconContent("TestPassed");
                    if (checkIcon != null && checkIcon.image != null)
                    {
                        var prevColor = GUI.color;
                        GUI.color = new Color(0.3f, 1f, 0.3f, 0.6f);
                        GUI.DrawTexture(iconRect, checkIcon.image);
                        GUI.color = prevColor;
                    }
                }
            }

            EditorGUI.PropertyField(position, property, label, true);

            if (isMissing && !string.IsNullOrEmpty(attr.ErrorMessage))
            {
                var helpRect = new Rect(position.x, position.yMax, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.HelpBox(helpRect, attr.ErrorMessage, MessageType.Error);
            }
        }
    }
}
#endif
