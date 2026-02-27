#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Editor.PrefabBuilderSystem.Core;
using Editor.PrefabBuilderSystem.Base.Validation.Validators;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Validation
{
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }

    public class ValidationIssue
    {
        public ValidationSeverity Severity { get; }
        public string Message { get; }
        public string ValidatorName { get; }
        public GameObject Element { get; }
        public string ElementPath { get; }
        public string FixSuggestion { get; }
        public Action AutoFix { get; }

        public ValidationIssue(
            ValidationSeverity severity,
            string message,
            string validatorName,
            GameObject element = null,
            string fixSuggestion = null,
            Action autoFix = null)
        {
            Severity = severity;
            Message = message;
            ValidatorName = validatorName;
            Element = element;
            ElementPath = element != null ? GetGameObjectPath(element) : null;
            FixSuggestion = fixSuggestion;
            AutoFix = autoFix;
        }

        private static string GetGameObjectPath(GameObject go)
        {
            var path = go.name;
            var parent = go.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        public bool CanAutoFix => AutoFix != null;
    }

    public class ValidationReport
    {
        public List<ValidationIssue> Issues { get; } = new();
        public DateTime Timestamp { get; } = DateTime.Now;
        public string RootName { get; set; }

        public bool HasCritical => Issues.Any(i => i.Severity == ValidationSeverity.Critical);
        public bool HasErrors => Issues.Any(i => i.Severity == ValidationSeverity.Error);
        public bool HasWarnings => Issues.Any(i => i.Severity == ValidationSeverity.Warning);
        public bool IsValid => !HasCritical && !HasErrors;

        public int CriticalCount => Issues.Count(i => i.Severity == ValidationSeverity.Critical);
        public int ErrorCount => Issues.Count(i => i.Severity == ValidationSeverity.Error);
        public int WarningCount => Issues.Count(i => i.Severity == ValidationSeverity.Warning);
        public int InfoCount => Issues.Count(i => i.Severity == ValidationSeverity.Info);

        public void Add(ValidationIssue issue) => Issues.Add(issue);
        public void AddRange(IEnumerable<ValidationIssue> issues) => Issues.AddRange(issues);

        public IEnumerable<ValidationIssue> GetBySeverity(ValidationSeverity severity) =>
            Issues.Where(i => i.Severity == severity);

        public void LogToConsole()
        {
            foreach (var issue in Issues.OrderByDescending(i => i.Severity))
            {
                var prefix = issue.Severity switch
                {
                    ValidationSeverity.Critical => "🔴 CRITICAL",
                    ValidationSeverity.Error => "❌ ERROR",
                    ValidationSeverity.Warning => "⚠️ WARNING",
                    ValidationSeverity.Info => "ℹ️ INFO",
                    _ => "?"
                };

                var message = $"[{issue.ValidatorName}] {prefix}: {issue.Message}";
                if (!string.IsNullOrEmpty(issue.ElementPath))
                    message += $" (at {issue.ElementPath})";

                switch (issue.Severity)
                {
                    case ValidationSeverity.Critical:
                    case ValidationSeverity.Error:
                        Debug.LogError(message);
                        break;
                    case ValidationSeverity.Warning:
                        Debug.LogWarning(message);
                        break;
                    default:
                        Debug.Log(message);
                        break;
                }
            }
        }

        public string GetSummary()
        {
            if (Issues.Count == 0)
                return "✅ No issues found";

            var parts = new List<string>();
            if (CriticalCount > 0) parts.Add($"{CriticalCount} critical");
            if (ErrorCount > 0) parts.Add($"{ErrorCount} errors");
            if (WarningCount > 0) parts.Add($"{WarningCount} warnings");
            if (InfoCount > 0) parts.Add($"{InfoCount} info");

            return string.Join(", ", parts);
        }
    }

    public interface IUIValidator
    {
        string Name { get; }
        string Description { get; }
        bool IsEnabled { get; set; }
        IEnumerable<ValidationIssue> Validate(GameObject root, PrefabBuilderContext context);
    }

    public abstract class BaseUIValidator : IUIValidator
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public bool IsEnabled { get; set; } = true;

        public abstract IEnumerable<ValidationIssue> Validate(GameObject root, PrefabBuilderContext context);

        protected ValidationIssue Info(string message, GameObject element = null, string fix = null) =>
            new(ValidationSeverity.Info, message, Name, element, fix);

        protected ValidationIssue Warning(string message, GameObject element = null, string fix = null, Action autoFix = null) =>
            new(ValidationSeverity.Warning, message, Name, element, fix, autoFix);

        protected ValidationIssue Error(string message, GameObject element = null, string fix = null, Action autoFix = null) =>
            new(ValidationSeverity.Error, message, Name, element, fix, autoFix);

        protected ValidationIssue Critical(string message, GameObject element = null, string fix = null) =>
            new(ValidationSeverity.Critical, message, Name, element, fix);
    }

    public class UIValidationSystem
    {
        private readonly List<IUIValidator> _validators = new();

        public static UIValidationSystem Instance { get; } = new();

        public bool ValidateBeforeBuild { get; set; } = true;
        public bool BlockBuildOnError { get; set; } = true;
        public bool BlockBuildOnCritical { get; set; } = true;

        public UIValidationSystem()
        {
            RegisterBuiltInValidators();
        }

        private void RegisterBuiltInValidators()
        {
            Register(new AnchorValidator());
            Register(new HierarchyValidator());
            Register(new LayoutValidator());
            Register(new NamingValidator());
            Register(new ComponentValidator());
            Register(new ReferenceValidator());
        }

        public void Register(IUIValidator validator)
        {
            if (_validators.All(v => v.GetType() != validator.GetType()))
                _validators.Add(validator);
        }

        public void Unregister<T>() where T : IUIValidator
        {
            _validators.RemoveAll(v => v is T);
        }

        public T GetValidator<T>() where T : IUIValidator
        {
            return _validators.OfType<T>().FirstOrDefault();
        }

        public IReadOnlyList<IUIValidator> GetAllValidators() => _validators.AsReadOnly();

        public ValidationReport Validate(GameObject root, PrefabBuilderContext context = null)
        {
            var report = new ValidationReport { RootName = root?.name ?? "null" };

            if (root == null)
            {
                report.Add(new ValidationIssue(
                    ValidationSeverity.Critical,
                    "Root GameObject is null",
                    "System"));
                return report;
            }

            foreach (var validator in _validators.Where(v => v.IsEnabled))
            {
                try
                {
                    var issues = validator.Validate(root, context);
                    report.AddRange(issues);
                }
                catch (Exception ex)
                {
                    report.Add(new ValidationIssue(
                        ValidationSeverity.Error,
                        $"Validator threw exception: {ex.Message}",
                        validator.Name));
                }
            }

            return report;
        }

        public bool CanBuild(ValidationReport report)
        {
            if (BlockBuildOnCritical && report.HasCritical)
                return false;
            if (BlockBuildOnError && report.HasErrors)
                return false;
            return true;
        }
    }
}
#endif
