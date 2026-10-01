#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.UIToolkit.ScreenSpec
{
    public static class ScreenSpecImporter
    {
        public const string SpecsFolder = "Assets/Source/UI/Toolkit/_specs";
        public const string ToolkitRoot = "Assets/Source/UI/Toolkit";

        [MenuItem("Tools/UI Toolkit/Import Screen Spec...")]
        public static void ImportSelectedSpec()
        {
            var path = EditorUtility.OpenFilePanel("Import Screen Spec", SpecsFolder, "json");
            if (string.IsNullOrEmpty(path))
                return;

            var normalized = path.Replace('\\', '/');
            var assetsIndex = normalized.IndexOf("/Assets/");
            if (assetsIndex < 0)
            {
                Debug.LogError("[ScreenSpec] File must be under project Assets folder.");
                return;
            }

            var assetPath = normalized.Substring(assetsIndex + 1);
            ImportSpecAtPath(assetPath);
        }

        [MenuItem("Tools/UI Toolkit/Import All Screen Specs")]
        public static void ImportAllSpecs()
        {
            if (!Directory.Exists(SpecsFolder))
            {
                Debug.LogError("[ScreenSpec] Missing folder: " + SpecsFolder);
                return;
            }

            var files = Directory.GetFiles(SpecsFolder, "*.screen.json");
            if (files.Length == 0)
            {
                Debug.LogWarning("[ScreenSpec] No *.screen.json files in " + SpecsFolder);
                return;
            }

            foreach (var file in files)
                ImportSpecAtPath(file.Replace('\\', '/'));

            AssetDatabase.Refresh();
            Debug.Log("[ScreenSpec] Imported " + files.Length + " spec(s).");
        }

        public static int CountBlockingIssues()
        {
            if (!Directory.Exists(SpecsFolder))
                return 0;

            var issues = 0;
            foreach (var file in Directory.GetFiles(SpecsFolder, "*.screen.json"))
            {
                var assetPath = file.Replace('\\', '/');
                var spec = ScreenSpecReader.Load(assetPath);
                if (spec == null)
                {
                    Debug.LogError("[ScreenSpec] Failed to load: " + Path.GetFileName(assetPath));
                    issues++;
                    continue;
                }

                var uxmlPath = GetUxmlOutputPath(spec);
                if (string.IsNullOrEmpty(uxmlPath))
                {
                    Debug.LogError("[ScreenSpec] " + Path.GetFileName(assetPath) + ": ERROR: invalid output paths.");
                    issues++;
                    continue;
                }

                var validation = ScreenSpecValidator.ValidateDocument(spec, uxmlPath);
                foreach (var issue in validation)
                {
                    if (issue.StartsWith("ERROR:"))
                    {
                        Debug.LogError("[ScreenSpec] " + Path.GetFileName(assetPath) + ": " + issue);
                        issues++;
                    }
                    else
                        Debug.LogWarning("[ScreenSpec] " + Path.GetFileName(assetPath) + ": " + issue);
                }
            }

            return issues;
        }

        public static void ImportSpecAtPath(string assetPath)
        {
            var spec = ScreenSpecReader.Load(assetPath);
            if (spec == null)
            {
                Debug.LogError("[ScreenSpec] Failed to load: " + assetPath);
                return;
            }

            var fileName = Path.GetFileName(assetPath);
            var uxmlPath = GetUxmlOutputPath(spec);
            var bindingsPath = GetBindingsOutputPath(spec);
            if (string.IsNullOrEmpty(uxmlPath) || string.IsNullOrEmpty(bindingsPath))
            {
                Debug.LogError("[ScreenSpec] " + fileName + ": invalid output paths (folder/uxmlFile/viewType).");
                return;
            }

            var uxml = ScreenSpecUxmlGenerator.Generate(spec, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(uxmlPath) ?? ToolkitRoot);
            Directory.CreateDirectory(Path.GetDirectoryName(bindingsPath) ?? ToolkitRoot);
            File.WriteAllText(uxmlPath, uxml);
            File.WriteAllText(bindingsPath, ScreenSpecViewGenerator.GenerateBindingsPartial(spec));

            AssetDatabase.ImportAsset(uxmlPath);
            AssetDatabase.ImportAsset(bindingsPath);

            var validation = ScreenSpecValidator.ValidateDocument(spec, uxmlPath);
            foreach (var issue in validation)
            {
                if (issue.StartsWith("ERROR:"))
                    Debug.LogError("[ScreenSpec] " + fileName + ": " + issue);
                else
                    Debug.LogWarning("[ScreenSpec] " + fileName + ": " + issue);
            }

            Debug.Log("[ScreenSpec] Imported " + fileName + " -> " + uxmlPath);
        }

        private static string GetUxmlOutputPath(ScreenSpecDocument spec)
        {
            if (spec?.output == null || string.IsNullOrEmpty(spec.output.folder) || string.IsNullOrEmpty(spec.output.uxmlFile))
                return null;

            return Path.Combine(ToolkitRoot, spec.output.folder, spec.output.uxmlFile).Replace('\\', '/');
        }

        private static string GetBindingsOutputPath(ScreenSpecDocument spec)
        {
            if (spec == null || string.IsNullOrEmpty(spec.viewType) || spec.output == null || string.IsNullOrEmpty(spec.output.folder))
                return null;

            var viewsRoot = "Assets/Source/Runtime/Core/Presentation/MVVM/Views";
            return Path.Combine(viewsRoot, spec.output.folder, spec.viewType + ".Bindings.generated.cs").Replace('\\', '/');
        }
    }
}
#endif
