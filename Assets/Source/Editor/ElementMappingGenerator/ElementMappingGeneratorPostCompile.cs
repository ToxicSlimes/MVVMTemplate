#if UNITY_EDITOR
using System.IO;
using System.Linq;
using Core.Infrastructure.MVVM.Config;
using UnityEditor;
using UnityEditor.AddressableAssets;
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
            if (mapping.ElementPrefab != null && mapping.ElementPrefab.RuntimeKeyIsValid()) return;

            var prefabName = elementName.Replace("Element", "");
            var guids = AssetDatabase.FindAssets($"t:Prefab {prefabName}", new[] { "Assets/Prefabs" });

            string matchGuid = null;
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                if (prefab == null) continue;

                if (prefab.name.Equals(prefabName, System.StringComparison.OrdinalIgnoreCase) ||
                    prefab.name.Equals(elementName, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (prefab.GetComponent(mapping.ElementType) != null)
                    {
                        matchGuid = g;
                        break;
                    }
                }
            }

            if (matchGuid == null) return;

            var matchPath = AssetDatabase.GUIDToAssetPath(matchGuid);
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                var entry = settings.FindAssetEntry(matchGuid);
                if (entry == null)
                {
                    entry = settings.CreateOrMoveEntry(matchGuid, settings.DefaultGroup);
                    entry.address = matchPath;
                    EditorUtility.SetDirty(settings);
                }
            }

            var so = new SerializedObject(mapping);
            var guidProp = so.FindProperty("ElementPrefab.m_AssetGUID");
            if (guidProp != null)
            {
                guidProp.stringValue = matchGuid;
                so.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(mapping);
            AssetDatabase.SaveAssets();
            Debug.Log($"[ElementMappingGenerator] Auto-assigned prefab: {matchPath}");
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
