#if UNITY_EDITOR
using System.Collections.Generic;
using EndlessAdventure.Editor.PrefabBuilderSystem.Core;
using EndlessAdventure.Editor.PrefabBuilderSystem.Validation;
using UnityEngine;
using UnityEngine.UI;

namespace Editor.PrefabBuilderSystem.Base.Validation.Validators
{
    public class LayoutValidator : BaseUIValidator
    {
        public override string Name => "Layout Validator";
        public override string Description => "Validates LayoutGroup configurations and potential conflicts";

        public override IEnumerable<ValidationIssue> Validate(GameObject root, PrefabBuilderContext context)
        {
            var issues = new List<ValidationIssue>();
            ValidateRecursive(root.transform, issues);
            return issues;
        }

        private void ValidateRecursive(Transform transform, List<ValidationIssue> issues)
        {
            var layoutGroup = transform.GetComponent<LayoutGroup>();
            var rt = transform.GetComponent<RectTransform>();

            if (layoutGroup != null)
            {
                ValidateLayoutGroup(layoutGroup, rt, issues);
            }

            var layoutElement = transform.GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                ValidateLayoutElement(layoutElement, transform.parent, issues);
            }

            foreach (Transform child in transform)
            {
                ValidateRecursive(child, issues);
            }
        }

        private void ValidateLayoutGroup(LayoutGroup layoutGroup, RectTransform rt, List<ValidationIssue> issues)
        {
            var go = layoutGroup.gameObject;

            var childrenWithAbsolutePos = new List<Transform>();
            foreach (Transform child in layoutGroup.transform)
            {
                var childRt = child.GetComponent<RectTransform>();
                if (childRt != null)
                {
                    bool hasCustomAnchors = childRt.anchorMin != childRt.anchorMax ||
                                           (childRt.anchorMin != new Vector2(0.5f, 0.5f) &&
                                            childRt.anchorMin != Vector2.zero);

                    if (hasCustomAnchors && childRt.anchoredPosition != Vector2.zero)
                    {
                        childrenWithAbsolutePos.Add(child);
                    }
                }
            }

            if (childrenWithAbsolutePos.Count > 0)
            {
                issues.Add(Warning(
                    $"LayoutGroup has {childrenWithAbsolutePos.Count} children with custom anchors/positions that may be overridden",
                    go,
                    "Children of LayoutGroup should not have custom anchors - LayoutGroup controls positioning"));
            }

            if (layoutGroup is HorizontalLayoutGroup hlg)
            {
                if (!hlg.childControlWidth && !hlg.childControlHeight && 
                    !hlg.childForceExpandWidth && !hlg.childForceExpandHeight)
                {
                    issues.Add(Info(
                        "HorizontalLayoutGroup has all control options disabled",
                        go,
                        "Consider if LayoutGroup is needed or enable some control options"));
                }
            }

            if (layoutGroup is VerticalLayoutGroup vlg)
            {
                if (!vlg.childControlWidth && !vlg.childControlHeight && 
                    !vlg.childForceExpandWidth && !vlg.childForceExpandHeight)
                {
                    issues.Add(Info(
                        "VerticalLayoutGroup has all control options disabled",
                        go,
                        "Consider if LayoutGroup is needed or enable some control options"));
                }
            }

            if (layoutGroup is GridLayoutGroup glg)
            {
                if (glg.cellSize == Vector2.zero)
                {
                    issues.Add(Error(
                        "GridLayoutGroup has zero cell size",
                        go,
                        "Set a valid cell size for GridLayoutGroup"));
                }
            }

            var contentSizeFitter = go.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter != null && layoutGroup.transform.parent != null)
            {
                var parentLayoutGroup = layoutGroup.transform.parent.GetComponent<LayoutGroup>();
                if (parentLayoutGroup != null)
                {
                    if ((parentLayoutGroup is HorizontalLayoutGroup && contentSizeFitter.horizontalFit != ContentSizeFitter.FitMode.Unconstrained) ||
                        (parentLayoutGroup is VerticalLayoutGroup && contentSizeFitter.verticalFit != ContentSizeFitter.FitMode.Unconstrained))
                    {
                        issues.Add(Info(
                            "ContentSizeFitter on child of LayoutGroup - may cause layout calculation issues",
                            go,
                            "Consider layout structure to avoid nested ContentSizeFitter conflicts"));
                    }
                }
            }
        }

        private void ValidateLayoutElement(LayoutElement le, Transform parent, List<ValidationIssue> issues)
        {
            if (parent == null) return;

            var parentLayoutGroup = parent.GetComponent<LayoutGroup>();
            if (parentLayoutGroup == null)
            {
                if (le.preferredWidth >= 0 || le.preferredHeight >= 0 || 
                    le.minWidth >= 0 || le.minHeight >= 0 ||
                    le.flexibleWidth >= 0 || le.flexibleHeight >= 0)
                {
                    issues.Add(Info(
                        "LayoutElement without parent LayoutGroup has no effect",
                        le.gameObject,
                        "LayoutElement only works when parent has a LayoutGroup"));
                }
            }
        }
    }
}
#endif
