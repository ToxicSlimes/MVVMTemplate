#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MissingReferencesVisualizer
{
    public static class MissingRefsInspectorHelper
    {
        private static readonly Color MissingRefBackgroundColor = new Color(1f, 0.3f, 0.3f, 0.2f);
        private static readonly Color MissingRefBorderColor = new Color(1f, 0f, 0f, 0.8f);

        public static void DrawPropertyFieldWithMissingRefCheck(SerializedProperty property, GUIContent label, bool includeChildren = true)
        {
            if (!InspectorMissingRefsHighlighter.IsEnabled())
            {
                EditorGUILayout.PropertyField(property, label, includeChildren);
                return;
            }

            if (property.propertyType == SerializedPropertyType.ObjectReference && InspectorMissingRefsHighlighter.IsMissingReference(property))
            {
                Rect rect = EditorGUILayout.GetControlRect(true, EditorGUI.GetPropertyHeight(property, label, includeChildren));
                DrawMissingRefField(rect, property, label, includeChildren);
            }
            else
            {
                EditorGUILayout.PropertyField(property, label, includeChildren);
            }
        }

        public static void DrawPropertyFieldWithMissingRefCheck(Rect position, SerializedProperty property, GUIContent label, bool includeChildren = true)
        {
            if (!InspectorMissingRefsHighlighter.IsEnabled())
            {
                EditorGUI.PropertyField(position, property, label, includeChildren);
                return;
            }

            if (property.propertyType == SerializedPropertyType.ObjectReference && InspectorMissingRefsHighlighter.IsMissingReference(property))
            {
                DrawMissingRefField(position, property, label, includeChildren);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, includeChildren);
            }
        }

        private static void DrawMissingRefField(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            EditorGUI.DrawRect(position, MissingRefBackgroundColor);
            
            Rect borderRect = new Rect(position.x, position.y, 3f, position.height);
            EditorGUI.DrawRect(borderRect, MissingRefBorderColor);
            
            EditorGUI.PropertyField(position, property, label, includeChildren);
            
            Rect iconRect = new Rect(position.xMax - 18f, position.y + 1f, 16f, 16f);
            GUIContent warningIcon = EditorGUIUtility.IconContent("console.warnicon");
            if (warningIcon != null && warningIcon.image != null)
            {
                GUI.color = Color.red;
                GUI.DrawTexture(iconRect, warningIcon.image);
                GUI.color = Color.white;
            }
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
    public class MissingRefsGlobalEditor : Editor
    {
        private static readonly Color MissingRefBackgroundColor = new Color(1f, 0.3f, 0.3f, 0.2f);
        private static readonly Color MissingRefBorderColor = new Color(1f, 0f, 0f, 0.8f);

        public override void OnInspectorGUI()
        {
            if (!InspectorMissingRefsHighlighter.IsEnabled())
            {
                DrawDefaultInspector();
                return;
            }

            foreach (Object obj in targets)
            {
                InspectorMissingRefsHighlighter.GetMissingRefsForObject(obj);
            }

            serializedObject.Update();

            SerializedProperty prop = serializedObject.GetIterator();
            bool enterChildren = true;

            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (prop.propertyPath == "m_Script")
                {
                    EditorGUILayout.PropertyField(prop, true);
                    continue;
                }

                if (prop.propertyType == SerializedPropertyType.ObjectReference && InspectorMissingRefsHighlighter.IsMissingReference(prop))
                {
                    DrawMissingRefField(prop);
                }
                else
                {
                    EditorGUILayout.PropertyField(prop, true);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMissingRefField(SerializedProperty property)
        {
            Rect rect = EditorGUILayout.GetControlRect(true, EditorGUI.GetPropertyHeight(property, true));
            
            EditorGUI.DrawRect(rect, MissingRefBackgroundColor);
            
            Rect borderRect = new Rect(rect.x, rect.y, 3f, rect.height);
            EditorGUI.DrawRect(borderRect, MissingRefBorderColor);
            
            EditorGUI.PropertyField(rect, property, true);
            
            Rect iconRect = new Rect(rect.xMax - 18f, rect.y + 1f, 16f, 16f);
            GUIContent warningIcon = EditorGUIUtility.IconContent("console.warnicon");
            if (warningIcon != null && warningIcon.image != null)
            {
                GUI.color = Color.red;
                GUI.DrawTexture(iconRect, warningIcon.image);
                GUI.color = Color.white;
            }
        }
    }

}

#endif

