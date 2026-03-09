using Core.Abstractions.UI.Validation;
using Core.UI.Controls;

namespace Core.UI.Binding
{
    public static class ValidationExtensions
    {
        public static BaseInputField WithValidation(this BaseInputField input, IValidationRule rule)
        {
            input.AddRule(rule);
            return input;
        }

        public static BaseInputField Required(this BaseInputField input, string message = "This field is required.")
        {
            return input.WithValidation(new RequiredRule(message));
        }

        public static BaseInputField MinLength(this BaseInputField input, int min, string message = null)
        {
            return input.WithValidation(new MinLengthRule(min, message));
        }

        public static BaseInputField MaxLength(this BaseInputField input, int max, string message = null)
        {
            return input.WithValidation(new MaxLengthRule(max, message));
        }

        public static BaseInputField Range(this BaseInputField input, float min, float max, string message = null)
        {
            return input.WithValidation(new RangeRule(min, max, message));
        }

        public static BaseInputField MatchesPattern(this BaseInputField input, string pattern, string message = "Invalid format.")
        {
            return input.WithValidation(new RegexRule(pattern, message));
        }
    }
}
