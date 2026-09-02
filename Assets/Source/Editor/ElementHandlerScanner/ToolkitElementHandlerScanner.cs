#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Abstractions.MVVM.Elements;
using Core.Data.Domain.Models.Elements;
using Core.UI.Toolkit.Elements;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.ElementHandlerScanner
{
    public static class ToolkitElementHandlerScanner
    {
        private const string OutputPath = "Assets/Source/Runtime/Core/DependencyInjection/Generated/ToolkitElementHandlerRegistration.generated.cs";
        private const string Namespace = "Core.DependencyInjection.Generated";

        public static bool Generate()
        {
            var elements = DiscoverElements();

            if (elements.Count == 0)
            {
                Debug.LogWarning("[ToolkitElementScanner] No ToolkitBindableElement<TData> classes found.");
                return false;
            }

            var code = GenerateRegistrationCode(elements);
            EnsureDirectoryExists(Path.GetDirectoryName(OutputPath));

            if (!WriteIfChanged(OutputPath, code))
            {
                Debug.Log("[ToolkitElementScanner] No changes detected, skipping.");
                return false;
            }

            Debug.Log($"[ToolkitElementScanner] Generated {elements.Count} toolkit element types at {OutputPath}");
            return true;
        }

        private static List<ElementInfo> DiscoverElements()
        {
            var result = new List<ElementInfo>();
            var bindableElementType = typeof(ToolkitBindableElement<>);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
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

                        if (!IsEquatableType(dataType)) continue;

                        result.Add(new ElementInfo
                        {
                            ElementType = type,
                            DataType = dataType
                        });
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ToolkitElementScanner] Error scanning {assembly.GetName().Name}: {ex.Message}");
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

            foreach (var element in elements)
            {
                if (!string.IsNullOrEmpty(element.ElementType.Namespace))
                    namespaces.Add(element.ElementType.Namespace);
                if (!string.IsNullOrEmpty(element.DataType.Namespace))
                    namespaces.Add(element.DataType.Namespace);
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("// This file was generated by ToolkitElementHandlerScanner.");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine();
            sb.AppendLine("#pragma warning disable");
            sb.AppendLine();

            foreach (var ns in namespaces.OrderBy(n => n))
                sb.AppendLine($"using {ns};");

            sb.AppendLine();
            sb.AppendLine($"namespace {Namespace}");
            sb.AppendLine("{");
            sb.AppendLine("    public static class ToolkitElementHandlerRegistration");
            sb.AppendLine("    {");
            sb.AppendLine("        public static MediatorBuilder RegisterToolkitElementHandlers(this MediatorBuilder builder)");
            sb.AppendLine("        {");

            foreach (var element in elements)
            {
                sb.AppendLine($"            builder.AddHandler<");
                sb.AppendLine($"                ReconcileToolkitElementsCommand<{element.ElementType.Name}, {element.DataType.Name}>,");
                sb.AppendLine($"                Nothing,");
                sb.AppendLine($"                ReconcileToolkitElementsHandler<{element.ElementType.Name}, {element.DataType.Name}>>();");
                sb.AppendLine();
            }

            sb.AppendLine("            return builder;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
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

        private class ElementInfo
        {
            public Type ElementType;
            public Type DataType;
        }
    }
}
#endif
