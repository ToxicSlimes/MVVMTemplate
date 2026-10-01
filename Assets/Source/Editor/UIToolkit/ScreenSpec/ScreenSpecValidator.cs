using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Core.Editor.UIToolkit.ScreenSpec
{
    public static class ScreenSpecValidator
    {
        public static List<string> ValidateDocument(ScreenSpecDocument spec, string uxmlPath)
        {
            var issues = new List<string>();

            if (spec == null)
            {
                issues.Add("ERROR: Spec document is null.");
                return issues;
            }

            if (spec.schemaVersion != 1)
                issues.Add("ERROR: Unsupported schemaVersion: " + spec.schemaVersion);

            if (string.IsNullOrEmpty(spec.screenId))
                issues.Add("ERROR: screenId is required.");

            if (spec.output == null || string.IsNullOrEmpty(spec.output.folder) || string.IsNullOrEmpty(spec.output.uxmlFile))
                issues.Add("ERROR: output.folder and output.uxmlFile are required.");

            var specNames = new HashSet<string>();
            CollectSpecNames(spec.nodes, specNames);

            foreach (var tab in spec.nodes.SelectMany(FindTabs))
            {
                if (!string.IsNullOrEmpty(tab.tabButtonName))
                    specNames.Add(tab.tabButtonName);
                if (!string.IsNullOrEmpty(tab.id))
                    specNames.Add("content-" + tab.id);
            }

            foreach (var binding in spec.bindings)
            {
                if (string.IsNullOrEmpty(binding.name))
                    continue;

                if (binding.kind != "localized" || !binding.queryByClass)
                    specNames.Add(binding.name);
            }

            if (!File.Exists(uxmlPath))
            {
                issues.Add("ERROR: UXML not found: " + uxmlPath);
                return issues;
            }

            var uxml = File.ReadAllText(uxmlPath);
            var uxmlNames = ExtractUxmlNames(uxml);

            foreach (var name in specNames)
            {
                if (!uxmlNames.Contains(name))
                    issues.Add("ERROR: Spec name '" + name + "' missing from UXML.");
            }

            foreach (var name in uxmlNames)
            {
                if (!specNames.Contains(name))
                    issues.Add("WARN: UXML name '" + name + "' not declared in spec.");
            }

            return issues;
        }

        private static IEnumerable<ScreenSpecTab> FindTabs(ScreenSpecNode node)
        {
            if (node.type == "Tabs")
            {
                foreach (var tab in node.tabs)
                    yield return tab;
            }

            foreach (var child in node.children)
            {
                foreach (var tab in FindTabs(child))
                    yield return tab;
            }
        }

        private static void CollectSpecNames(List<ScreenSpecNode> nodes, HashSet<string> names)
        {
            foreach (var node in nodes)
            {
                if (!string.IsNullOrEmpty(node.name))
                    names.Add(node.name);

                if (!string.IsNullOrEmpty(node.valueName))
                    names.Add(node.valueName);

                if (node.type == "Tabs")
                {
                    foreach (var tab in node.tabs)
                    {
                        if (!string.IsNullOrEmpty(tab.tabButtonName))
                            names.Add(tab.tabButtonName);
                        if (!string.IsNullOrEmpty(tab.id))
                            names.Add("content-" + tab.id);
                        CollectSpecNames(tab.children, names);
                    }
                }

                CollectSpecNames(node.children, names);
            }
        }

        private static HashSet<string> ExtractUxmlNames(string uxml)
        {
            var names = new HashSet<string>();
            var regex = new Regex("\\bname=\"([^\"]+)\"", RegexOptions.Compiled);
            foreach (Match match in regex.Matches(uxml))
                names.Add(match.Groups[1].Value);

            return names;
        }
    }
}
