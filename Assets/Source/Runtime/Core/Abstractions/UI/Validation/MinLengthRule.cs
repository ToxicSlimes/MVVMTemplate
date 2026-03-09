using Core.Abstractions.UI.Validation;

namespace Core.Abstractions.UI.Validation
{
    public class MinLengthRule : IValidationRule
    {
        private readonly int _minLength;
        private readonly string _message;

        public MinLengthRule(int minLength, string message = null)
        {
            _minLength = minLength;
            _message = message ?? $"Minimum length is {minLength} characters.";
        }

        public ValidationResult Validate(string value) =>
            string.IsNullOrEmpty(value) || value.Length < _minLength
                ? ValidationResult.Invalid(_message)
                : ValidationResult.Valid;
    }
}
