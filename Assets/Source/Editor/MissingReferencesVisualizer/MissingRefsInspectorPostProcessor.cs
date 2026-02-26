#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MissingReferencesVisualizer
{
    [InitializeOnLoad]
    public static class MissingRefsInspectorPostProcessor
    {
        static MissingRefsInspectorPostProcessor()
        {
            EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
        }

        private static void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
        {
            if (!InspectorMissingRefsHighlighter.IsEnabled()) return;
            if (property.propertyType != SerializedPropertyType.ObjectReference) return;
            if (!InspectorMissingRefsHighlighter.IsMissingReference(property)) return;

            menu.AddItem(new GUIContent("Missing Reference Detected"), false, () => { });
        }
    }

    public static class MissingRefsPropertyField
    {
        private static readonly Color MissingRefBackgroundColor = new Color(1f, 0.3f, 0.3f, 0.2f);
        private static readonly Color MissingRefBorderColor = new Color(1f, 0f, 0f, 0.8f);

        public static void PropertyField(SerializedProperty property, GUIContent label, bool includeChildren = true)
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

        public static void PropertyField(Rect position, SerializedProperty property, GUIContent label, bool includeChildren = true)
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

        public static void PropertyFieldWithMissingRefCheck(SerializedProperty property, GUIContent label, bool includeChildren = true)
        {
            if (!InspectorMissingRefsHighlighter.IsEnabled())
            {
                EditorGUILayout.PropertyField(property, label, includeChildren);
                return;
            }

            bool isMissing = property.propertyType == SerializedPropertyType.ObjectReference && 
                            InspectorMissingRefsHighlighter.IsMissingReference(property);

            if (isMissing)
            {
                Rect rect = EditorGUILayout.GetControlRect(true, EditorGUI.GetPropertyHeight(property, label, includeChildren));
                DrawMissingRefField(rect, property, label, includeChildren);
            }
            else
            {
                EditorGUILayout.PropertyField(property, label, includeChildren);
            }
        }

        public static void PropertyFieldWithMissingRefCheck(Rect position, SerializedProperty property, GUIContent label, bool includeChildren = true)
        {
            if (!InspectorMissingRefsHighlighter.IsEnabled())
            {
                EditorGUI.PropertyField(position, property, label, includeChildren);
                return;
            }

            bool isMissing = property.propertyType == SerializedPropertyType.ObjectReference && 
                            InspectorMissingRefsHighlighter.IsMissingReference(property);

            if (isMissing)
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

    public static class MissingRefsEditorGUI
    {
        public static bool PropertyField(SerializedProperty property, GUIContent label, bool includeChildren = true)
        {
            return PropertyField(EditorGUILayout.GetControlRect(true, EditorGUI.GetPropertyHeight(property, label, includeChildren)), property, label, includeChildren);
        }

        public static bool PropertyField(Rect position, SerializedProperty property, GUIContent label, bool includeChildren = true)
        {
            if (!InspectorMissingRefsHighlighter.IsEnabled())
            {
                return EditorGUI.PropertyField(position, property, label, includeChildren);
            }

            bool isMissing = property.propertyType == SerializedPropertyType.ObjectReference && 
                            InspectorMissingRefsHighlighter.IsMissingReference(property);

            if (isMissing)
            {
                Color originalBg = GUI.backgroundColor;
                Color originalColor = GUI.color;

                EditorGUI.DrawRect(position, new Color(1f, 0.3f, 0.3f, 0.2f));
                EditorGUI.DrawRect(new Rect(position.x, position.y, 3f, position.height), new Color(1f, 0f, 0f, 0.8f));

                GUI.backgroundColor = new Color(1f, 0.3f, 0.3f, 0.3f);
                bool result = EditorGUI.PropertyField(position, property, label, includeChildren);
                GUI.backgroundColor = originalBg;

                Rect iconRect = new Rect(position.xMax - 18f, position.y + 1f, 16f, 16f);
                GUIContent warningIcon = EditorGUIUtility.IconContent("console.warnicon");
                if (warningIcon != null && warningIcon.image != null)
                {
                    GUI.color = Color.red;
                    GUI.DrawTexture(iconRect, warningIcon.image);
                    GUI.color = originalColor;
                }

                return result;
            }
            else
            {
                return EditorGUI.PropertyField(position, property, label, includeChildren);
            }
        }
    }

    [CustomPropertyDrawer(typeof(Sprite), true)]
    [CustomPropertyDrawer(typeof(Texture), true)]
    [CustomPropertyDrawer(typeof(Texture2D), true)]
    [CustomPropertyDrawer(typeof(Material), true)]
    [CustomPropertyDrawer(typeof(Shader), true)]
    [CustomPropertyDrawer(typeof(GameObject), true)]
    [CustomPropertyDrawer(typeof(Transform), true)]
    [CustomPropertyDrawer(typeof(AnimationClip), true)]
    [CustomPropertyDrawer(typeof(AudioClip), true)]
    [CustomPropertyDrawer(typeof(Font), true)]
    [CustomPropertyDrawer(typeof(Mesh), true)]
    [CustomPropertyDrawer(typeof(ScriptableObject), true)]
    public class MissingRefsTypePropertyDrawer : PropertyDrawer
    {
        private static readonly Color MissingRefBackgroundColor = new Color(1f, 0.3f, 0.3f, 0.2f);
        private static readonly Color MissingRefBorderColor = new Color(1f, 0f, 0f, 0.8f);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!InspectorMissingRefsHighlighter.IsEnabled())
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            if (property.propertyType == SerializedPropertyType.ObjectReference && InspectorMissingRefsHighlighter.IsMissingReference(property))
            {
                EditorGUI.DrawRect(position, MissingRefBackgroundColor);
                
                Rect borderRect = new Rect(position.x, position.y, 3f, position.height);
                EditorGUI.DrawRect(borderRect, MissingRefBorderColor);
                
                Color originalBg = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1f, 0.3f, 0.3f, 0.3f);
                
                EditorGUI.PropertyField(position, property, label, true);
                
                GUI.backgroundColor = originalBg;
                
                Rect iconRect = new Rect(position.xMax - 18f, position.y + 1f, 16f, 16f);
                GUIContent warningIcon = EditorGUIUtility.IconContent("console.warnicon");
                if (warningIcon != null && warningIcon.image != null)
                {
                    Color originalColor = GUI.color;
                    GUI.color = Color.red;
                    GUI.DrawTexture(iconRect, warningIcon.image);
                    GUI.color = originalColor;
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }

}
#endif

