using System;
using UnityEngine;

namespace Core.Abstractions.Validation
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ValidateMissingRefsAttribute : PropertyAttribute
    {
        public string ErrorMessage { get; }
        public bool ShowWarningIcon { get; }

        public ValidateMissingRefsAttribute(string errorMessage = null, bool showWarningIcon = true)
        {
            ErrorMessage = errorMessage ?? "Missing reference detected!";
            ShowWarningIcon = showWarningIcon;
        }
    }
}
