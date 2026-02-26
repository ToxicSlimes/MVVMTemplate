#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using EndlessAdventure.Editor.PrefabBuilderSystem.Conventions;
using EndlessAdventure.Editor.PrefabBuilderSystem.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Validation
{
    public class NamingValidator : BaseUIValidator
    {
        public override string Name => "Naming Validator";
        public override string Description => "Validates element naming follows conventions";

        public bool RequirePrefix { get; set; } = false;
        public bool WarnOnMissingPrefix { get; set; } = true;
        public bool CheckGlobalUniqueness { get; set; } = false;

        private static readonly HashSet<string> StandardUnityUIChildren = new()
        {
            "Background", "Fill Area", "Fill", "Handle Slide Area", "Handle",
            "Checkmark", "Label", "Arrow", "Template", "Viewport", "Content",
            "Item", "Item Background", "Item Checkmark", "Item Label",
            "Scrollbar Horizontal", "Scrollbar Vertical", "Sliding Area",
            "Text Area", "Placeholder", "Text", "Caret", "X"
        };

        private static readonly HashSet<string> StructuralContainerNames = new()
        {
            "Header", "Footer", "Content", "ContentArea", "Body", "Container",
            "TabBar", "Tabs", "DimBackground", "Overlay", "Background",
            "ModalWindow", "Window", "Dialog", "Wrapper", "Root",
            "Title", "Subtitle", "Description", "Divider", "Spacer", "Separator",
            "CloseButton", "BackButton", "NextButton", "PrevButton", "SubmitButton", "CancelButton"
        };

        private static readonly HashSet<string> InternalChildNames = new()
        {
            "Label", "Value", "Icon", "Text", "Title", "Slider", "Toggle",
            "Dropdown", "Background", "Checkmark", "Handle", "Fill", "Arrow",
            "X", "CloseIcon", "Indicator", "Thumb"
        };

        private static readonly string[] ViewSuffixes = { "View", "Window", "Modal", "Dialog", "Panel", "Screen", "Popup" };

        public override IEnumerable<ValidationIssue> Validate(GameObject root, PrefabBuilderContext context)
        {
            var issues = new List<ValidationIssue>();
            var globalNames = CheckGlobalUniqueness ? new HashSet<string>() : null;
            ValidateRecursive(root.transform, issues, globalNames, root.transform);
            return issues;
        }

        private void ValidateRecursive(Transform transform, List<ValidationIssue> issues, HashSet<string> globalNames, Transform root)
        {
            var go = transform.gameObject;
            var name = go.name;

            var skipValidation = ShouldSkipValidation(transform, root);
            
            var siblingNames = new HashSet<string>();
            foreach (Transform sibling in transform.parent != null ? transform.parent : transform)
            {
                if (sibling != transform && !ShouldSkipValidation(sibling, root))
                    siblingNames.Add(sibling.name);
            }

            if (!skipValidation)
            {
                var oldLogWarnings = NamingConvention.LogWarnings;
                NamingConvention.LogWarnings = false;
                var validationResult = NamingConvention.Validate(name, siblingNames);
                NamingConvention.LogWarnings = oldLogWarnings;
                
                if (validationResult.HasError)
                {
                    issues.Add(Error(validationResult.Message, go));
                }
                else if (globalNames != null && globalNames.Contains(name))
                {
                    issues.Add(Warning($"Element '{name}' appears multiple times in hierarchy (consider using unique names)", go));
                }
                else if (validationResult.HasWarning && WarnOnMissingPrefix)
                {
                    var expectedPrefix = GetExpectedPrefix(go);
                    if (expectedPrefix != null)
                    {
                        var suggestedName = NamingConvention.Format(expectedPrefix, name);
                        if (RequirePrefix)
                        {
                            issues.Add(Error(
                                $"Element '{name}' should follow naming convention",
                                go,
                                $"Rename to '{suggestedName}'",
                                () => go.name = suggestedName));
                        }
                        else
                        {
                            issues.Add(Warning(
                                $"Element '{name}' does not follow naming convention",
                                go,
                                $"Consider renaming to '{suggestedName}'",
                                () => go.name = suggestedName));
                        }
                    }
                }

                globalNames?.Add(name);
            }

            foreach (Transform child in transform)
            {
                ValidateRecursive(child, issues, globalNames, root);
            }
        }

        private bool ShouldSkipValidation(Transform transform, Transform root)
        {
            if (transform == root)
                return true;

            var name = transform.name;

            if (IsStandardUnityUIChild(transform))
                return true;

            if (ViewSuffixes.Any(suffix => name.EndsWith(suffix)))
                return true;

            if (StructuralContainerNames.Contains(name))
                return true;

            if (IsInternalChildOfPrefixedParent(transform))
                return true;

            if (name.Contains("Tab_") || name.Contains("_Content") || name.Contains("_Tab"))
                return true;

            return false;
        }

        private bool IsInternalChildOfPrefixedParent(Transform transform)
        {
            if (transform.parent == null) return false;
            
            var name = transform.name;
            if (!InternalChildNames.Contains(name)) return false;

            var parent = transform.parent;
            if (NamingConvention.HasPrefix(parent.name))
                return true;

            var current = parent;
            var depth = 0;
            while (current != null && depth < 3)
            {
                var go = current.gameObject;
                if (go.GetComponent<Slider>() != null ||
                    go.GetComponent<Toggle>() != null ||
                    go.GetComponent<TMP_Dropdown>() != null ||
                    go.GetComponent<Dropdown>() != null ||
                    go.GetComponent<ScrollRect>() != null ||
                    go.GetComponent<TMP_InputField>() != null ||
                    go.GetComponent<InputField>() != null ||
                    go.GetComponent<Button>() != null)
                {
                    return true;
                }
                current = current.parent;
                depth++;
            }

            return false;
        }

        private bool IsStandardUnityUIChild(Transform transform)
        {
            if (transform.parent == null) return false;
            if (!StandardUnityUIChildren.Contains(transform.name)) return false;
            
            var current = transform.parent;
            var depth = 0;
            const int maxDepth = 6;
            
            while (current != null && depth < maxDepth)
            {
                var go = current.gameObject;
                if (go.GetComponent<Slider>() != null ||
                    go.GetComponent<Toggle>() != null ||
                    go.GetComponent<TMP_Dropdown>() != null ||
                    go.GetComponent<Dropdown>() != null ||
                    go.GetComponent<ScrollRect>() != null ||
                    go.GetComponent<TMP_InputField>() != null ||
                    go.GetComponent<InputField>() != null)
                {
                    return true;
                }
                current = current.parent;
                depth++;
            }

            return false;
        }

        private string GetExpectedPrefix(GameObject go)
        {
            if (go.GetComponent<Button>()) return NamingConvention.Button;
            if (go.GetComponent<TMP_Text>()) return NamingConvention.Text;
            if (go.GetComponent<TMP_InputField>()) return NamingConvention.Input;
            if (go.GetComponent<TMP_Dropdown>()) return NamingConvention.Dropdown;
            if (go.GetComponent<Toggle>()) return NamingConvention.Toggle;
            if (go.GetComponent<Slider>()) return NamingConvention.Slider;
            if (go.GetComponent<ScrollRect>()) return NamingConvention.Scroll;
            if (go.GetComponent<RawImage>()) return NamingConvention.RawImage;
            if (go.GetComponent<Image>()) return NamingConvention.Image;
            if (go.GetComponent<GridLayoutGroup>()) return NamingConvention.Grid;
            if (go.GetComponent<HorizontalLayoutGroup>() || go.GetComponent<VerticalLayoutGroup>()) return NamingConvention.Layout;
            if (go.GetComponent<Canvas>()) return NamingConvention.Canvas;

            return null;
        }
    }
}
#endif
