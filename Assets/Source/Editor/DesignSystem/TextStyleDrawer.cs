#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Core.Configs.DesignSystem;
using MissingReferencesVisualizer;

namespace Editor.DesignSystem
{
    [CustomPropertyDrawer(typeof(TypographySettings.TextStyle))]
    public class TextStyleDrawer : BaseMissingRefsPropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (EditorGUIUtility.singleLineHeight + 2f) * 4f + 8f;
        }

        protected override void DrawPropertyContent(Rect contentRect, SerializedProperty property, GUIContent label, bool hasMissingRef)
        {
            var fontSizeProp = property.FindPropertyRelative("fontSize");
            var fontWeightProp = property.FindPropertyRelative("fontWeight");
            var lineHeightProp = property.FindPropertyRelative("lineHeight");
            var letterSpacingProp = property.FindPropertyRelative("letterSpacing");
            var alignmentProp = property.FindPropertyRelative("alignment");

            var line1Y = contentRect.y + 2f;
            var line2Y = line1Y + EditorGUIUtility.singleLineHeight + 2f;
            var line3Y = line2Y + EditorGUIUtility.singleLineHeight + 2f;
            var line4Y = line3Y + EditorGUIUtility.singleLineHeight + 2f;

            var labelWidth = 90f;
            var fieldWidth = contentRect.width - labelWidth - 4f - 24f;
            var halfFieldWidth = (fieldWidth - 4f) / 2f;

            DrawPropertyField(new Rect(contentRect.x, line1Y, labelWidth, EditorGUIUtility.singleLineHeight),
                             new Rect(contentRect.x + labelWidth + 4f, line1Y, halfFieldWidth, EditorGUIUtility.singleLineHeight),
                             "Size:", fontSizeProp);

            var prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 60f;
            var weightRect = new Rect(contentRect.x + labelWidth + halfFieldWidth + 8f, line1Y, halfFieldWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(weightRect, fontWeightProp, new GUIContent("Weight:"));
            EditorGUIUtility.labelWidth = prevLabelWidth;

            DrawPropertyField(new Rect(contentRect.x, line2Y, labelWidth, EditorGUIUtility.singleLineHeight),
                             new Rect(contentRect.x + labelWidth + 4f, line2Y, halfFieldWidth, EditorGUIUtility.singleLineHeight),
                             "Line H:", lineHeightProp);

            EditorGUIUtility.labelWidth = 40f;
            var letterSpacingRect = new Rect(contentRect.x + labelWidth + halfFieldWidth + 8f, line2Y, halfFieldWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(letterSpacingRect, letterSpacingProp, new GUIContent("Lttr:"));
            EditorGUIUtility.labelWidth = prevLabelWidth;

            DrawPropertyField(new Rect(contentRect.x, line3Y, labelWidth, EditorGUIUtility.singleLineHeight),
                             new Rect(contentRect.x + labelWidth + 4f, line3Y, fieldWidth, EditorGUIUtility.singleLineHeight),
                             "Alignment:", alignmentProp);
        }

        private void DrawPropertyField(Rect labelRect, Rect fieldRect, string labelText, SerializedProperty property)
        {
            var labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold
            };

            EditorGUI.LabelField(labelRect, labelText, labelStyle);
            EditorGUI.PropertyField(fieldRect, property, GUIContent.none);
        }
    }
}
#endif
