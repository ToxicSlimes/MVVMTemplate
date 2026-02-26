#if UNITY_EDITOR
using System.Collections.Generic;
using EndlessAdventure.Editor.PrefabBuilderSystem.Core;
using EndlessAdventure.Editor.PrefabBuilderSystem.Validation;
using UnityEngine;

namespace Editor.PrefabBuilderSystem.Base.Validation.Validators
{
    public class HierarchyValidator : BaseUIValidator
    {
        public override string Name => "Hierarchy Validator";
        public override string Description => "Validates hierarchy depth and structure";

        public int MaxDepth { get; set; } = 15;
        public int MaxChildren { get; set; } = 100;

        public override IEnumerable<ValidationIssue> Validate(GameObject root, PrefabBuilderContext context)
        {
            var issues = new List<ValidationIssue>();
            ValidateRecursive(root.transform, 0, issues);
            return issues;
        }

        private void ValidateRecursive(Transform transform, int depth, List<ValidationIssue> issues)
        {
            if (depth > MaxDepth)
            {
                issues.Add(Warning(
                    $"Hierarchy depth ({depth}) exceeds recommended maximum ({MaxDepth})",
                    transform.gameObject,
                    "Consider flattening the hierarchy for better performance"));
                return;
            }

            var childCount = transform.childCount;
            if (childCount > MaxChildren)
            {
                issues.Add(Warning(
                    $"Too many direct children ({childCount}) - recommended max is {MaxChildren}",
                    transform.gameObject,
                    "Consider grouping children or using virtualization"));
            }

            if (string.IsNullOrWhiteSpace(transform.name))
            {
                issues.Add(Error(
                    "GameObject has empty or whitespace name",
                    transform.gameObject,
                    "Give the GameObject a meaningful name"));
            }

            if (transform.name.Contains("(Clone)"))
            {
                issues.Add(Info(
                    "GameObject name contains '(Clone)' suffix",
                    transform.gameObject,
                    "Consider renaming instantiated objects"));
            }

            foreach (Transform child in transform)
            {
                ValidateRecursive(child, depth + 1, issues);
            }
        }
    }
}
#endif
