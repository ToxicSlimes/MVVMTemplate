#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Core.Editor.CodeGenPipeline;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.ViewModelScanner
{
    public static class ViewModelScanner
    {
        private const string OutputPath =
            "Assets/Source/Runtime/Core/CompositionRoot/Generated/ViewModelRegistration.generated.cs";

        private const string Namespace = "Core.DependencyInjection.Generated";
        private const string AttributeName = "GenerateViewCommandsAttribute";

        [MenuItem("Tools/MVVM/Scan & Generate ViewModel Registration")]
        public static void ScanAndGenerate()
        {
            if (Generate())
                AssetDatabase.Refresh();
        }

        public static bool Generate()
        {
            var viewModels = DiscoverViewModels();

            if (viewModels.Count == 0 && !ConsumerCodeGenContext.IsPackageInstallation)
            {
                Debug.LogWarning("[ViewModelScanner] No classes with [GenerateViewCommands] found.");
                return false;
            }

            var code = GenerateRegistrationCode(viewModels);

            if (ConsumerCodeGenContext.IsPackageInstallation)
            {
                return ConsumerCodeGenContext.WriteConsumerPart(
                    "ViewModels", code, "RegisterViewModels", "RegisterViewModels", "ContainerBuilder");
            }

            EnsureDirectoryExists(Path.GetDirectoryName(OutputPath));

            if (!WriteIfChanged(OutputPath, code))
            {
                Debug.Log("[ViewModelScanner] No changes detected, skipping.");
                return false;
            }

            Debug.Log($"[ViewModelScanner] Generated {viewModels.Count} ViewModels at {OutputPath}");
            return true;
        }

        private static List<ViewModelInfo> DiscoverViewModels()
        {
            var result = new List<ViewModelInfo>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                if (assembly.IsDynamic) continue;
                if (IsTestAssembly(assembly)) continue;
                if (!ConsumerCodeGenContext.ShouldScanAssembly(assembly)) continue;

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).ToArray();
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (type == null || type.IsAbstract || type.IsGenericType) continue;
                    if (!HasGenerateViewCommandsAttribute(type)) continue;

                    var constructor = GetBestConstructor(type);
                    if (constructor == null)
                    {
                        Debug.LogWarning($"[ViewModelScanner] No public constructor found for {type.FullName}");
                        continue;
                    }

                    var info = new ViewModelInfo
                    {
                        ViewModelType = type,
                        Constructor = constructor,
                        Model = FindModelDependency(constructor, assemblies)
                    };

                    result.Add(info);
                }
            }

            return result.OrderBy(v => v.ViewModelType.Name).ToList();
        }

        private static bool HasGenerateViewCommandsAttribute(Type type)
        {
            return type.GetCustomAttributes(false)
                .Any(attr => attr.GetType().Name == AttributeName);
        }

        private static ConstructorInfo GetBestConstructor(Type type)
        {
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            if (constructors.Length == 0)
                return null;

            if (constructors.Length == 1)
                return constructors[0];

            return constructors.OrderByDescending(c => c.GetParameters().Length).First();
        }

        private static ModelInfo FindModelDependency(ConstructorInfo constructor, Assembly[] assemblies)
        {
            foreach (var param in constructor.GetParameters())
            {
                var paramType = param.ParameterType;
                if (!IsModelInterface(paramType)) continue;

                var concreteType = FindConcreteImplementation(paramType, assemblies);
                if (concreteType == null)
                {
                    Debug.LogWarning(
                        $"[ViewModelScanner] No concrete implementation found for {paramType.Name}, skipping model registration");
                    continue;
                }

                var modelConstructor = GetBestConstructor(concreteType);

                return new ModelInfo
                {
                    InterfaceType = paramType,
                    ConcreteType = concreteType,
                    Constructor = modelConstructor
                };
            }

            return null;
        }

        private static bool IsModelInterface(Type type)
        {
            if (!type.IsInterface) return false;
            var name = type.Name;
            return name.StartsWith("I") && name.EndsWith("Model") && name.Length > 6;
        }

        private static Type FindConcreteImplementation(Type interfaceType, Assembly[] assemblies)
        {
            var candidates = new List<Type>();

            foreach (var assembly in assemblies)
            {
                if (assembly.IsDynamic) continue;
                if (IsTestAssembly(assembly)) continue;
                if (!ConsumerCodeGenContext.ShouldScanAssembly(assembly)) continue;

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).ToArray();
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (type == null || type.IsAbstract || type.IsInterface || type.IsGenericType) continue;
                    if (!interfaceType.IsAssignableFrom(type)) continue;
                    candidates.Add(type);
                }
            }

            if (candidates.Count == 0)
                return null;

            if (candidates.Count > 1)
            {
                Debug.LogWarning(
                    $"[ViewModelScanner] Multiple implementations of {interfaceType.Name}: " +
                    $"{string.Join(", ", candidates.Select(c => c.Name))}. Using {candidates[0].Name}.");
            }

            return candidates[0];
        }

        private static bool IsTestAssembly(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            return name.Contains("Tests") ||
                   name.Contains("Test") ||
                   name.Contains("nunit") ||
                   name.Contains("NSubstitute");
        }

        private static string GenerateRegistrationCode(List<ViewModelInfo> viewModels)
        {
            var namespaces = new HashSet<string>();
            var registeredModelInterfaces = new HashSet<Type>();

            foreach (var vm in viewModels)
            {
                AddNamespace(namespaces, vm.ViewModelType);
                AddConstructorNamespaces(namespaces, vm.Constructor);

                if (vm.Model != null)
                {
                    AddNamespace(namespaces, vm.Model.InterfaceType);
                    AddNamespace(namespaces, vm.Model.ConcreteType);
                    if (vm.Model.Constructor != null)
                    {
                        AddConstructorNamespaces(namespaces, vm.Model.Constructor);
                    }
                }
            }

            namespaces.Add("Reflex.Core");
            namespaces.Remove("System");

            var hasLoggerParam = viewModels.Any(vm =>
                HasParameterOfType(vm.Constructor, "ILogger") ||
                (vm.Model?.Constructor != null && HasParameterOfType(vm.Model.Constructor, "ILogger")));

            var sb = new System.Text.StringBuilder();

            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("// This file was generated by ViewModelScanner.");
            sb.AppendLine(
                "// Do not edit manually. Run 'Tools/MVVM/Scan & Generate ViewModel Registration' to regenerate.");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine();
            sb.AppendLine("#pragma warning disable");
            sb.AppendLine();

            foreach (var ns in namespaces.OrderBy(n => n))
            {
                sb.AppendLine($"using {ns};");
            }

            if (hasLoggerParam)
            {
                sb.AppendLine("using ILogger = Core.Abstractions.Services.ILogger;");
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {Namespace}");
            sb.AppendLine("{");
            sb.AppendLine("    public static class ViewModelRegistration");
            sb.AppendLine("    {");
            sb.AppendLine(
                "        public static ContainerBuilder RegisterViewModels(this ContainerBuilder builder)");
            sb.AppendLine("        {");

            var models = viewModels
                .Where(vm => vm.Model != null)
                .Select(vm => vm.Model)
                .GroupBy(m => m.InterfaceType)
                .Select(g => g.First())
                .OrderBy(m => m.InterfaceType.Name)
                .ToList();

            if (models.Count > 0)
            {
                foreach (var model in models)
                {
                    if (registeredModelInterfaces.Contains(model.InterfaceType)) continue;
                    registeredModelInterfaces.Add(model.InterfaceType);

                    GenerateModelRegistration(sb, model);
                }

                sb.AppendLine();
            }

            foreach (var vm in viewModels)
            {
                GenerateViewModelRegistration(sb, vm);
            }

            sb.AppendLine();
            sb.AppendLine("            return builder;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static void GenerateModelRegistration(System.Text.StringBuilder sb, ModelInfo model)
        {
            var interfaceName = model.InterfaceType.Name;
            var concreteName = model.ConcreteType.Name;

            if (model.Constructor == null || model.Constructor.GetParameters().Length == 0)
            {
                sb.AppendLine($"            builder.AddSingleton<{interfaceName}>(_ => new {concreteName}());");
            }
            else
            {
                var parameters = model.Constructor.GetParameters();
                sb.AppendLine(
                    $"            builder.AddSingleton<{interfaceName}>(container => new {concreteName}(");

                for (var i = 0; i < parameters.Length; i++)
                {
                    var paramTypeName = GetTypeName(parameters[i].ParameterType);
                    var separator = i < parameters.Length - 1 ? "," : "));";
                    sb.AppendLine($"                container.Resolve<{paramTypeName}>(){separator}");
                }
            }
        }

        private static void GenerateViewModelRegistration(System.Text.StringBuilder sb, ViewModelInfo vm)
        {
            var vmName = vm.ViewModelType.Name;
            var parameters = vm.Constructor.GetParameters();

            if (parameters.Length == 0)
            {
                sb.AppendLine($"            builder.AddSingleton(_ => new {vmName}());");
            }
            else
            {
                sb.AppendLine($"            builder.AddSingleton(container => new {vmName}(");

                for (var i = 0; i < parameters.Length; i++)
                {
                    var paramTypeName = GetTypeName(parameters[i].ParameterType);
                    var separator = i < parameters.Length - 1 ? "," : "));";
                    sb.AppendLine($"                container.Resolve<{paramTypeName}>(){separator}");
                }
            }
        }

        private static string GetTypeName(Type type)
        {
            if (type.Name == "ILogger" && type.Namespace != null && type.Namespace.Contains("Abstractions"))
                return "ILogger";

            return type.Name;
        }

        private static string NormalizeNamespace(string ns)
        {
            if (string.IsNullOrEmpty(ns)) return ns;
            const string prefix = "Source.Runtime.";
            return ns.StartsWith(prefix, StringComparison.Ordinal)
                ? ns.Substring(prefix.Length)
                : ns;
        }

        private static void AddNamespace(HashSet<string> namespaces, Type type)
        {
            if (!string.IsNullOrEmpty(type.Namespace))
                namespaces.Add(NormalizeNamespace(type.Namespace));
        }

        private static void AddConstructorNamespaces(HashSet<string> namespaces, ConstructorInfo constructor)
        {
            foreach (var param in constructor.GetParameters())
            {
                AddNamespace(namespaces, param.ParameterType);
            }
        }

        private static bool HasParameterOfType(ConstructorInfo constructor, string typeName)
        {
            return constructor.GetParameters().Any(p => p.ParameterType.Name == typeName);
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

        private class ViewModelInfo
        {
            public Type ViewModelType;
            public ConstructorInfo Constructor;
            public ModelInfo Model;
        }

        private class ModelInfo
        {
            public Type InterfaceType;
            public Type ConcreteType;
            public ConstructorInfo Constructor;
        }
    }
}
#endif
