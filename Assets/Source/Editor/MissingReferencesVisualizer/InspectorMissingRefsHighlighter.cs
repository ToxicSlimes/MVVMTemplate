#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MissingReferencesVisualizer
{
    [InitializeOnLoad]
    public static class InspectorMissingRefsHighlighter
    {
        private const string ENABLE_KEY = "MissingRefsVisualizer_InspectorEnabled";
        private static bool isEnabled = true;
        private static Dictionary<Object, List<MissingRefInfo>> cachedInspectorRefs = new Dictionary<Object, List<MissingRefInfo>>();
        public static Color missingRefColor = new Color(1f, 0.3f, 0.3f, 0.2f);

        static InspectorMissingRefsHighlighter()
        {
            isEnabled = EditorPrefs.GetBool(ENABLE_KEY, true);

            if (isEnabled)
            {
                EditorApplication.hierarchyChanged += OnHierarchyChanged;
                Selection.selectionChanged += OnSelectionChanged;
            }
        }

        private static void OnHierarchyChanged()
        {
            if (!isEnabled) return;
            cachedInspectorRefs.Clear();
        }

        private static void OnSelectionChanged()
        {
            if (!isEnabled) return;
            cachedInspectorRefs.Clear();
        }

        public static bool IsMissingReference(SerializedProperty property)
        {
            if (property == null) return false;
            if (property.propertyType != SerializedPropertyType.ObjectReference) return false;

            return property.objectReferenceValue == null && property.objectReferenceEntityIdValue.GetHashCode() != 0;
        }


        public static List<MissingRefInfo> GetMissingRefsForObject(Object obj)
        {
            if (obj == null) return new List<MissingRefInfo>();

            if (cachedInspectorRefs.ContainsKey(obj))
            {
                return cachedInspectorRefs[obj];
            }

            List<MissingRefInfo> missingRefs = new List<MissingRefInfo>();

            if (obj is GameObject go)
            {
                Component[] components = go.GetComponents<Component>();
                foreach (Component comp in components)
                {
                    if (comp == null)
                    {
                        missingRefs.Add(new MissingRefInfo(go, null, "Missing Script", "Missing Script", go.name));
                        continue;
                    }

                    SerializedObject so = new SerializedObject(comp);
                    SerializedProperty prop = so.GetIterator();

                    while (prop.NextVisible(true))
                    {
                        if (IsMissingReference(prop))
                        {
                            string componentType = comp.GetType().Name;
                            string fieldName = prop.displayName;
                            missingRefs.Add(new MissingRefInfo(go, comp, fieldName, componentType, go.name));
                        }
                    }
                }
            }
            else
            {
                SerializedObject so = new SerializedObject(obj);
                SerializedProperty prop = so.GetIterator();

                while (prop.NextVisible(true))
                {
                    if (IsMissingReference(prop))
                    {
                        string fieldName = prop.displayName;
                        missingRefs.Add(new MissingRefInfo(null, null, fieldName, obj.GetType().Name, obj.name));
                    }
                }
            }

            cachedInspectorRefs[obj] = missingRefs;
            return missingRefs;
        }

        public static bool IsPropertyMissingRef(SerializedProperty property, Object targetObject)
        {
            if (!IsMissingReference(property)) return false;

            string propertyPath = property.propertyPath;
            string fieldName = property.displayName;

            var missingRefs = GetMissingRefsForObject(targetObject);
            return missingRefs.Any(refInfo => refInfo.fieldName == fieldName || propertyPath.Contains(fieldName));
        }

        public static void DrawPropertyFieldWithHighlight(Rect position, SerializedProperty property, GUIContent label, Object targetObject)
        {
            bool isMissing = IsPropertyMissingRef(property, targetObject);

            if (isMissing)
            {
                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = missingRefColor;
                
                EditorGUI.DrawRect(position, missingRefColor);
                
                EditorGUI.DrawRect(new Rect(position.x, position.y, 2f, position.height), Color.red);
                
                EditorGUI.PropertyField(position, property, label, true);
                
                GUI.backgroundColor = originalColor;

                Rect iconRect = new Rect(position.xMax - 18f, position.y + 1f, 16f, 16f);
                GUIContent warningIcon = EditorGUIUtility.IconContent("console.warnicon");
                GUI.color = Color.red;
                GUI.DrawTexture(iconRect, warningIcon.image);
                GUI.color = Color.white;
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public static void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            EditorPrefs.SetBool(ENABLE_KEY, enabled);

            if (enabled)
            {
                EditorApplication.hierarchyChanged += OnHierarchyChanged;
                Selection.selectionChanged += OnSelectionChanged;
            }
            else
            {
                EditorApplication.hierarchyChanged -= OnHierarchyChanged;
                Selection.selectionChanged -= OnSelectionChanged;
            }

            cachedInspectorRefs.Clear();
        }

        public static bool IsEnabled()
        {
            return isEnabled;
        }
    }
}
#endif

