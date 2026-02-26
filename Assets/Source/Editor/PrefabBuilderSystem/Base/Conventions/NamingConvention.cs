#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Conventions
{
    public enum ValidationSeverity { Valid, Warning, Error }

    public readonly struct ValidationResult
    {
        public ValidationSeverity Severity { get; }
        public string Message { get; }
        public string ElementName { get; }

        public bool IsValid => Severity == ValidationSeverity.Valid;
        public bool HasWarning => Severity == ValidationSeverity.Warning;
        public bool HasError => Severity == ValidationSeverity.Error;

        public ValidationResult(ValidationSeverity severity, string message = null, string elementName = null)
        {
            Severity = severity;
            Message = message;
            ElementName = elementName;
        }

        public static ValidationResult Valid() => new(ValidationSeverity.Valid);
        public static ValidationResult Warning(string message, string elementName = null) => 
            new(ValidationSeverity.Warning, message, elementName);
        public static ValidationResult Error(string message, string elementName = null) => 
            new(ValidationSeverity.Error, message, elementName);
    }

    public static class NamingConvention
    {
        public static bool AutoApply { get; set; } = true;
        public static bool LogWarnings { get; set; } = true;
        public static bool StrictMode { get; set; } = false;

        private static readonly HashSet<char> ForbiddenChars = new() { '/', '\\', ':', '*', '?', '"', '<', '>', '|' };

        public const string Button = "Button_";
        public const string Text = "Text_";
        public const string Image = "Image_";
        public const string RawImage = "RawImage_";
        public const string Panel = "Panel_";
        public const string Scroll = "Scroll_";
        public const string Toggle = "Toggle_";
        public const string Slider = "Slider_";
        public const string Input = "Input_";
        public const string Dropdown = "Dropdown_";
        public const string Layout = "Layout_";
        public const string Canvas = "Canvas_";
        public const string Grid = "Grid_";

        public static string Format(string prefix, string name) => $"{prefix}{name}";

        public static bool HasPrefix(string name)
        {
            return !string.IsNullOrEmpty(name) && name.Contains("_") && TryParse(name, out _, out _);
        }

        public static string ApplyPrefix(string name, string prefix)
        {
            if (string.IsNullOrEmpty(name)) return name;
            if (!AutoApply) return name;
            if (HasPrefix(name)) return name;
            return Format(prefix, name);
        }

        public static ValidationResult Validate(string name, HashSet<string> existingNames = null)
        {
            if (string.IsNullOrEmpty(name))
                return ValidationResult.Error("Element name cannot be empty", name);

            if (string.IsNullOrWhiteSpace(name))
                return ValidationResult.Error("Element name cannot be whitespace only", name);

            foreach (var c in name)
            {
                if (ForbiddenChars.Contains(c))
                    return ValidationResult.Error($"Element name contains forbidden character: '{c}'", name);
            }

            if (existingNames != null && existingNames.Contains(name))
                return ValidationResult.Error($"Duplicate element name: '{name}'", name);

            if (!HasPrefix(name))
            {
                var result = ValidationResult.Warning($"Element name '{name}' does not follow naming convention (missing prefix)", name);
                if (LogWarnings)
                    Debug.LogWarning($"[NamingConvention] {result.Message}");
                if (StrictMode)
                    return ValidationResult.Error(result.Message, name);
                return result;
            }

            return ValidationResult.Valid();
        }

        public static List<ValidationResult> ValidateAll(IEnumerable<string> names)
        {
            var results = new List<ValidationResult>();
            var existingNames = new HashSet<string>();

            foreach (var name in names)
            {
                var result = Validate(name, existingNames);
                if (!result.IsValid)
                    results.Add(result);
                existingNames.Add(name);
            }

            return results;
        }

        public static bool TryParse(string elementName, out string prefix, out string name)
        {
            var match = Regex.Match(elementName, @"^([A-Z][a-zA-Z]*_)(.+)$");
            if (match.Success)
            {
                prefix = match.Groups[1].Value;
                name = match.Groups[2].Value;
                return true;
            }
            prefix = null;
            name = null;
            return false;
        }

        public static string ToFieldName(string elementName)
        {
            if (!TryParse(elementName, out var prefix, out var name))
                return null;

            var typeSuffix = prefix.TrimEnd('_');
            var fieldName = char.ToLower(name[0]) + name.Substring(1);
            return $"_{fieldName}{typeSuffix}";
        }

        public static string ToElementName(string fieldName, Type fieldType)
        {
            if (string.IsNullOrEmpty(fieldName) || !fieldName.StartsWith("_"))
                return null;

            var prefix = GetPrefixForType(fieldType);
            if (prefix == null)
                return null;

            var withoutUnderscore = fieldName.Substring(1);
            var typeSuffix = prefix.TrimEnd('_').ToLower();

            string baseName;
            if (withoutUnderscore.EndsWith(typeSuffix, StringComparison.OrdinalIgnoreCase))
            {
                baseName = withoutUnderscore.Substring(0, withoutUnderscore.Length - typeSuffix.Length);
            }
            else if (withoutUnderscore.EndsWith(prefix.TrimEnd('_'), StringComparison.OrdinalIgnoreCase))
            {
                baseName = withoutUnderscore.Substring(0, withoutUnderscore.Length - prefix.TrimEnd('_').Length);
            }
            else
            {
                baseName = withoutUnderscore;
            }

            if (string.IsNullOrEmpty(baseName))
                return null;

            return prefix + char.ToUpper(baseName[0]) + baseName.Substring(1);
        }

        public static string GetPrefixForType(Type type)
        {
            if (type == null) return null;

            var typeName = type.Name;

            return typeName switch
            {
                "Button" => Button,
                "TMP_Text" or "TextMeshProUGUI" => Text,
                "Image" => Image,
                "RawImage" => RawImage,
                "Toggle" => Toggle,
                "Slider" => Slider,
                "TMP_InputField" => Input,
                "GridLayoutGroup" => Grid,
                "TMP_Dropdown" => Dropdown,
                "ScrollRect" => Scroll,
                "HorizontalLayoutGroup" or "VerticalLayoutGroup" or "GridLayoutGroup" => Layout,
                "Canvas" => Canvas,
                "GameObject" or "RectTransform" or "Transform" => Panel,
                _ => null
            };
        }
    }
}
#endif
