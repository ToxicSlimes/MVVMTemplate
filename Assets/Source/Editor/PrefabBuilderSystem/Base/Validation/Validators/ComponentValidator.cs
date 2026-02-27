#if UNITY_EDITOR
using System.Collections.Generic;
using Core.Editor.PrefabBuilderSystem.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.Editor.PrefabBuilderSystem.Validation
{
    public class ComponentValidator : BaseUIValidator
    {
        public override string Name => "Component Validator";
        public override string Description => "Validates required components and their configuration";

        public override IEnumerable<ValidationIssue> Validate(GameObject root, PrefabBuilderContext context)
        {
            var issues = new List<ValidationIssue>();

            var canvas = root.GetComponent<Canvas>();
            if (canvas == null)
            {
                issues.Add(Error(
                    "Root GameObject is missing Canvas component",
                    root,
                    "Add Canvas component to root",
                    () => root.AddComponent<Canvas>()));
            }
            else
            {
                var canvasScaler = root.GetComponent<CanvasScaler>();
                if (canvasScaler == null)
                {
                    issues.Add(Warning(
                        "Canvas is missing CanvasScaler component",
                        root,
                        "Add CanvasScaler for responsive UI",
                        () => root.AddComponent<CanvasScaler>()));
                }

                var raycaster = root.GetComponent<GraphicRaycaster>();
                if (raycaster == null)
                {
                    issues.Add(Warning(
                        "Canvas is missing GraphicRaycaster component",
                        root,
                        "Add GraphicRaycaster for UI interaction",
                        () => root.AddComponent<GraphicRaycaster>()));
                }
            }

            ValidateRecursive(root.transform, issues);

            return issues;
        }

        private void ValidateRecursive(Transform transform, List<ValidationIssue> issues)
        {
            var go = transform.gameObject;

            var button = go.GetComponent<Button>();
            if (button != null)
            {
                ValidateButton(button, issues);
            }

            var inputField = go.GetComponent<TMP_InputField>();
            if (inputField != null)
            {
                ValidateInputField(inputField, issues);
            }

            var dropdown = go.GetComponent<TMP_Dropdown>();
            if (dropdown != null)
            {
                ValidateDropdown(dropdown, issues);
            }

            var slider = go.GetComponent<Slider>();
            if (slider != null)
            {
                ValidateSlider(slider, issues);
            }

            var toggle = go.GetComponent<Toggle>();
            if (toggle != null)
            {
                ValidateToggle(toggle, issues);
            }

            var scrollRect = go.GetComponent<ScrollRect>();
            if (scrollRect != null)
            {
                ValidateScrollRect(scrollRect, issues);
            }

            foreach (Transform child in transform)
            {
                ValidateRecursive(child, issues);
            }
        }

        private void ValidateButton(Button button, List<ValidationIssue> issues)
        {
            if (button.targetGraphic == null)
            {
                var image = button.GetComponent<Image>();
                if (image != null)
                {
                    issues.Add(Warning(
                        "Button has no targetGraphic but has Image component",
                        button.gameObject,
                        "Set targetGraphic for visual feedback",
                        () => button.targetGraphic = image));
                }
            }
        }

        private void ValidateInputField(TMP_InputField inputField, List<ValidationIssue> issues)
        {
            if (inputField.textComponent == null)
            {
                issues.Add(Error(
                    "TMP_InputField is missing textComponent reference",
                    inputField.gameObject,
                    "Assign a TMP_Text component for text display"));
            }

            if (inputField.textViewport == null)
            {
                issues.Add(Warning(
                    "TMP_InputField is missing textViewport reference",
                    inputField.gameObject,
                    "Assign a RectTransform for text viewport"));
            }
        }

        private void ValidateDropdown(TMP_Dropdown dropdown, List<ValidationIssue> issues)
        {
            if (dropdown.template == null)
            {
                issues.Add(Error(
                    "TMP_Dropdown is missing template reference",
                    dropdown.gameObject,
                    "Assign a RectTransform for dropdown template"));
            }

            if (dropdown.captionText == null)
            {
                issues.Add(Warning(
                    "TMP_Dropdown is missing captionText reference",
                    dropdown.gameObject,
                    "Assign a TMP_Text for caption display"));
            }
        }

        private void ValidateSlider(Slider slider, List<ValidationIssue> issues)
        {
            if (slider.minValue >= slider.maxValue)
            {
                issues.Add(Error(
                    $"Slider minValue ({slider.minValue}) >= maxValue ({slider.maxValue})",
                    slider.gameObject,
                    "Set minValue < maxValue"));
            }

            if (slider.fillRect == null && slider.handleRect == null)
            {
                issues.Add(Warning(
                    "Slider has no fillRect or handleRect",
                    slider.gameObject,
                    "Assign fillRect and/or handleRect for visual feedback"));
            }
        }

        private void ValidateToggle(Toggle toggle, List<ValidationIssue> issues)
        {
            if (toggle.graphic == null)
            {
                issues.Add(Warning(
                    "Toggle is missing checkmark graphic",
                    toggle.gameObject,
                    "Assign a Graphic for checkmark display"));
            }
        }

        private void ValidateScrollRect(ScrollRect scrollRect, List<ValidationIssue> issues)
        {
            if (scrollRect.content == null)
            {
                issues.Add(Error(
                    "ScrollRect is missing content reference",
                    scrollRect.gameObject,
                    "Assign a RectTransform for scrollable content"));
            }

            if (scrollRect.viewport == null)
            {
                issues.Add(Warning(
                    "ScrollRect is missing viewport reference",
                    scrollRect.gameObject,
                    "Assign a RectTransform with Mask for viewport"));
            }
            else
            {
                var mask = scrollRect.viewport.GetComponent<Mask>();
                var rectMask = scrollRect.viewport.GetComponent<RectMask2D>();
                if (mask == null && rectMask == null)
                {
                    issues.Add(Warning(
                        "ScrollRect viewport has no Mask or RectMask2D",
                        scrollRect.viewport.gameObject,
                        "Add Mask or RectMask2D to viewport for proper clipping"));
                }
            }
        }
    }
}
#endif
