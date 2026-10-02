#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Presentation.MVVM.Config;
using Core.Editor.UIToolkit.ScreenSpec;
using Core.UI.Toolkit;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Editor.UIToolkit
{
    public static class ToolkitBindingValidator
    {
        private static readonly string[] SupplementalBindingSources =
        {
            "Assets/Source/Runtime/Core/Presentation/UI/Toolkit/ToolkitConfirmOverlay.cs"
        };

        private static readonly Regex NamedButtonRegex = new(
            @"<ui:Button\b[^>]*\bname=""([^""]+)""",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex NamedElementRegex = new(
            @"\bname=""([^""]+)""",
            RegexOptions.Compiled);

        private static readonly Regex QueryRegex = new(
            @"\.Q<[^>]+>\(\s*""([^""]+)""\s*\)",
            RegexOptions.Compiled);

        private static readonly Regex ProjectDatabaseStyleRegex = new(
            @"project://database/",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        [MenuItem("Tools/UI Toolkit/Validate Toolkit Bindings")]
        public static void ValidateFromMenu()
        {
            var report = ValidateAll();
            report.LogToConsole();
        }

        public static ToolkitBindingReport ValidateAll()
        {
            var report = new ToolkitBindingReport();
            foreach (var viewType in FindToolkitViewTypes())
                ValidateView(viewType, report);

            return report;
        }

        public static int CountBlockingIssues() => ValidateAll().ErrorCount;

        private static IReadOnlyList<Type> FindToolkitViewTypes()
        {
            return TypeCache.GetTypesDerivedFrom(typeof(ToolkitBindableView<>))
                .Where(t => !t.IsAbstract)
                .OrderBy(t => t.Name)
                .ToArray();
        }

        private static void ValidateView(Type viewType, ToolkitBindingReport report)
        {
            var uxmlPath = ResolveUxmlPath(viewType);
            if (string.IsNullOrEmpty(uxmlPath))
            {
                report.Add(new ToolkitBindingIssue(
                    ToolkitBindingSeverity.Error,
                    viewType.Name,
                    "Could not resolve UXML layout from mapping prefab."));
                return;
            }

            if (!File.Exists(uxmlPath))
            {
                report.Add(new ToolkitBindingIssue(
                    ToolkitBindingSeverity.Error,
                    viewType.Name,
                    $"UXML not found at {uxmlPath}."));
                return;
            }

            var uxmlText = File.ReadAllText(uxmlPath);
            var uxmlButtons = ExtractMatches(NamedButtonRegex, uxmlText).ToHashSet();
            var uxmlNamedElements = ExtractMatches(NamedElementRegex, uxmlText).ToHashSet();
            var viewBoundNames = CollectViewBoundNames(viewType);
            var effectiveBoundNames = BuildEffectiveBoundNames(viewBoundNames, uxmlNamedElements);

            foreach (var buttonName in uxmlButtons.OrderBy(n => n))
            {
                if (!effectiveBoundNames.Contains(buttonName))
                {
                    report.Add(new ToolkitBindingIssue(
                        ToolkitBindingSeverity.Error,
                        viewType.Name,
                        $"UXML button '{buttonName}' is not queried in BindPanel ({Path.GetFileName(uxmlPath)})."));
                }
            }

            foreach (var queryName in viewBoundNames.OrderBy(n => n))
            {
                if (!uxmlNamedElements.Contains(queryName))
                {
                    report.Add(new ToolkitBindingIssue(
                        ToolkitBindingSeverity.Warning,
                        viewType.Name,
                        $"Queries '{queryName}' but no matching named element exists in UXML."));
                }
            }

            if (ProjectDatabaseStyleRegex.IsMatch(uxmlText))
            {
                report.Add(new ToolkitBindingIssue(
                    ToolkitBindingSeverity.Error,
                    viewType.Name,
                    $"UXML uses project://database style paths ({Path.GetFileName(uxmlPath)})."));
            }

            ValidateScreenSpecDrift(viewType, uxmlPath, report);

            ValidatePrefab(viewType, report);
        }

        private static void ValidateScreenSpecDrift(Type viewType, string uxmlPath, ToolkitBindingReport report)
        {
            var spec = TryLoadSpecForView(viewType.Name);
            if (spec == null)
                return;

            foreach (var issue in ScreenSpecValidator.ValidateDocument(spec, uxmlPath))
            {
                var severity = issue.StartsWith("ERROR:", StringComparison.Ordinal)
                    ? ToolkitBindingSeverity.Error
                    : ToolkitBindingSeverity.Warning;

                report.Add(new ToolkitBindingIssue(
                    severity,
                    viewType.Name,
                    issue));
            }
        }

        private static ScreenSpecDocument TryLoadSpecForView(string viewTypeName)
        {
            const string specsDir = "Assets/Source/UI/Toolkit/_specs";
            if (!Directory.Exists(specsDir))
                return null;

            foreach (var file in Directory.GetFiles(specsDir, "*.screen.json"))
            {
                var spec = ScreenSpecReader.Load(file.Replace('\\', '/'));
                if (spec != null && spec.viewType == viewTypeName)
                    return spec;
            }

            return null;
        }

        private static void ValidatePrefab(Type viewType, ToolkitBindingReport report)
        {
            var prefab = ResolvePrefab(viewType);
            if (prefab == null)
            {
                report.Add(new ToolkitBindingIssue(
                    ToolkitBindingSeverity.Error,
                    viewType.Name,
                    "Toolkit prefab not found via ViewMapping."));
                return;
            }

            if (prefab.GetComponent<UIDocument>() == null)
            {
                report.Add(new ToolkitBindingIssue(
                    ToolkitBindingSeverity.Error,
                    viewType.Name,
                    $"Prefab '{prefab.name}' is missing UIDocument."));
            }

            if (prefab.GetComponent(viewType) == null)
            {
                report.Add(new ToolkitBindingIssue(
                    ToolkitBindingSeverity.Error,
                    viewType.Name,
                    $"Prefab '{prefab.name}' is missing {viewType.Name} component."));
            }
        }

        private static HashSet<string> BuildEffectiveBoundNames(
            HashSet<string> viewBoundNames,
            HashSet<string> uxmlNamedElements)
        {
            var names = new HashSet<string>(viewBoundNames, StringComparer.Ordinal);
            foreach (var supplementalName in CollectSupplementalBoundNames())
            {
                if (uxmlNamedElements.Contains(supplementalName))
                    names.Add(supplementalName);
            }

            return names;
        }

        private static HashSet<string> CollectViewBoundNames(Type viewType)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            var viewScriptPath = GetScriptPath(viewType);
            if (!string.IsNullOrEmpty(viewScriptPath))
            {
                AddQueryNamesFromFile(viewScriptPath, names);
                var generatedPath = Path.Combine(
                    Path.GetDirectoryName(viewScriptPath) ?? string.Empty,
                    viewType.Name + ".Bindings.generated.cs");
                if (File.Exists(generatedPath))
                    AddQueryNamesFromFile(generatedPath, names);
            }

            return names;
        }

        private static HashSet<string> CollectSupplementalBoundNames()
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var supplementalPath in SupplementalBindingSources)
            {
                if (File.Exists(supplementalPath))
                    AddQueryNamesFromFile(supplementalPath, names);
            }

            return names;
        }

        private static void AddQueryNamesFromFile(string path, HashSet<string> names)
        {
            var text = File.ReadAllText(path);
            foreach (var match in QueryRegex.Matches(text).Cast<Match>())
                names.Add(match.Groups[1].Value);
        }

        private static IEnumerable<string> ExtractMatches(Regex regex, string text)
        {
            foreach (Match match in regex.Matches(text))
                yield return match.Groups[1].Value;
        }

        private static string ResolveUxmlPath(Type viewType)
        {
            var prefab = ResolvePrefab(viewType);
            if (prefab == null)
                return null;

            var document = prefab.GetComponent<UIDocument>();
            if (document?.visualTreeAsset != null)
                return AssetDatabase.GetAssetPath(document.visualTreeAsset);

            var view = prefab.GetComponent(viewType);
            if (view == null)
                return null;

            var serializedView = new SerializedObject(view);
            var layout = serializedView.FindProperty("_layout")?.objectReferenceValue as VisualTreeAsset;
            return layout != null ? AssetDatabase.GetAssetPath(layout) : null;
        }

        private static GameObject ResolvePrefab(Type viewType)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:ViewMappingBase"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mapping = AssetDatabase.LoadAssetAtPath<ViewMappingBase>(path);
                if (mapping == null || mapping.ViewType != viewType)
                    continue;

                if (mapping.ViewPrefab == null || string.IsNullOrEmpty(mapping.ViewPrefab.AssetGUID))
                    return null;

                var prefabPath = AssetDatabase.GUIDToAssetPath(mapping.ViewPrefab.AssetGUID);
                return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }

            return null;
        }

        private static string GetScriptPath(Type type)
        {
            foreach (var guid in AssetDatabase.FindAssets($"{type.Name} t:MonoScript"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() == type)
                    return path;
            }

            return null;
        }
    }
}
#endif
