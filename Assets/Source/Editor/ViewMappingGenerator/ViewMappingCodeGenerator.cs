using System.Text;

namespace Core.Editor.ViewMappingGenerator
{
    public static class ViewMappingCodeGenerator
    {
        public static string GenerateMappingCode(string viewName,
            string viewNamespace = null, string viewModelNamespace = null)
        {
            var builder = new StringBuilder();
            builder.AppendLine("using Core.Presentation.MVVM.Config;");
            builder.AppendLine("using Core.Abstractions.MVVM.Views;");
            if (!string.IsNullOrEmpty(viewModelNamespace))
                builder.AppendLine("using " + viewModelNamespace + ";");
            if (!string.IsNullOrEmpty(viewNamespace) && viewNamespace != viewModelNamespace)
                builder.AppendLine("using " + viewNamespace + ";");
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine();
            builder.AppendLine("namespace Core.Views." + viewName);
            builder.AppendLine("{");
            builder.Append("    public class ");
            builder.Append(viewName);
            builder.AppendLine("ViewMapping : ViewMappingBase<" + viewName + "View, " + viewName + "ViewModel>");
            builder.AppendLine("    {");
            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }
    }
}
