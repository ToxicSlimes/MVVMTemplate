#if UNITY_EDITOR
using System.Collections.Generic;
using EndlessAdventure.Editor.PrefabBuilderSystem.Core;
using EndlessAdventure.Editor.PrefabBuilderSystem.Validation;
using UnityEngine;

namespace Editor.PrefabBuilderSystem.Base.Validation.Validators
{
    public class AnchorValidator : BaseUIValidator
    {
        public override string Name => "Anchor Validator";
        public override string Description => "Validates that anchors are within valid range (0-1)";

        public override IEnumerable<ValidationIssue> Validate(GameObject root, PrefabBuilderContext context)
        {
            var issues = new List<ValidationIssue>();
            ValidateRecursive(root.transform, issues);
            return issues;
        }

        private void ValidateRecursive(Transform transform, List<ValidationIssue> issues)
        {
            var rt = transform.GetComponent<RectTransform>();
            if (rt != null)
            {
                if (rt.anchorMin.x < 0 || rt.anchorMin.x > 1 ||
                    rt.anchorMin.y < 0 || rt.anchorMin.y > 1)
                {
                    issues.Add(Warning(
                        $"AnchorMin ({rt.anchorMin}) is outside valid range [0,1]",
                        rt.gameObject,
                        "Set anchorMin values between 0 and 1",
                        () => rt.anchorMin = new Vector2(
                            Mathf.Clamp01(rt.anchorMin.x),
                            Mathf.Clamp01(rt.anchorMin.y))));
                }

                if (rt.anchorMax.x < 0 || rt.anchorMax.x > 1 ||
                    rt.anchorMax.y < 0 || rt.anchorMax.y > 1)
                {
                    issues.Add(Warning(
                        $"AnchorMax ({rt.anchorMax}) is outside valid range [0,1]",
                        rt.gameObject,
                        "Set anchorMax values between 0 and 1",
                        () => rt.anchorMax = new Vector2(
                            Mathf.Clamp01(rt.anchorMax.x),
                            Mathf.Clamp01(rt.anchorMax.y))));
                }

                if (rt.anchorMin.x > rt.anchorMax.x)
                {
                    issues.Add(Error(
                        $"AnchorMin.x ({rt.anchorMin.x}) > AnchorMax.x ({rt.anchorMax.x})",
                        rt.gameObject,
                        "AnchorMin.x should be <= AnchorMax.x"));
                }

                if (rt.anchorMin.y > rt.anchorMax.y)
                {
                    issues.Add(Error(
                        $"AnchorMin.y ({rt.anchorMin.y}) > AnchorMax.y ({rt.anchorMax.y})",
                        rt.gameObject,
                        "AnchorMin.y should be <= AnchorMax.y"));
                }
            }

            foreach (Transform child in transform)
            {
                ValidateRecursive(child, issues);
            }
        }
    }
}
#endif
