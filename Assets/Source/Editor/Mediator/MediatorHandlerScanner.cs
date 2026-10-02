namespace Core.Editor.Mediator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using Core.Mediator.Abstractions;
    using Core.Mediator.Attributes;
    using Core.Mediator.Handlers;
    using UnityEditor;
    using UnityEngine;

    public static class MediatorHandlerScanner
    {
        private const string OutputPath = "Assets/Source/Runtime/Core/CompositionRoot/Generated";
        private const string OutputFileName = "MediatorHandlerRegistration.generated.cs";
        private const string Namespace = "Core.DependencyInjection.Generated";

        [MenuItem("Tools/Mediator/Scan & Generate Handler Registration")]
        public static void ScanAndGenerate()
        {
            if (Generate())
                AssetDatabase.Refresh();
        }

        public static bool Generate()
        {
            var scanResult = ScanAllHandlers();
            var code = GenerateRegistrationCode(scanResult);

            if (!WriteIfChanged(code))
            {
                Debug.Log("[MediatorScanner] No changes detected, skipping.");
                return false;
            }

            Debug.Log($"[MediatorScanner] Generated: " +
                      $"{scanResult.RequestHandlers.Count} request, " +
                      $"{scanResult.NotificationHandlers.Count} notification, " +
                      $"{scanResult.StreamHandlers.Count} stream, " +
                      $"{scanResult.ViewModelTypes.Count} viewmodel, " +
                      $"{scanResult.GenericHandlerUsages.Count} generic handlers");
            return true;
        }

        [MenuItem("Tools/Mediator/List All Handlers (Dry Run)")]
        public static void ListAllHandlers()
        {
            var scanResult = ScanAllHandlers();
            
            var sb = new StringBuilder();
            sb.AppendLine("=== REQUEST HANDLERS ===");
            foreach (var h in scanResult.RequestHandlers)
                sb.AppendLine($"  {h.HandlerType.Name} : {h.RequestType.Name} → {h.ResponseType.Name}");
            
            sb.AppendLine("\n=== NOTIFICATION HANDLERS ===");
            foreach (var h in scanResult.NotificationHandlers)
                sb.AppendLine($"  {h.HandlerType.Name} : {h.NotificationType.Name}");
            
            sb.AppendLine("\n=== STREAM HANDLERS ===");
            foreach (var h in scanResult.StreamHandlers)
                sb.AppendLine($"  {h.HandlerType.Name} : {h.RequestType.Name} → {h.ResponseType.Name}");
            
            sb.AppendLine("\n=== VIEWMODELS (Generic Handlers) ===");
            foreach (var vm in scanResult.ViewModelTypes)
                sb.AppendLine($"  {GetFullTypeName(vm)} → ShowViewHandler, HideViewHandler");
            
            Debug.Log(sb.ToString());
        }

        private static ScanResult ScanAllHandlers()
        {
            var result = new ScanResult();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !a.FullName.StartsWith("Unity") &&
                            !a.FullName.StartsWith("System") && !a.FullName.StartsWith("mscorlib") &&
                            !IsTestAssembly(a))
                .ToList();

            foreach (var assembly in assemblies)
            {
                try
                {
                    Type[] types;
                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        types = ex.Types.Where(t => t != null).ToArray();
                    }
                    
                    foreach (var type in types)
                    {
                        try
                        {
                            if (type.IsAbstract || type.IsInterface || !type.IsClass)
                                continue;

                            if (type.ContainsGenericParameters)
                                continue;

                            ScanRequestHandlers(type, result);
                            ScanNotificationHandlers(type, result);
                            ScanStreamHandlers(type, result);
                            ScanViewModels(type, result);
                        }
                        catch (Exception typeEx)
                        {
                            Debug.LogWarning($"[MediatorScanner] Failed to scan type {type?.Name}: {typeEx.Message}");
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[MediatorScanner] Failed to process assembly {assembly.GetName().Name}: {ex.Message}");
                }
            }

            ScanGenericHandlerUsages(result);

            return result;
        }

        private static void ScanRequestHandlers(Type type, ScanResult result)
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

            foreach (var iface in interfaces)
            {
                var args = iface.GetGenericArguments();
                result.RequestHandlers.Add(new RequestHandlerInfo
                {
                    HandlerType = type,
                    RequestType = args[0],
                    ResponseType = args[1]
                });
            }
        }

        private static void ScanNotificationHandlers(Type type, ScanResult result)
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>));

            foreach (var iface in interfaces)
            {
                var args = iface.GetGenericArguments();
                result.NotificationHandlers.Add(new NotificationHandlerInfo
                {
                    HandlerType = type,
                    NotificationType = args[0]
                });
            }
        }

        private static void ScanStreamHandlers(Type type, ScanResult result)
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStreamRequestHandler<,>));

            foreach (var iface in interfaces)
            {
                var args = iface.GetGenericArguments();
                result.StreamHandlers.Add(new StreamHandlerInfo
                {
                    HandlerType = type,
                    RequestType = args[0],
                    ResponseType = args[1]
                });
            }
        }

        private static void ScanViewModels(Type type, ScanResult result)
        {
            var hasAttribute = type.GetCustomAttribute<GenerateViewCommandsAttribute>() != null;
            if (hasAttribute && !string.IsNullOrEmpty(type.Namespace))
            {
                result.ViewModelTypes.Add(type);
            }
        }

        private static void ScanGenericHandlerUsages(ScanResult result)
        {
            var sourceDir = Path.Combine(Application.dataPath, "Source/Runtime");
            if (!Directory.Exists(sourceDir))
            {
                Debug.LogWarning("[MediatorScanner] Source/Runtime directory not found");
                return;
            }

            var patterns = new Dictionary<string, Regex>
            {
                ["GetConfigQuery"] = new Regex(@"new\s+GetConfigQuery<([^>]+)>\s*\(", RegexOptions.Compiled),
                ["SavePrefsCommand"] = new Regex(@"new\s+SavePrefsCommand<([^>]+)>\s*\(", RegexOptions.Compiled),
                ["LoadPrefsQuery"] = new Regex(@"new\s+LoadPrefsQuery<([^>]+)>\s*\(", RegexOptions.Compiled),
                ["LoadGameDataQuery"] = new Regex(@"new\s+LoadGameDataQuery<([^>]+)>\s*\(", RegexOptions.Compiled)
            };

            var csFiles = Directory.GetFiles(sourceDir, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains("/Generated/") && !f.Contains("\\Generated\\") &&
                            !f.Contains("/Tests/") && !f.Contains("\\Tests\\"))
                .ToList();

            var foundUsages = new HashSet<string>();

            foreach (var file in csFiles)
            {
                try
                {
                    var content = File.ReadAllText(file);
                    var fileNamespace = ExtractNamespace(content);

                    foreach (var kvp in patterns)
                    {
                        var matches = kvp.Value.Matches(content);
                        foreach (Match match in matches)
                        {
                            var typeArg = match.Groups[1].Value.Trim();
                            var key = $"{kvp.Key}<{typeArg}>";
                            
                            if (foundUsages.Contains(key))
                                continue;
                                
                            foundUsages.Add(key);

                            var typeArgNamespace = ResolveTypeNamespace(content, typeArg, fileNamespace);

                            result.GenericHandlerUsages.Add(new GenericHandlerUsage
                            {
                                RequestTypeName = kvp.Key,
                                TypeArgument = typeArg,
                                TypeArgumentNamespace = typeArgNamespace
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[MediatorScanner] Failed to scan file {file}: {ex.Message}");
                }
            }

        }

        private static string ExtractNamespace(string content)
        {
            var match = Regex.Match(content, @"namespace\s+([\w.]+)");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static string ResolveTypeNamespace(string content, string typeName, string fileNamespace)
        {
            var simpleTypeName = typeName.Contains(".") ? typeName : typeName;
            
            if (typeName.Contains("."))
                return typeName.Substring(0, typeName.LastIndexOf('.'));

            var usingPattern = new Regex($@"using\s+([\w.]+\.{Regex.Escape(typeName)});|using\s+([\w.]+);");
            var matches = usingPattern.Matches(content);
            
            foreach (Match match in matches)
            {
                if (match.Groups[1].Success)
                    return match.Groups[1].Value.Replace($".{typeName}", "");
            }

            var allUsings = Regex.Matches(content, @"using\s+([\w.]+);")
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ToList();

            foreach (var ns in allUsings)
            {
                var fullTypeName = $"{ns}.{typeName}";
                var type = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                    .FirstOrDefault(t => t.FullName == fullTypeName);
                    
                if (type != null)
                    return ns;
            }

            return fileNamespace;
        }

        private static string GenerateRegistrationCode(ScanResult scanResult)
        {
            var sb = new StringBuilder();
            var usings = new HashSet<string>
            {
                "Core.Mediator.Registration"
            };

            foreach (var h in scanResult.RequestHandlers)
            {
                CollectNamespaces(usings, h.HandlerType);
                CollectNamespaces(usings, h.RequestType);
                CollectNamespaces(usings, h.ResponseType);
            }

            foreach (var h in scanResult.NotificationHandlers)
            {
                CollectNamespaces(usings, h.HandlerType);
                CollectNamespaces(usings, h.NotificationType);
            }

            foreach (var h in scanResult.StreamHandlers)
            {
                CollectNamespaces(usings, h.HandlerType);
                CollectNamespaces(usings, h.RequestType);
                CollectNamespaces(usings, h.ResponseType);
            }

            if (scanResult.ViewModelTypes.Count > 0)
            {
                usings.Add("Core.Application.Commands.Common");
                usings.Add("Core.Application.Handlers.Common");
                foreach (var vm in scanResult.ViewModelTypes)
                {
                    CollectNamespaces(usings, vm);
                }
            }

            if (scanResult.GenericHandlerUsages.Count > 0)
            {
                usings.Add("Core.Mediator.Abstractions");
                usings.Add("Core.Application.Commands.Common");
                usings.Add("Core.Application.Handlers.Common");
                usings.Add("Core.Application.Queries.Common");
                usings.Add("Core.Application.Queries.GameData");
                usings.Add("Core.Application.Handlers.GameData");
                foreach (var usage in scanResult.GenericHandlerUsages)
                {
                    if (!string.IsNullOrEmpty(usage.TypeArgumentNamespace))
                        usings.Add(NormalizeNamespace(usage.TypeArgumentNamespace));
                }
            }

            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("// This file was generated by MediatorHandlerScanner.");
            sb.AppendLine("// Do not edit manually. Run 'Tools/Mediator/Scan & Generate Handler Registration' to regenerate.");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine();
            sb.AppendLine("#pragma warning disable");
            sb.AppendLine();

            foreach (var ns in usings.Where(n => !string.IsNullOrEmpty(n)).OrderBy(n => n))
            {
                sb.AppendLine($"using {ns};");
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {Namespace}");
            sb.AppendLine("{");
            sb.AppendLine("    public static class MediatorHandlerRegistration");
            sb.AppendLine("    {");
            sb.AppendLine("        public static MediatorBuilder RegisterAllHandlers(this MediatorBuilder builder)");
            sb.AppendLine("        {");

            if (scanResult.RequestHandlers.Count > 0)
            {
                sb.AppendLine("            // Request Handlers");
                foreach (var h in scanResult.RequestHandlers.OrderBy(x => x.HandlerType.Name))
                {
                    var request = GetFullTypeName(h.RequestType);
                    var response = GetFullTypeName(h.ResponseType);
                    var handler = GetFullTypeName(h.HandlerType);
                    sb.AppendLine($"            builder.AddHandler<{request}, {response}, {handler}>();");
                }
                sb.AppendLine();
            }

            if (scanResult.NotificationHandlers.Count > 0)
            {
                sb.AppendLine("            // Notification Handlers");
                foreach (var h in scanResult.NotificationHandlers.OrderBy(x => x.HandlerType.Name))
                {
                    var notification = GetFullTypeName(h.NotificationType);
                    var handler = GetFullTypeName(h.HandlerType);
                    sb.AppendLine($"            builder.AddNotificationHandler<{notification}, {handler}>();");
                }
                sb.AppendLine();
            }

            if (scanResult.StreamHandlers.Count > 0)
            {
                sb.AppendLine("            // Stream Handlers");
                foreach (var h in scanResult.StreamHandlers.OrderBy(x => x.HandlerType.Name))
                {
                    var request = GetFullTypeName(h.RequestType);
                    var response = GetFullTypeName(h.ResponseType);
                    var handler = GetFullTypeName(h.HandlerType);
                    sb.AppendLine($"            builder.AddStreamHandler<{request}, {response}, {handler}>();");
                }
                sb.AppendLine();
            }

            if (scanResult.ViewModelTypes.Count > 0)
            {
                sb.AppendLine("            // Generic View Handlers (from [GenerateViewCommands] attribute)");
                foreach (var vm in scanResult.ViewModelTypes.OrderBy(x => x.Name))
                {
                    var vmName = vm.Name;
                    sb.AppendLine($"            builder.AddHandler<ShowViewCommand<{vmName}>, bool, ShowViewHandler<{vmName}>>();");
                    sb.AppendLine($"            builder.AddHandler<HideViewCommand<{vmName}>, bool, HideViewHandler<{vmName}>>();");
                }
                sb.AppendLine();
            }

            if (scanResult.GenericHandlerUsages.Count > 0)
            {
                sb.AppendLine("            // Generic Handlers (from source file scan)");
                foreach (var usage in scanResult.GenericHandlerUsages.OrderBy(x => x.RequestTypeName).ThenBy(x => x.TypeArgument))
                {
                    var typeArg = usage.TypeArgument;
                    switch (usage.RequestTypeName)
                    {
                        case "GetConfigQuery":
                            sb.AppendLine($"            builder.AddHandler<GetConfigQuery<{typeArg}>, {typeArg}, GetConfigHandler<{typeArg}>>();");
                            break;
                        case "SavePrefsCommand":
                            sb.AppendLine($"            builder.AddHandler<SavePrefsCommand<{typeArg}>, Nothing, SavePrefsHandler<{typeArg}>>();");
                            break;
                        case "LoadPrefsQuery":
                            sb.AppendLine($"            builder.AddHandler<LoadPrefsQuery<{typeArg}>, {typeArg}, LoadPrefsHandler<{typeArg}>>();");
                            break;
                        case "LoadGameDataQuery":
                            sb.AppendLine($"            builder.AddHandler<LoadGameDataQuery<{typeArg}>, GameDataResult<{typeArg}>, LoadGameDataHandler<{typeArg}>>();");
                            break;
                    }
                }
                sb.AppendLine();
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

        private static void CollectNamespaces(HashSet<string> usings, Type type)
        {
            if (type == null)
                return;

            if (!string.IsNullOrEmpty(type.Namespace) && 
                (type.Namespace == "System.Collections.Generic" || 
                 (!type.Namespace.StartsWith("System.") && type.Namespace != "System")))
            {
                usings.Add(NormalizeNamespace(type.Namespace));
            }

            if (type.IsGenericType)
            {
                foreach (var arg in type.GetGenericArguments())
                    CollectNamespaces(usings, arg);
            }
        }

        private static readonly Dictionary<Type, string> PrimitiveTypeAliases = new()
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(string), "string" },
            { typeof(object), "object" },
            { typeof(void), "void" }
        };

        private static string GetFullTypeName(Type type)
        {
            if (type == null)
                return "void";

            if (PrimitiveTypeAliases.TryGetValue(type, out var alias))
                return alias;

            if (!type.IsGenericType)
                return type.Name;

            var name = type.Name;
            var backtickIndex = name.IndexOf('`');
            if (backtickIndex > 0)
                name = name.Substring(0, backtickIndex);

            var args = string.Join(", ", type.GetGenericArguments().Select(GetFullTypeName));
            return $"{name}<{args}>";
        }

        private static bool IsTestAssembly(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            return name.Contains("Tests") ||
                   name.Contains("Test") ||
                   name.Contains("nunit") ||
                   name.Contains("NSubstitute");
        }

        private static bool WriteIfChanged(string content)
        {
            var directory = Path.Combine(Application.dataPath.Replace("/Assets", ""), OutputPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, OutputFileName);

            if (File.Exists(filePath) && File.ReadAllText(filePath) == content)
                return false;

            File.WriteAllText(filePath, content);
            return true;
        }

        private class ScanResult
        {
            public List<RequestHandlerInfo> RequestHandlers { get; } = new();
            public List<NotificationHandlerInfo> NotificationHandlers { get; } = new();
            public List<StreamHandlerInfo> StreamHandlers { get; } = new();
            public List<Type> ViewModelTypes { get; } = new();
            public List<GenericHandlerUsage> GenericHandlerUsages { get; } = new();
        }
        
        private class GenericHandlerUsage
        {
            public string RequestTypeName { get; set; }
            public string TypeArgument { get; set; }
            public string TypeArgumentNamespace { get; set; }
        }

        private class RequestHandlerInfo
        {
            public Type HandlerType { get; set; }
            public Type RequestType { get; set; }
            public Type ResponseType { get; set; }
        }

        private class NotificationHandlerInfo
        {
            public Type HandlerType { get; set; }
            public Type NotificationType { get; set; }
        }

        private class StreamHandlerInfo
        {
            public Type HandlerType { get; set; }
            public Type RequestType { get; set; }
            public Type ResponseType { get; set; }
        }
    }
}
