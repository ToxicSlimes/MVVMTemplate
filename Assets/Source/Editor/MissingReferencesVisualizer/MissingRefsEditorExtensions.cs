#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace MissingReferencesVisualizer
{
    public static class MissingRefsEditorExtensions
    {
        private static readonly Color MissingRefBorderColor = new Color(1f, 0f, 0f, 0.8f);
        private static readonly Color MissingRefBackgroundColor = new Color(1f, 0.3f, 0.3f, 0.15f);

        public static void DrawMissingRefsWarning(this Editor editor, int missingCount)
        {
            if (missingCount <= 0) return;

            EditorGUILayout.Space(6f);

            var prevColor = GUI.backgroundColor;
            var prevContentColor = GUI.contentColor;

            GUI.backgroundColor = new Color(1f, 0.2f, 0.2f, 0.9f);
            GUI.contentColor = Color.white;

            var helpBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(12, 12, 10, 10),
                border = new RectOffset(4, 4, 4, 4),
                alignment = TextAnchor.MiddleLeft
            };

            EditorGUILayout.LabelField(
                $"⚠️  MISSING REFERENCES: {missingCount} field(s) need attention!",
                helpBoxStyle
            );

            GUI.backgroundColor = prevColor;
            GUI.contentColor = prevContentColor;
            EditorGUILayout.Space(6f);
        }

        public static int CountMissingRefsInTargets(Object[] targets)
        {
            if (!InspectorMissingRefsHighlighter.IsEnabled()) return 0;

            int missingCount = 0;
            foreach (var obj in targets)
            {
                var missingRefs = InspectorMissingRefsHighlighter.GetMissingRefsForObject(obj);
                missingCount += missingRefs.Count;
            }
            return missingCount;
        }

        public static void DrawPropertyWithMissingRefHighlight(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!InspectorMissingRefsHighlighter.IsEnabled())
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            bool isMissing = property.propertyType == SerializedPropertyType.ObjectReference &&
                           InspectorMissingRefsHighlighter.IsMissingReference(property);

            if (isMissing)
            {
                EditorGUI.DrawRect(position, MissingRefBackgroundColor);

                var borderRect = new Rect(position.x, position.y, 3f, position.height);
                EditorGUI.DrawRect(borderRect, MissingRefBorderColor);

                EditorGUI.PropertyField(position, property, label, true);

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
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
    }

    public abstract class BaseMissingRefsEditor : Editor
    {
        private int _cachedMissingCount = -1;

        protected virtual bool EnableMissingRefsValidation => true;

        public sealed override void OnInspectorGUI()
        {
            if (EnableMissingRefsValidation && InspectorMissingRefsHighlighter.IsEnabled())
            {
                if (_cachedMissingCount < 0)
                {
                    _cachedMissingCount = MissingRefsEditorExtensions.CountMissingRefsInTargets(targets);
                }

                this.DrawMissingRefsWarning(_cachedMissingCount);
            }

            DrawInspectorContent();

            if (GUI.changed)
            {
                _cachedMissingCount = -1;
            }
        }

        protected abstract void DrawInspectorContent();

        protected void ResetMissingRefsCache()
        {
            _cachedMissingCount = -1;
        }
    }
}
#endif
