using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Core.Editor.ViewMappingGenerator
{
    internal static class ViewMappingGeneratorPostCompile
    {
        private const string PendingViewNameKey = "ViewMappingGenerator.PendingViewName";

        public static void Schedule(string viewName)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                return;
            }

            EditorPrefs.SetString(PendingViewNameKey, viewName);
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            var viewName = EditorPrefs.GetString(PendingViewNameKey, string.Empty);
            if (string.IsNullOrEmpty(viewName))
            {
                return;
            }

            EditorPrefs.DeleteKey(PendingViewNameKey);

            var mappingTypeName = ViewMappingGeneratorConfig.GetMappingTypeName(viewName);
            var mappingType = ViewMappingGeneratorWindow.FindTypeInAssemblies(mappingTypeName);
            if (mappingType == null)
            {
                return;
            }

            ViewMappingGeneratorConfig.EnsureDirectoriesExist(viewName);

            var assetPath = ViewMappingGeneratorConfig.GetMappingAssetPath(viewName);
            var existingAsset = AssetDatabase.LoadAssetAtPath(assetPath, mappingType);
            Object mapping;

            if (existingAsset != null)
            {
                mapping = existingAsset;
            }
            else
            {
                var instance = ScriptableObject.CreateInstance(mappingType);
                AssetDatabase.CreateAsset(instance, assetPath);
                AssetDatabase.SaveAssets();
                mapping = instance;
            }

            ViewMappingGeneratorWindow.TryAutoAssignViewPrefab(mapping, viewName);
            ViewMappingGeneratorWindow.TryAddMappingToViewConfig(mapping);
        }
    }
}
