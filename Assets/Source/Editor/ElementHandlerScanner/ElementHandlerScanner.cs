#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Abstractions.MVVM.Elements;
using Core.Data.Domain.Models.Elements;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.ElementHandlerScanner
{
    public static class ElementHandlerScanner
    {
        private const string OutputPath = "Assets/Source/Runtime/Core/CompositionRoot/Generated/ElementHandlerRegistration.generated.cs";
        private const string Namespace = "Core.DependencyInjection.Generated";

        [MenuItem("Tools/Elements/Scan and Generate Handler Registration")]
        public static void ScanAndGenerate()
        {
            if (Generate())
                AssetDatabase.Refresh();
        }

        public static bool Generate()
        {
            var elements = DiscoverElements();

            if (elements.Count == 0)
            {
                Debug.LogWarning("[ElementScanner] No BindableElement<TData> classes found.");
                return false;
            }

            var code = GenerateRegistrationCode(elements);

            EnsureDirectoryExists(Path.GetDirectoryName(OutputPath));

            if (!WriteIfChanged(OutputPath, code))
            {
                Debug.Log("[ElementScanner] No changes detected, skipping.");
                return false;
            }

            Debug.Log($"[ElementScanner] Generated {elements.Count} element types at {OutputPath}");
            return true;
        }

        private static List<ElementInfo> DiscoverElements()
        {
            var result = new List<ElementInfo>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var bindableElementType = typeof(BindableElement<>);

            foreach (var assembly in assemblies)
            {
                if (assembly.IsDynamic) continue;
                if (IsTestAssembly(assembly)) continue;

                try
                {
                    Type[] types;
                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch (System.Reflection.ReflectionTypeLoadException ex)
                    {
                        types = ex.Types.Where(t => t != null).ToArray();
                    }

                    foreach (var type in types)
                    {
                        if (type == null || type.IsAbstract || type.IsGenericType) continue;

                        var dataType = GetBindableElementDataType(type, bindableElementType);
                        if (dataType == null) continue;

                        var isEquatable = IsEquatableType(dataType);

                        result.Add(new ElementInfo
                        {
                            ElementType = type,
                            DataType = dataType,
                            IsEquatable = isEquatable
                        });
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ElementScanner] Error scanning {assembly.GetName().Name}: {ex.Message}");
                }
            }

            return result.OrderBy(e => e.ElementType.FullName).ToList();
        }

        private static Type GetBindableElementDataType(Type type, Type bindableElementType)
        {
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType &&
                    baseType.GetGenericTypeDefinition() == bindableElementType)
                {
                    return baseType.GetGenericArguments()[0];
                }
                baseType = baseType.BaseType;
            }
            return null;
        }

        private static bool IsEquatableType(Type dataType)
        {
            var equatableInterface = typeof(IEquatable<>).MakeGenericType(dataType);
            return equatableInterface.IsAssignableFrom(dataType);
        }

        private static bool IsTestAssembly(System.Reflection.Assembly assembly)
        {
            var name = assembly.GetName().Name;
            return name.Contains("Tests") ||
                   name.Contains("Test") ||
                   name.Contains("nunit") ||
                   name.Contains("NSubstitute");
        }

        private static string GenerateRegistrationCode(List<ElementInfo> elements)
        {
            var namespaces = new HashSet<string>
            {
                "Core.Infrastructure.Mediator.Abstractions",
                "Core.Infrastructure.Mediator.Registration",
                "Core.Data.Domain.Models.Elements",
                "Core.Application.Commands.Elements",
                "Core.Application.Handlers.Elements"
            };

            foreach (var e in elements)
            {
                if (!string.IsNullOrEmpty(e.ElementType.Namespace))
                    namespaces.Add(NormalizeNamespace(e.ElementType.Namespace));
                if (!string.IsNullOrEmpty(e.DataType.Namespace))
                    namespaces.Add(NormalizeNamespace(e.DataType.Namespace));
            }

            var sb = new System.Text.StringBuilder();

            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("// This file was generated by ElementHandlerScanner.");
            sb.AppendLine("// Do not edit manually. Run 'Tools/Elements/Scan and Generate Handler Registration' to regenerate.");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine();
            sb.AppendLine("#pragma warning disable");
            sb.AppendLine();

            foreach (var ns in namespaces.OrderBy(n => n))
            {
                sb.AppendLine($"using {ns};");
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {Namespace}");
            sb.AppendLine("{");
            sb.AppendLine("    public static class ElementHandlerRegistration");
            sb.AppendLine("    {");
            sb.AppendLine("        public static MediatorBuilder RegisterElementHandlers(this MediatorBuilder builder)");
            sb.AppendLine("        {");

            sb.AppendLine("            builder.AddHandler<DestroyElementCommand, Nothing, DestroyElementHandler>();");
            sb.AppendLine("            builder.AddHandler<ClearContainerCommand, Nothing, ClearContainerHandler>();");
            sb.AppendLine();

            foreach (var e in elements)
            {
                sb.AppendLine($"            builder.AddHandler<");
                sb.AppendLine($"                CreateElementCommand<{e.ElementType.Name}, {e.DataType.Name}>,");
                sb.AppendLine($"                {e.ElementType.Name},");
                sb.AppendLine($"                CreateElementHandler<{e.ElementType.Name}, {e.DataType.Name}>>();");
                sb.AppendLine();
            }

            var equatableElements = elements.Where(e => e.IsEquatable).ToList();
            if (equatableElements.Count > 0)
            {
                foreach (var e in equatableElements)
                {
                    sb.AppendLine($"            builder.AddHandler<");
                    sb.AppendLine($"                ReconcileElementsCommand<{e.ElementType.Name}, {e.DataType.Name}>,");
                    sb.AppendLine($"                Nothing,");
                    sb.AppendLine($"                ReconcileElementsHandler<{e.ElementType.Name}, {e.DataType.Name}>>();");
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
            {
                Directory.CreateDirectory(path);
            }
        }

        private class ElementInfo
        {
            public Type ElementType;
            public Type DataType;
            public bool IsEquatable;
        }
    }
}
#endif
