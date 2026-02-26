#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Core.Editor.CodeGenPipeline
{
    [InitializeOnLoad]
    internal static class CodeGenPipeline
    {
        private static readonly string[] ScannerTypeNames =
        {
            "Core.Editor.ElementHandlerScanner.ElementHandlerScanner",
            "Core.Editor.Mediator.MediatorHandlerScanner",
            "Core.Editor.ViewModelScanner.ViewModelScanner",
            "Core.Editor.ViewHandlerScanner.ViewHandlerScanner",
            "Core.Editor.StorageMapperScanner.StorageMapperScanner"
        };

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            var anyChanged = false;

            foreach (var typeName in ScannerTypeNames)
                anyChanged |= TryInvokeGenerate(typeName);

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
