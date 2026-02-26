using System;
using System.Reflection;
using System.Text;

namespace Core.Editor.ViewMappingGenerator
{
    public static class ViewBindingsGenerator
    {
        public static string GenerateBindings(Type viewModelType)
        {
            var builder = new StringBuilder();
            var properties = viewModelType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var propertyType = property.PropertyType;

                if (IsReactiveProperty(propertyType) || IsReadOnlyReactiveProperty(propertyType))
                {
                    builder.AppendLine("        ViewModel." + property.Name);
                    builder.AppendLine("            .Subscribe(value => _ = value)");
                    builder.AppendLine("            .AddTo(disposables);");
                    builder.AppendLine();
                }
                else if (IsReactiveCommand(propertyType))
                {
                    builder.AppendLine("        ViewModel." + property.Name);
                    builder.AppendLine("            .BindTo(default(UnityEngine.UI.Button))");
                    builder.AppendLine("            .AddTo(disposables);");
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }

        private static bool IsReactiveProperty(Type type)
        {
            return IsDerivedFromGeneric(type, "UniRx.ReactiveProperty`1");
        }

        private static bool IsReadOnlyReactiveProperty(Type type)
        {
            return IsDerivedFromGeneric(type, "UniRx.ReadOnlyReactiveProperty`1");
        }

        private static bool IsReactiveCommand(Type type)
        {
            var current = type;
            while (current != null)
            {
                var fullName = current.FullName;
                if (fullName == "UniRx.ReactiveCommand")
                {
                    return true;
                }

                if (current.IsGenericType)
                {
                    var genericDefinition = current.GetGenericTypeDefinition();
                    if (genericDefinition.FullName == "UniRx.ReactiveCommand`1")
                    {
                        return true;
                    }
                }

                current = current.BaseType;
            }

            return false;
        }

        private static bool IsDerivedFromGeneric(Type type, string genericFullName)
        {
            var current = type;
            while (current != null)
            {
                if (current.IsGenericType)
                {
                    var genericDefinition = current.GetGenericTypeDefinition();
                    if (genericDefinition.FullName == genericFullName)
                    {
                        return true;
                    }
                }

                current = current.BaseType;
            }

            return false;
        }
    }
}
