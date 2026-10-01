using System.Collections.Generic;
using System.Text;

namespace Core.Editor.UIToolkit.ScreenSpec
{
    public static class ScreenSpecViewGenerator
    {
        public static string GenerateBindingsPartial(ScreenSpecDocument spec)
        {
            var sb = new StringBuilder();
            var viewNs = string.IsNullOrEmpty(spec.viewNamespace)
                ? "Core.MVVM.Views"
                : spec.viewNamespace;

            sb.AppendLine("using Core.Abstractions.Localization;");
            sb.AppendLine("using Core.Abstractions.Services;");
            sb.AppendLine("using R3;");
            sb.AppendLine("using UnityEngine.UIElements;");
            sb.AppendLine();
            sb.AppendLine("namespace " + viewNs);
            sb.AppendLine("{");
            sb.AppendLine("    public partial class " + spec.viewType);
            sb.AppendLine("    {");
            sb.AppendLine("        private void ApplyGeneratedBindings(ref DisposableBag disposables)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (_localizationService == null)");
            sb.AppendLine("                return;");
            sb.AppendLine();

            var emitted = new HashSet<string>();
            foreach (var binding in spec.bindings)
            {
                if (binding.kind != "localized")
                    continue;

                var key = binding.localizationKey ?? binding.name;
                if (!emitted.Add(key))
                    continue;

                if (binding.queryByClass)
                {
                    var elementType = binding.elementType ?? "Label";
                    sb.AppendLine("            BindLocalizedByClass(_localizationService, \"" + binding.className + "\", typeof(" + elementType + "), UiLocalizationKeys.Table, \"" + binding.localizationKey + "\", ref disposables);");
                }
                else if (binding.elementType == "Button")
                {
                    sb.AppendLine("            BindLocalized(Root.Q<Button>(\"" + binding.name + "\"), _localizationService, UiLocalizationKeys.Table, \"" + binding.localizationKey + "\", ref disposables);");
                }
                else if (binding.elementType == "Dropdown")
                {
                    sb.AppendLine("            BindLocalizedLabel(Root.Q<DropdownField>(\"" + binding.name + "\"), _localizationService, UiLocalizationKeys.Table, \"" + binding.localizationKey + "\", ref disposables);");
                }
            }

            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        private void BindLocalizedByClass(ILocalizationService localization, string className, System.Type elementType, string table, string key, ref DisposableBag disposables)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (Root == null) return;");
            sb.AppendLine("            if (elementType == typeof(Label))");
            sb.AppendLine("            {");
            sb.AppendLine("                var label = Root.Q<Label>(className: className);");
            sb.AppendLine("                BindLocalized(label, localization, table, key, ref disposables);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
