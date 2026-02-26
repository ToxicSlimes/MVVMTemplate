#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Core.Abstractions.Attributes;

namespace MissingReferencesVisualizer
{
    public abstract class BaseMissingRefsPropertyDrawer : PropertyDrawer
    {
        private ConfigEntryStyleAttribute _cachedStyleAttribute;
        private bool _styleAttributeCached;

        protected ConfigEntryStyleAttribute StyleAttribute
        {
            get
            {
                if (!_styleAttributeCached)
                {
                    _cachedStyleAttribute = fieldInfo?.FieldType
                        .GetCustomAttributes(typeof(ConfigEntryStyleAttribute), true)
                        .FirstOrDefault() as ConfigEntryStyleAttribute;
                    _styleAttributeCached = true;
                }
                return _cachedStyleAttribute;
            }
        }

        protected virtual float ExtraHeight => 6f;
        protected virtual float BorderWidth => StyleAttribute?.BorderWidth ?? 4f;
        protected virtual float ContentPadding => 8f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + ExtraHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position.height = EditorGUIUtility.singleLineHeight + 2f;
            position.y += 2f;

            DrawPropertyWithBackground(position, property, label);

            EditorGUI.EndProperty();
        }

        protected virtual void DrawPropertyWithBackground(Rect position, SerializedProperty property, GUIContent label)
        {
            var missingRefProp = GetMissingRefProperty(property);
            var isMissing = IsMissingReference(missingRefProp);

            var (bgColor, borderColor) = GetColors(isMissing);

            var bgRect = new Rect(position.x + 2f, position.y, position.width - 4f, position.height);
            EditorGUI.DrawRect(bgRect, bgColor);

            var borderRect = new Rect(bgRect.x, bgRect.y, BorderWidth, bgRect.height);
            EditorGUI.DrawRect(borderRect, borderColor);

            var contentRect = new Rect(bgRect.x + ContentPadding, bgRect.y + 2f, bgRect.width - ContentPadding - 2f, bgRect.height - 4f);

            DrawPropertyContent(contentRect, property, label, isMissing);

            if (StyleAttribute?.ShowStatusIcon ?? true)
            {
                DrawStatusIcon(bgRect, isMissing, missingRefProp);
            }
        }

        protected virtual (Color background, Color border) GetColors(bool isMissing)
        {
            if (StyleAttribute != null)
            {
                return ColorPresets.GetColors(StyleAttribute.ColorPreset, isMissing);
            }

            return isMissing
                ? (new Color(1f, 0.3f, 0.3f, 0.2f), new Color(1f, 0f, 0f, 0.8f))
                : (new Color(0.2f, 0.6f, 1f, 0.12f), new Color(0.2f, 0.6f, 1f, 0.5f));
        }

        protected abstract void DrawPropertyContent(Rect contentRect, SerializedProperty property, GUIContent label, bool hasMissingRef);

        protected virtual SerializedProperty GetMissingRefProperty(SerializedProperty property)
        {
            if (StyleAttribute != null && !string.IsNullOrEmpty(StyleAttribute.MissingRefFieldName))
            {
                return property.FindPropertyRelative(StyleAttribute.MissingRefFieldName);
            }
            return null;
        }

        protected virtual bool IsMissingReference(SerializedProperty property)
        {
            if (property == null) return false;
            if (property.propertyType != SerializedPropertyType.ObjectReference) return false;
            if (!InspectorMissingRefsHighlighter.IsEnabled()) return false;

            return InspectorMissingRefsHighlighter.IsMissingReference(property);
        }

        protected virtual void DrawStatusIcon(Rect bgRect, bool isMissing, SerializedProperty refProperty)
        {
            if (isMissing)
            {
                DrawWarningIcon(bgRect);
            }
            else if (refProperty != null && refProperty.objectReferenceValue != null)
            {
                DrawCheckIcon(bgRect);
            }
        }

        protected void DrawWarningIcon(Rect bgRect)
        {
            var iconRect = new Rect(bgRect.xMax - 22f, bgRect.y + 2f, 18f, 18f);
            var warningIcon = EditorGUIUtility.IconContent("console.warnicon");
            if (warningIcon != null && warningIcon.image != null)
            {
                var prevColor = GUI.color;
                GUI.color = new Color(1f, 0.2f, 0.2f, 1f);
                GUI.DrawTexture(iconRect, warningIcon.image);
                GUI.color = prevColor;
            }
        }

        protected void DrawCheckIcon(Rect bgRect)
        {
            var iconRect = new Rect(bgRect.xMax - 22f, bgRect.y + 2f, 18f, 18f);
            var checkIcon = EditorGUIUtility.IconContent("TestPassed");
            if (checkIcon != null && checkIcon.image != null)
            {
                var prevColor = GUI.color;
                GUI.color = new Color(0.3f, 1f, 0.3f, 0.7f);
                GUI.DrawTexture(iconRect, checkIcon.image);
                GUI.color = prevColor;
            }
        }
    }
}
#endif
