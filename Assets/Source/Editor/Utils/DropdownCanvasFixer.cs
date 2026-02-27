#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.Editor.Utils
{
    public static class DropdownCanvasFixer
    {
        [MenuItem("Tools/Utils/Fix Dropdown Canvas Sorting")]
        public static void FixAllDropdowns()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
            var fixedCount = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab == null) continue;

                var dropdowns = prefab.GetComponentsInChildren<TMP_Dropdown>(true);
                
                foreach (var dropdown in dropdowns)
                {
                    if (dropdown.template == null) continue;
                    
                    var templateGo = dropdown.template.gameObject;
                    var canvas = templateGo.GetComponent<Canvas>();
                    
                    if (canvas == null)
                    {
                        using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
                        {
                            var root = scope.prefabContentsRoot;
                            var editDropdowns = root.GetComponentsInChildren<TMP_Dropdown>(true);
                            
                            foreach (var editDropdown in editDropdowns)
                            {
                                if (editDropdown.template == null) continue;
                                
                                var editTemplateGo = editDropdown.template.gameObject;
                                var editCanvas = editTemplateGo.GetComponent<Canvas>();
                                
                                if (editCanvas == null)
                                {
                                    editCanvas = editTemplateGo.AddComponent<Canvas>();
                                    editCanvas.overrideSorting = true;
                                    editCanvas.sortingOrder = 30001;
                                    
                                    if (editTemplateGo.GetComponent<GraphicRaycaster>() == null)
                                        editTemplateGo.AddComponent<GraphicRaycaster>();
                                    
                                    fixedCount++;
                                    Debug.Log($"Fixed dropdown in: {path}");
                                }
                            }
                        }
                        break;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Dropdown Canvas Fixer: Fixed {fixedCount} dropdown(s)");
        }
    }
}
#endif
