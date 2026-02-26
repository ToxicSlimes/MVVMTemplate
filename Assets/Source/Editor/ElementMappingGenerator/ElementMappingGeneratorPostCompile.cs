#if UNITY_EDITOR
using System.IO;
using System.Linq;
using Core.Infrastructure.MVVM.Config;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Core.Editor.ElementMappingGenerator
{
    internal static class ElementMappingGeneratorPostCompile
    {
        private const string PendingElementNameKey = "ElementMappingGenerator.PendingElementName";
        private const string ElementConfigPath = "Assets/Source/00. Configs/ElementConfig.asset";

        public static void Schedule(string elementName)
        {
            if (string.IsNullOrEmpty(elementName))
            {
                return;
            }

            EditorPrefs.SetString(PendingElementNameKey, elementName);
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            var elementName = EditorPrefs.GetString(PendingElementNameKey, string.Empty);
            if (string.IsNullOrEmpty(elementName))
            {
                return;
            }

            EditorPrefs.DeleteKey(PendingElementNameKey);

            var mappingTypeName = ElementMappingCodeGenerator.GetMappingTypeName(elementName);
            var mappingType = FindTypeInAssemblies(mappingTypeName);
            if (mappingType == null)
            {
                Debug.LogWarning($"[ElementMappingGenerator] Could not find type: {mappingTypeName}");
                return;
            }

            EnsureDirectoryExists(ElementMappingCodeGenerator.GetAssetFolder());

            var assetPath = ElementMappingCodeGenerator.GetMappingAssetPath(elementName);
            var existingAsset = AssetDatabase.LoadAssetAtPath(assetPath, mappingType);
            ElementMappingBase mapping;

            if (existingAsset != null)
            {
                mapping = existingAsset as ElementMappingBase;
            }
            else
            {
                var instance = ScriptableObject.CreateInstance(mappingType) as ElementMappingBase;
                if (instance == null)
                {
                    Debug.LogError($"[ElementMappingGenerator] Failed to create instance of {mappingType.Name}");
                    return;
                }

                AssetDatabase.CreateAsset(instance, assetPath);
                AssetDatabase.SaveAssets();
                mapping = instance;
                Debug.Log($"[ElementMappingGenerator] Created asset: {assetPath}");
            }

            TryAutoAssignElementPrefab(mapping, elementName);
            TryAddMappingToConfig(mapping);
        }

        private static void TryAutoAssignElementPrefab(ElementMappingBase mapping, string elementName)
        {
            if (mapping == null) return;

            // Ищем префаб в Assets/Prefabs/Elements
            var searchFilter = $"{elementName} t:prefab";
            var guids = AssetDatabase.FindAssets(searchFilter, new[] { "Assets/Prefabs/Elements" });
            
            if (guids == null || guids.Length == 0)
            {
                // Попробуем поискать по всему проекту
                guids = AssetDatabase.FindAssets(searchFilter);
            }

            if (guids == null || guids.Length != 1)
            {
                if (guids != null && guids.Length > 1)
                {
                    Debug.LogWarning($"[ElementMappingGenerator] Found multiple prefabs for {elementName}, skipping auto-assign");
                }
                return;
            }

            var prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab != null)
            {
                var serializedObject = new SerializedObject(mapping);
                var prefabProperty = serializedObject.FindProperty("_elementPrefab");
                if (prefabProperty != null)
                {
                    prefabProperty.objectReferenceValue = prefab;
                    serializedObject.ApplyModifiedProperties();
                    AssetDatabase.SaveAssets();
                    Debug.Log($"[ElementMappingGenerator] Auto-assigned prefab: {prefabPath}");
                }
            }
        }

        private static void TryAddMappingToConfig(ElementMappingBase mapping)
        {
            if (mapping == null) return;

            var config = AssetDatabase.LoadAssetAtPath<ElementConfig>(ElementConfigPath);
            if (config == null)
            {
                Debug.LogWarning($"[ElementMappingGenerator] ElementConfig not found at {ElementConfigPath}");
                return;
            }

            var mappings = config.GetAllMappings();
            if (mappings.Contains(mapping))
            {
                return;
            }

            config.AddMapping(mapping);
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            Debug.Log($"[ElementMappingGenerator] Added {mapping.name} to ElementConfig");
        }

        private static System.Type FindTypeInAssemblies(string typeName)
        {
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (var asm in assemblies)
            {
                var type = asm.GetType(typeName);
                if (type != null) return type;
            }

            foreach (var asm in assemblies)
            {
                try
                {
                    foreach (var t in asm.GetTypes())
                    {
                        if (t.FullName == typeName || t.Name == typeName)
                            return t;
                    }
                }
                catch (System.Reflection.ReflectionTypeLoadException) { }
            }

            return null;
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
#endif
