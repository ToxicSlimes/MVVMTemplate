#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Core.Editor.PrefabBuilderSystem.Core;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Tools
{
    public static class BuildAllViewsTool
    {
        private const string SettingsPropertyName = "Settings";

        [MenuItem("Tools/UI Prefab Builder/Build All Views")]
        public static void BuildAllViewsMenu()
        {
            RunBuild(out var succeeded, out var failed);

            EditorUtility.DisplayDialog(
                "Build All Views",
                $"Built: {succeeded}\nFailed: {failed}",
                "OK");
        }

        [MenuItem("Tools/UI Prefab Builder/Validate All Views")]
        public static void ValidateAllViewsMenu()
        {
            RunValidation(out var valid, out var invalid, out var failed);

            EditorUtility.DisplayDialog(
                "Validate All Views",
                $"Valid: {valid}\nWith issues: {invalid}\nFailed: {failed}",
                "OK");
        }

        /// <summary>Builds every registered view prefab and returns the number of builders that threw.</summary>
        public static int BuildAll()
        {
            RunBuild(out _, out var failed);
            return failed;
        }

        private static void RunBuild(out int succeeded, out int failed)
        {
            var builderTypes = GetBuilderTypes();
            succeeded = 0;
            failed = 0;

            foreach (var builderType in builderTypes)
            {
                try
                {
                    var builder = CreateBuilder(builderType);
                    builder.Build();
                    succeeded++;
                }
                catch (Exception e)
                {
                    failed++;
                    Debug.LogError($"<color=red>✗ {builderType.Name} failed: {e.Message}</color>\n{e}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=cyan>Build All Views:</color> {succeeded} built, {failed} failed of {builderTypes.Length} builders");
        }

        private static void RunValidation(out int valid, out int invalid, out int failed)
        {
            var builderTypes = GetBuilderTypes();
            valid = 0;
            invalid = 0;
            failed = 0;

            foreach (var builderType in builderTypes)
            {
                try
                {
                    var builder = CreateBuilder(builderType);
                    var report = builder.ValidateOnly();

                    if (report.IsValid)
                    {
                        valid++;
                        Debug.Log($"<color=green>✓ {builderType.Name}:</color> {report.GetSummary()}");
                    }
                    else
                    {
                        invalid++;
                        Debug.LogWarning($"<color=yellow>! {builderType.Name}:</color> {report.GetSummary()}");
                    }
                }
                catch (Exception e)
                {
                    failed++;
                    Debug.LogError($"<color=red>✗ {builderType.Name} failed: {e.Message}</color>\n{e}");
                }
            }

            Debug.Log($"<color=cyan>Validate All Views:</color> {valid} valid, {invalid} with issues, {failed} failed of {builderTypes.Length} builders");
        }

        private static ViewBuilder CreateBuilder(Type builderType)
        {
            var builder = (ViewBuilder)Activator.CreateInstance(builderType);
            ApplySettings(builder);
            return builder;
        }

        private static void ApplySettings(ViewBuilder builder)
        {
            var property = builder.GetType().GetProperty(SettingsPropertyName, BindingFlags.Public | BindingFlags.Instance);

            if (property == null || !property.CanWrite)
                return;

            var settings = FindSettings(property.PropertyType) ?? CreateSettings(property.PropertyType);

            if (settings == null)
            {
                throw new InvalidOperationException(
                    $"{builder.GetType().Name}: settings asset of type {property.PropertyType.Name} not found and could not be created. " +
                    $"Open the builder window once to generate it, or create the asset manually.");
            }

            property.SetValue(builder, settings);
        }

        private static UnityEngine.Object FindSettings(Type settingsType)
        {
            return AssetDatabase.FindAssets($"t:{settingsType.Name}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => Path.GetFileNameWithoutExtension(path) == settingsType.Name ? 0 : 1)
                .ThenBy(path => path, StringComparer.Ordinal)
                .Select(path => AssetDatabase.LoadAssetAtPath(path, settingsType))
                .FirstOrDefault(asset => asset != null);
        }

        /// <summary>Creates the settings asset next to the script that declares its type.</summary>
        private static UnityEngine.Object CreateSettings(Type settingsType)
        {
            var directory = ResolveScriptDirectory(settingsType);

            if (string.IsNullOrEmpty(directory))
                return null;

            var instance = ScriptableObject.CreateInstance(settingsType);

            if (instance == null)
                return null;

            settingsType.GetMethod("ResetToDefaults", BindingFlags.Public | BindingFlags.Instance)
                ?.Invoke(instance, null);

            var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{settingsType.Name}.asset");
            AssetDatabase.CreateAsset(instance, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"<color=cyan>Created settings asset:</color> {assetPath}");

            return instance;
        }

        private static string ResolveScriptDirectory(Type settingsType)
        {
            var scriptPath = AssetDatabase.FindAssets($"{settingsType.Name} t:MonoScript")
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(path => Path.GetFileNameWithoutExtension(path) == settingsType.Name);

            return string.IsNullOrEmpty(scriptPath) ? null : Path.GetDirectoryName(scriptPath).Replace('\\', '/');
        }

        private static Type[] GetBuilderTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .Where(t => !t.IsAbstract
                    && !t.IsGenericTypeDefinition
                    && typeof(ViewBuilder).IsAssignableFrom(t)
                    && t.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(t => t.Name, StringComparer.Ordinal)
                .ToArray();
        }
    }
}
#endif
