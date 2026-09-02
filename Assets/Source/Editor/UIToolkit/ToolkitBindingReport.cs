#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Editor.UIToolkit
{
    public enum ToolkitBindingSeverity
    {
        Warning,
        Error
    }

    public sealed class ToolkitBindingIssue
    {
        public ToolkitBindingSeverity Severity { get; }
        public string Message { get; }
        public string ViewName { get; }

        public ToolkitBindingIssue(ToolkitBindingSeverity severity, string viewName, string message)
        {
            Severity = severity;
            ViewName = viewName;
            Message = message;
        }
    }

    public sealed class ToolkitBindingReport
    {
        public List<ToolkitBindingIssue> Issues { get; } = new();

        public bool IsValid => !HasErrors;
        public bool HasErrors => Issues.Any(i => i.Severity == ToolkitBindingSeverity.Error);
        public int ErrorCount => Issues.Count(i => i.Severity == ToolkitBindingSeverity.Error);
        public int WarningCount => Issues.Count(i => i.Severity == ToolkitBindingSeverity.Warning);

        public void Add(ToolkitBindingIssue issue) => Issues.Add(issue);

        public void LogToConsole()
        {
            foreach (var issue in Issues.OrderByDescending(i => i.Severity))
            {
                var prefix = issue.Severity == ToolkitBindingSeverity.Error ? "ERROR" : "WARNING";
                var message = $"[Toolkit Binding] {prefix} ({issue.ViewName}): {issue.Message}";
                if (issue.Severity == ToolkitBindingSeverity.Error)
                    UnityEngine.Debug.LogError(message);
                else
                    UnityEngine.Debug.LogWarning(message);
            }

            if (Issues.Count == 0)
                UnityEngine.Debug.Log("[Toolkit Binding] No issues found.");
        }
    }
}
#endif
