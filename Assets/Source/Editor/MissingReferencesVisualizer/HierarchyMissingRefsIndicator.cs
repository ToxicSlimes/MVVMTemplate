#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MissingReferencesVisualizer
{
    [InitializeOnLoad]
    public static class HierarchyMissingRefsIndicator
    {
        private const string ENABLE_KEY = "MissingRefsVisualizer_Enabled";
        private static bool isEnabled = true;
        private static GUIContent warningIcon;

        static HierarchyMissingRefsIndicator()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
            warningIcon = EditorGUIUtility.IconContent("console.warnicon");
            
            isEnabled = EditorPrefs.GetBool(ENABLE_KEY, true);
        }

        private static void OnHierarchyItemGUI(int instanceID, Rect selectionRect)
        {
            if (!isEnabled) return;

            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null) return;

            if (MissingReferencesScanner.HasMissingRefs(go))
            {
                Rect iconRect = new Rect(selectionRect.x + selectionRect.width - 20f, selectionRect.y, 16f, 16f);
                
                GUI.color = Color.red;
                GUI.DrawTexture(iconRect, warningIcon.image);
                GUI.color = Color.white;

                if (iconRect.Contains(Event.current.mousePosition))
                {
                    var allMissingRefs = MissingReferencesScanner.ScanAllScenes();
                    var missingRefsOnThis = allMissingRefs.Where(r => r.gameObject != null && r.gameObject == go).ToList();
                    var missingRefsOnChildren = allMissingRefs.Where(r => r.gameObject != null && r.gameObject != go && MissingReferencesScanner.IsChildOf(r.gameObject, go)).ToList();
                    
                    int totalCount = missingRefsOnThis.Count + missingRefsOnChildren.Count;
                    if (totalCount > 0)
                    {
                        string tooltip = $"Missing References: {totalCount}\n";
                        
                        if (missingRefsOnThis.Count > 0)
                        {
                            tooltip += "\nOn this object:\n";
                            foreach (var refInfo in missingRefsOnThis.Take(5))
                            {
                                tooltip += $"- {refInfo.componentTypeName}.{refInfo.fieldName}\n";
                            }
                            if (missingRefsOnThis.Count > 5)
                                tooltip += $"... and {missingRefsOnThis.Count - 5} more\n";
                        }
                        
                        if (missingRefsOnChildren.Count > 0)
                        {
                            tooltip += $"\nOn children: {missingRefsOnChildren.Count}\n";
                        }
                        
                        GUI.tooltip = tooltip;
                    }
                }
            }
        }

        public static void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            EditorPrefs.SetBool(ENABLE_KEY, enabled);
            EditorApplication.RepaintHierarchyWindow();
        }

        public static bool IsEnabled()
        {
            return isEnabled;
        }
    }
}
#endif

