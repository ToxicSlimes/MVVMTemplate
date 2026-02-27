#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.ViewHandlerScanner
{
    public static class ViewHandlerScanner
    {
        private const string OutputPath =
            "Assets/Source/Runtime/Core/DependencyInjection/Generated/ViewHandlerRegistration.generated.cs";

        private const string Namespace = "Core.DependencyInjection.Generated";

        private static readonly Regex EmbeddedViewRegex =
            new(@"new\s+(?:Show|Hide)EmbeddedViewRequest<(\w+)>", RegexOptions.Compiled);

        private static readonly Regex ModalViewRegex =
            new(@"new\s+(?:Show|Hide)ViewRequest<(\w+),\s*(\w+)>", RegexOptions.Compiled);

        [MenuItem("Tools/MVVM/Scan & Generate View Handler Registration")]
        public static void ScanAndGenerate()
        {
            if (Generate())
                AssetDatabase.Refresh();
        }

        public static bool Generate()
        {
            var sourceDir = Path.Combine(Application.dataPath, "Source/Runtime");
            if (!Directory.Exists(sourceDir))
            {
                Debug.LogWarning("[ViewHandlerScanner] Source/Runtime directory not found.");
                return false;
            }

            var csFiles = Directory.GetFiles(sourceDir, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains("/Generated/") && !f.Contains("\\Generated\\") &&
                            !f.Contains("/Tests/") && !f.Contains("\\Tests\\"))
                .ToList();

            var embeddedViewModels = new HashSet<string>();
            var modalViews = new HashSet<(string View, string ViewModel)>();

            foreach (var file in csFiles)
            {
                try
                {
                    var content = File.ReadAllText(file);

                    foreach (Match match in EmbeddedViewRegex.Matches(content))
                        embeddedViewModels.Add(match.Groups[1].Value);

                    foreach (Match match in ModalViewRegex.Matches(content))
                        modalViews.Add((match.Groups[1].Value, match.Groups[2].Value));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ViewHandlerScanner] Failed to scan {file}: {ex.Message}");
                }
            }

            if (embeddedViewModels.Count == 0 && modalViews.Count == 0)
            {
                Debug.LogWarning("[ViewHandlerScanner] No view handler usages found.");
                return false;
            }

            var resolvedEmbedded = ResolveTypes(embeddedViewModels);
            var resolvedModal = ResolveModalTypes(modalViews);

            var code = GenerateCode(resolvedEmbedded, resolvedModal);

            EnsureDirectoryExists(Path.GetDirectoryName(OutputPath));

            if (!WriteIfChanged(OutputPath, code))
            {
                Debug.Log("[ViewHandlerScanner] No changes detected, skipping.");
                return false;
            }

            Debug.Log($"[ViewHandlerScanner] Generated {resolvedEmbedded.Count} embedded + " +
                      $"{resolvedModal.Count} modal handlers at {OutputPath}");
            return true;
        }

        private static List<ResolvedType> ResolveTypes(HashSet<string> typeNames)
        {
            var result = new List<ResolvedType>();

            foreach (var name in typeNames.OrderBy(n => n))
            {
                var type = FindTypeByName(name);
                if (type == null)
                {
                    Debug.LogWarning($"[ViewHandlerScanner] Type not found: {name}");
                    continue;
                }

                result.Add(new ResolvedType { Name = type.Name, Namespace = type.Namespace });
            }

            return result;
        }

        private static List<ResolvedModalPair> ResolveModalTypes(
            HashSet<(string View, string ViewModel)> pairs)
        {
            var result = new List<ResolvedModalPair>();

            foreach (var (viewName, vmName) in pairs.OrderBy(p => p.View))
            {
                var viewType = FindTypeByName(viewName);
                var vmType = FindTypeByName(vmName);

                if (viewType == null || vmType == null)
                {
                    Debug.LogWarning(
                        $"[ViewHandlerScanner] Modal pair not resolved: {viewName}/{vmName}");
                    continue;
                }

                result.Add(new ResolvedModalPair
                {
                    View = new ResolvedType { Name = viewType.Name, Namespace = viewType.Namespace },
                    ViewModel = new ResolvedType { Name = vmType.Name, Namespace = vmType.Namespace }
                });
            }

            return result;
        }

        private static Type FindTypeByName(string typeName)
        {
            Type fallback = null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic) continue;
                if (IsTestAssembly(assembly)) continue;

                var asmName = assembly.GetName().Name;

                foreach (var type in GetLoadableTypes(assembly))
                {
                    if (type.Name != typeName) continue;

                    if (IsProjectAssembly(asmName))
                        return type;

                    fallback ??= type;
                }
            }

            return fallback;
        }

        private static bool IsProjectAssembly(string assemblyName)
        {
            return assemblyName.StartsWith("Assembly-CSharp", StringComparison.Ordinal) ||
                   assemblyName.StartsWith("Core.", StringComparison.Ordinal);
        }

        private static IEnumerable<Type> GetLoadableTypes(System.Reflection.Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (System.Reflection.ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null);
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }

        private static bool IsTestAssembly(System.Reflection.Assembly assembly)
        {
            var name = assembly.GetName().Name;
            return name.Contains("Tests") ||
                   name.Contains("Test") ||
                   name.Contains("nunit") ||
                   name.Contains("NSubstitute");
        }

        private static string GenerateCode(
            List<ResolvedType> embedded,
            List<ResolvedModalPair> modal)
        {
            var namespaces = new HashSet<string>
            {
                "Core.Infrastructure.Mediator.Registration",
                "Core.Application.Requests.Views",
                "Core.Application.Handlers.Views"
            };

            foreach (var t in embedded)
                if (!string.IsNullOrEmpty(t.Namespace))
                    namespaces.Add(NormalizeNamespace(t.Namespace));

            foreach (var m in modal)
            {
                if (!string.IsNullOrEmpty(m.View.Namespace))
                    namespaces.Add(NormalizeNamespace(m.View.Namespace));
                if (!string.IsNullOrEmpty(m.ViewModel.Namespace))
                    namespaces.Add(NormalizeNamespace(m.ViewModel.Namespace));
            }

            var sb = new StringBuilder();

            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("// This file was generated by ViewHandlerScanner.");
            sb.AppendLine(
                "// Do not edit manually. Run 'Tools/MVVM/Scan & Generate View Handler Registration' to regenerate.");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine();
            sb.AppendLine("#pragma warning disable");
            sb.AppendLine();

            foreach (var ns in namespaces.OrderBy(n => n))
                sb.AppendLine($"using {ns};");

            sb.AppendLine();
            sb.AppendLine($"namespace {Namespace}");
            sb.AppendLine("{");
            sb.AppendLine("    public static class ViewHandlerRegistration");
            sb.AppendLine("    {");
            sb.AppendLine(
                "        public static MediatorBuilder RegisterViewHandlers(this MediatorBuilder builder)");
            sb.AppendLine("        {");

            if (embedded.Count > 0)
            {
                sb.AppendLine("            // Embedded panels");
                foreach (var vm in embedded)
                {
                    sb.AppendLine($"            builder.AddHandler<");
                    sb.AppendLine(
                        $"                ShowEmbeddedViewRequest<{vm.Name}>,");
                    sb.AppendLine($"                bool,");
                    sb.AppendLine(
                        $"                EmbeddedViewRequestHandler<{vm.Name}>>();");
                    sb.AppendLine();
                    sb.AppendLine($"            builder.AddHandler<");
                    sb.AppendLine(
                        $"                HideEmbeddedViewRequest<{vm.Name}>,");
                    sb.AppendLine(
                        $"                HideEmbeddedViewHandler<{vm.Name}>>();");
                    sb.AppendLine();
                }
            }

            if (modal.Count > 0)
            {
                sb.AppendLine("            // Modal views");
                foreach (var m in modal)
                {
                    sb.AppendLine($"            builder.AddHandler<");
                    sb.AppendLine(
                        $"                ShowViewRequest<{m.View.Name}, {m.ViewModel.Name}>,");
                    sb.AppendLine($"                bool,");
                    sb.AppendLine(
                        $"                ShowViewRequestHandler<{m.View.Name}, {m.ViewModel.Name}>>();");
                    sb.AppendLine();
                    sb.AppendLine($"            builder.AddHandler<");
                    sb.AppendLine(
                        $"                HideViewRequest<{m.View.Name}, {m.ViewModel.Name}>,");
                    sb.AppendLine($"                bool,");
                    sb.AppendLine(
                        $"                HideViewRequestHandler<{m.View.Name}, {m.ViewModel.Name}>>();");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("            return builder;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string NormalizeNamespace(string ns)
        {
            if (string.IsNullOrEmpty(ns)) return ns;
            const string prefix = "Source.Runtime.";
            return ns.StartsWith(prefix, StringComparison.Ordinal)
                ? ns.Substring(prefix.Length)
                : ns;
        }

        private static bool WriteIfChanged(string path, string content)
        {
            if (File.Exists(path) && File.ReadAllText(path) == content)
                return false;

            File.WriteAllText(path, content);
            return true;
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private struct ResolvedType
        {
            public string Name;
            public string Namespace;
        }

        private struct ResolvedModalPair
        {
            public ResolvedType View;
            public ResolvedType ViewModel;
        }
    }
}
#endif
