using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Core.Editor.UIToolkit.ScreenSpec
{
    public static class ScreenSpecUxmlGenerator
    {
        public static string Generate(ScreenSpecDocument spec, string sourceFileName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!-- Generated from " + sourceFileName + " - do not edit -->");
            sb.AppendLine("<ui:UXML xmlns:ui=\"UnityEngine.UIElements\" editor-extension-mode=\"False\">");

            foreach (var style in spec.theme?.styles ?? new List<string>())
            {
                var path = style.Contains("/") ? style : "../Theme/" + style;
                sb.AppendLine("    <Style src=\"" + path + "\" />");
            }

            if (spec.output.includePixelControls && !ContainsStyle(spec, "pixel-controls.uss"))
                sb.AppendLine("    <Style src=\"../Theme/pixel-controls.uss\" />");

            if (spec.output.includeToolkitScreen && !ContainsStyle(spec, "toolkit-screen.uss"))
                sb.AppendLine("    <Style src=\"../Theme/toolkit-screen.uss\" />");

            var screenUss = spec.output.ussFile;
            if (!string.IsNullOrEmpty(screenUss))
                sb.AppendLine("    <Style src=\"" + screenUss + "\" />");

            foreach (var node in spec.nodes)
                AppendNode(sb, node, 1);

            sb.AppendLine("</ui:UXML>");
            return sb.ToString();
        }

        private static bool ContainsStyle(ScreenSpecDocument spec, string fileName)
        {
            if (spec.theme?.styles == null)
                return false;

            foreach (var s in spec.theme.styles)
            {
                if (s == fileName || s.EndsWith(fileName))
                    return true;
            }

            return false;
        }

        private static void AppendNode(StringBuilder sb, ScreenSpecNode node, int depth)
        {
            var indent = new string(' ', depth * 4);

            switch (node.type)
            {
                case "Label":
                    sb.Append(indent);
                    sb.Append("<ui:Label");
                    AppendAttr(sb, "text", node.text);
                    AppendAttr(sb, "class", node.@class);
                    AppendAttr(sb, "name", node.name);
                    sb.AppendLine(" />");
                    break;

                case "Button":
                    sb.Append(indent);
                    sb.Append("<ui:Button");
                    AppendAttr(sb, "name", node.name);
                    AppendAttr(sb, "text", node.text);
                    if (node.rawClass)
                        AppendAttr(sb, "class", node.@class);
                    else
                        AppendAttr(sb, "class", ScreenSpecToneMapper.ToButtonClasses(node.tone, node.@class));
                    sb.AppendLine(" />");
                    break;

                case "SliderRow":
                    sb.AppendLine(indent + "<ui:VisualElement class=\"settings-field-row\">");
                    sb.Append(indent + "    ");
                    sb.Append("<ui:Label");
                    AppendAttr(sb, "text", node.label);
                    sb.AppendLine(" class=\"settings-field-label\" />");
                    sb.Append(indent + "    ");
                    sb.Append("<ui:Slider");
                    AppendAttr(sb, "name", node.name);
                    AppendAttr(sb, "label", "", allowEmpty: true);
                    sb.Append(" low-value=\"" + node.low.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\"");
                    sb.Append(" high-value=\"" + node.high.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\"");
                    sb.AppendLine(" class=\"pixel-slider\" />");
                    sb.Append(indent + "    ");
                    sb.Append("<ui:Label");
                    AppendAttr(sb, "name", node.valueName);
                    sb.AppendLine(" class=\"settings-field-value\" />");
                    sb.AppendLine(indent + "</ui:VisualElement>");
                    break;

                case "Toggle":
                    sb.Append(indent);
                    sb.Append("<ui:Toggle");
                    AppendAttr(sb, "name", node.name);
                    AppendAttr(sb, "label", node.label);
                    sb.AppendLine(" class=\"pixel-toggle\" />");
                    break;

                case "Dropdown":
                    sb.Append(indent);
                    sb.Append("<ui:DropdownField");
                    AppendAttr(sb, "name", node.name);
                    AppendAttr(sb, "label", node.label);
                    sb.AppendLine(" class=\"pixel-dropdown\" />");
                    break;

                case "Tabs":
                    sb.AppendLine(indent + "<ui:VisualElement class=\"settings-tabs\">");
                    foreach (var tab in node.tabs)
                    {
                        sb.Append(indent + "    ");
                        sb.Append("<ui:Button");
                        AppendAttr(sb, "name", tab.tabButtonName);
                        AppendAttr(sb, "text", tab.label);
                        sb.AppendLine(" class=\"btn btn-outline tab-button\" />");
                    }
                    sb.AppendLine(indent + "</ui:VisualElement>");
                    sb.AppendLine(indent + "<ui:ScrollView class=\"settings-content-scroll\">");
                    foreach (var tab in node.tabs)
                    {
                        sb.AppendLine(indent + "    <ui:VisualElement name=\"content-" + tab.id + "\" class=\"tab-content\">");
                        foreach (var child in tab.children)
                            AppendNode(sb, child, depth + 2);
                        sb.AppendLine(indent + "    </ui:VisualElement>");
                    }
                    sb.AppendLine(indent + "</ui:ScrollView>");
                    break;

                case "ScrollView":
                    sb.AppendLine(indent + "<ui:ScrollView class=\"" + (node.@class ?? "scroll-panel") + "\">");
                    foreach (var child in node.children)
                        AppendNode(sb, child, depth + 1);
                    sb.AppendLine(indent + "</ui:ScrollView>");
                    break;

                default:
                    sb.AppendLine(indent + "<ui:VisualElement" + BuildOpenAttrs(node) + ">");
                    foreach (var child in node.children)
                        AppendNode(sb, child, depth + 1);
                    sb.AppendLine(indent + "</ui:VisualElement>");
                    break;
            }
        }

        private static string BuildOpenAttrs(ScreenSpecNode node)
        {
            var sb = new StringBuilder();
            AppendAttr(sb, "name", node.name);
            AppendAttr(sb, "class", node.@class);
            return sb.ToString();
        }

        private static void AppendAttr(StringBuilder sb, string name, string value, bool allowEmpty = false)
        {
            if (value == null)
                return;

            if (!allowEmpty && value.Length == 0)
                return;

            sb.Append(" ");
            sb.Append(name);
            sb.Append("=\"");
            sb.Append(XmlEscape(value));
            sb.Append("\"");
        }

        private static string XmlEscape(string value)
        {
            return value
                .Replace("&", "&amp;")
                .Replace("\"", "&quot;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }
    }
}
