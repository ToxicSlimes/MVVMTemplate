using Core.Abstractions.UI.Validation;

namespace Core.Abstractions.UI.Validation
{
    public class MaxLengthRule : IValidationRule
    {
        private readonly int _maxLength;
        private readonly string _message;

        public MaxLengthRule(int maxLength, string message = null)
        {
            _maxLength = maxLength;
            _message = message ?? $"Maximum length is {maxLength} characters.";
        }

        public ValidationResult Validate(string value) =>
            value != null && value.Length > _maxLength
                ? ValidationResult.Invalid(_message)
                : ValidationResult.Valid;
    }
}
