#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Validation
{
    public class ValidationReportWindow : EditorWindow
    {
        private ValidationReport _report;
        private Vector2 _scrollPosition;
        private bool _showInfo = true;
        private bool _showWarnings = true;
        private bool _showErrors = true;
        private bool _showCritical = true;
        private string _searchFilter = "";

        [MenuItem("Tools/UI Prefab Builder/Validation Report")]
        public static void ShowWindow()
        {
            var window = GetWindow<ValidationReportWindow>();
            window.titleContent = new GUIContent("Validation Report", EditorGUIUtility.IconContent("console.infoicon").image);
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        public static void ShowReport(ValidationReport report)
        {
            var window = GetWindow<ValidationReportWindow>();
            window._report = report;
            window.titleContent = new GUIContent("Validation Report", EditorGUIUtility.IconContent("console.infoicon").image);
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_report == null || _report.Issues.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    _report == null 
                        ? "No validation report available.\nRun validation from a Builder window." 
                        : "✅ No issues found!",
                    _report == null ? MessageType.Info : MessageType.None);
                return;
            }

            DrawSummary();
            DrawFilters();
            DrawIssuesList();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                _report = null;
            }

            GUILayout.FlexibleSpace();

            if (_report != null && _report.Issues.Any(i => i.CanAutoFix))
            {
                if (GUILayout.Button("Fix All Auto-Fixable", EditorStyles.toolbarButton, GUILayout.Width(120)))
                {
                    FixAllAutoFixable();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSummary()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"Report: {_report.RootName}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Generated: {_report.Timestamp:HH:mm:ss}");

            EditorGUILayout.BeginHorizontal();

            if (_report.CriticalCount > 0)
                DrawCountBadge("Critical", _report.CriticalCount, new Color(0.8f, 0.2f, 0.2f));
            if (_report.ErrorCount > 0)
                DrawCountBadge("Errors", _report.ErrorCount, new Color(0.9f, 0.3f, 0.3f));
            if (_report.WarningCount > 0)
                DrawCountBadge("Warnings", _report.WarningCount, new Color(0.9f, 0.7f, 0.2f));
            if (_report.InfoCount > 0)
                DrawCountBadge("Info", _report.InfoCount, new Color(0.3f, 0.6f, 0.9f));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawCountBadge(string label, int count, Color color)
        {
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUILayout.Label($"{label}: {count}", EditorStyles.miniButton, GUILayout.Width(80));
            GUI.backgroundColor = oldColor;
        }

        private void DrawFilters()
        {
            EditorGUILayout.BeginHorizontal();

            _searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField);

            var iconSize = GUILayout.Width(24);
            _showCritical = GUILayout.Toggle(_showCritical, new GUIContent("🔴", "Critical"), EditorStyles.toolbarButton, iconSize);
            _showErrors = GUILayout.Toggle(_showErrors, new GUIContent("❌", "Errors"), EditorStyles.toolbarButton, iconSize);
            _showWarnings = GUILayout.Toggle(_showWarnings, new GUIContent("⚠️", "Warnings"), EditorStyles.toolbarButton, iconSize);
            _showInfo = GUILayout.Toggle(_showInfo, new GUIContent("ℹ️", "Info"), EditorStyles.toolbarButton, iconSize);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawIssuesList()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            var filteredIssues = GetFilteredIssues();

            foreach (var issue in filteredIssues.OrderByDescending(i => i.Severity))
            {
                DrawIssue(issue);
            }

            EditorGUILayout.EndScrollView();
        }

        private IEnumerable<ValidationIssue> GetFilteredIssues()
        {
            return _report.Issues.Where(issue =>
            {
                if (!_showCritical && issue.Severity == ValidationSeverity.Critical) return false;
                if (!_showErrors && issue.Severity == ValidationSeverity.Error) return false;
                if (!_showWarnings && issue.Severity == ValidationSeverity.Warning) return false;
                if (!_showInfo && issue.Severity == ValidationSeverity.Info) return false;

                if (!string.IsNullOrEmpty(_searchFilter))
                {
                    var searchLower = _searchFilter.ToLower();
                    if (!issue.Message.ToLower().Contains(searchLower) &&
                        !issue.ValidatorName.ToLower().Contains(searchLower) &&
                        (issue.ElementPath == null || !issue.ElementPath.ToLower().Contains(searchLower)))
                    {
                        return false;
                    }
                }

                return true;
            });
        }

        private void DrawIssue(ValidationIssue issue)
        {
            var bgColor = issue.Severity switch
            {
                ValidationSeverity.Critical => new Color(0.8f, 0.2f, 0.2f, 0.3f),
                ValidationSeverity.Error => new Color(0.9f, 0.3f, 0.3f, 0.2f),
                ValidationSeverity.Warning => new Color(0.9f, 0.7f, 0.2f, 0.2f),
                _ => new Color(0.3f, 0.6f, 0.9f, 0.2f)
            };

            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = bgColor;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = oldColor;

            EditorGUILayout.BeginHorizontal();

            var icon = issue.Severity switch
            {
                ValidationSeverity.Critical => "🔴",
                ValidationSeverity.Error => "❌",
                ValidationSeverity.Warning => "⚠️",
                _ => "ℹ️"
            };

            EditorGUILayout.LabelField($"{icon} [{issue.ValidatorName}]", EditorStyles.boldLabel, GUILayout.Width(200));
            GUILayout.FlexibleSpace();

            if (issue.Element != null)
            {
                if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(50)))
                {
                    Selection.activeGameObject = issue.Element;
                    EditorGUIUtility.PingObject(issue.Element);
                }
            }

            if (issue.CanAutoFix)
            {
                if (GUILayout.Button("Fix", EditorStyles.miniButton, GUILayout.Width(40)))
                {
                    issue.AutoFix?.Invoke();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(issue.Message, EditorStyles.wordWrappedLabel);

            if (!string.IsNullOrEmpty(issue.ElementPath))
            {
                EditorGUILayout.LabelField($"Path: {issue.ElementPath}", EditorStyles.miniLabel);
            }

            if (!string.IsNullOrEmpty(issue.FixSuggestion))
            {
                EditorGUILayout.LabelField($"💡 {issue.FixSuggestion}", EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(2);
        }

        private void FixAllAutoFixable()
        {
            var fixableIssues = _report.Issues.Where(i => i.CanAutoFix).ToList();
            foreach (var issue in fixableIssues)
            {
                try
                {
                    issue.AutoFix?.Invoke();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to auto-fix: {ex.Message}");
                }
            }

            Debug.Log($"Applied {fixableIssues.Count} auto-fixes");
        }
    }
}
#endif
