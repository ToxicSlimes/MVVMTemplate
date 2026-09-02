#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using Core.Configs.Services;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Core.Editor.CodeGenPipeline
{
    public static class CodeGenPipeline
    {
        private static readonly string[] ScannerTypeNames =
        {
            "Core.Editor.ElementHandlerScanner.ElementHandlerScanner",
            "Core.Editor.ElementHandlerScanner.ToolkitElementHandlerScanner",
            "Core.Editor.Mediator.MediatorHandlerScanner",
            "Core.Editor.ViewModelScanner.ViewModelScanner",
            "Core.Editor.ViewHandlerScanner.ViewHandlerScanner",
            "Core.Editor.StorageMapperScanner.StorageMapperScanner"
        };

        [MenuItem("Tools/MVVM/Scan & Generate All")]
        public static void ScanAndGenerateAll()
        {
            var anyChanged = RunAllScanners();
            if (anyChanged)
            {
                AssetDatabase.Refresh();
                Debug.Log("[CodeGenPipeline] All registrations regenerated.");
            }
            else
            {
                Debug.Log("[CodeGenPipeline] All generated files are up to date.");
            }
        }

        [MenuItem("Tools/MVVM/Validate Template Setup")]
        public static void ValidateTemplateSetup()
        {
            var issues = 0;

            if (!System.IO.File.Exists("Assets/Source/Runtime/Core/DependencyInjection/Generated/ViewModelRegistration.generated.cs"))
            {
                Debug.LogError("[Template] Missing ViewModelRegistration.generated.cs — run Tools → MVVM → Scan & Generate All.");
                issues++;
            }

            if (!System.IO.File.Exists("Assets/Source/Runtime/Core/DependencyInjection/Generated/MediatorHandlerRegistration.generated.cs"))
            {
                Debug.LogError("[Template] Missing MediatorHandlerRegistration.generated.cs — run Tools → Mediator → Scan & Generate Handler Registration.");
                issues++;
            }

            var bootstrapScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Source/Assets/Scenes/BootstrapScene.unity");
            if (bootstrapScene == null)
            {
                Debug.LogError("[Template] BootstrapScene not found at Assets/Source/Assets/Scenes/BootstrapScene.unity");
                issues++;
            }
            else
            {
                var scenes = EditorBuildSettings.scenes;
                var inBuild = scenes.Any(s => s.path.Contains("BootstrapScene") && s.enabled);
                if (!inBuild)
                {
                    Debug.LogWarning("[Template] BootstrapScene is not enabled in Build Settings.");
                    issues++;
                }
            }

            var viewConfig = AssetDatabase.FindAssets("t:ViewConfig");
            if (viewConfig.Length == 0)
            {
                Debug.LogWarning("[Template] ViewConfig asset not found under Assets/Source/00. Configs/");
                issues++;
            }

            issues += CountToolkitBindingIssues();
            issues += CountAudioConfigIssues();

            if (issues == 0)
                Debug.Log("[Template] All checks passed. Open BootstrapScene and press Play.");
            else
                Debug.LogWarning($"[Template] Found {issues} setup issue(s). See messages above.");
        }

        private static int CountToolkitBindingIssues()
        {
            var validatorType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .FirstOrDefault(t => t.FullName == "Core.Editor.UIToolkit.ToolkitBindingValidator");

            if (validatorType == null)
                return 0;

            var method = validatorType.GetMethod("CountBlockingIssues", BindingFlags.Public | BindingFlags.Static);
            if (method == null)
                return 0;

            var result = method.Invoke(null, null);
            if (result is not int count)
                return 0;

            if (count > 0)
                Debug.LogError($"[Template] Toolkit binding validation found {count} error(s). Run Tools → UI Toolkit → Validate Toolkit Bindings.");

            return count;
        }

        private static int CountAudioConfigIssues()
        {
            var issues = 0;
            var guids = AssetDatabase.FindAssets("t:AudioConfig");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<AudioConfig>(path);
                if (config == null)
                    continue;

                if (config.AudioSourcePrefab == null)
                {
                    Debug.LogError($"[Template] AudioConfig missing AudioSourcePrefab at {path}");
                    issues++;
                }

                if (config.AudioMixer == null)
                {
                    Debug.LogError($"[Template] AudioConfig missing AudioMixer at {path}");
                    issues++;
                }
            }

            return issues;
        }

        private static bool RunAllScanners()
        {
            var anyChanged = false;

            foreach (var typeName in ScannerTypeNames)
                anyChanged |= TryInvokeGenerate(typeName);

            return anyChanged;
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            var anyChanged = RunAllScanners();

            if (anyChanged)
            {
                Debug.Log("[CodeGenPipeline] Files changed, refreshing AssetDatabase...");
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.Log("[CodeGenPipeline] All generated files up to date.");
            }
        }

        private static bool TryInvokeGenerate(string fullTypeName)
        {
            var scannerType = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .FirstOrDefault(t => t.FullName == fullTypeName);

            if (scannerType == null)
            {
                Debug.LogWarning($"[CodeGenPipeline] Scanner not found: {fullTypeName}");
                return false;
            }

            var method = scannerType.GetMethod("Generate",
                BindingFlags.Public | BindingFlags.Static);

            if (method == null)
            {
                Debug.LogWarning($"[CodeGenPipeline] Generate() not found on {fullTypeName}");
                return false;
            }

            try
            {
                var result = method.Invoke(null, null);
                return result is true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CodeGenPipeline] Error invoking {fullTypeName}: {ex.Message}");
                return false;
            }
        }
    }
}
#endif
