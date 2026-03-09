using System.Globalization;
using Core.Abstractions.UI.Validation;

namespace Core.Abstractions.UI.Validation
{
    public class RangeRule : IValidationRule
    {
        private readonly float _min;
        private readonly float _max;
        private readonly string _message;

        public RangeRule(float min, float max, string message = null)
        {
            _min = min;
            _max = max;
            _message = message ?? $"Value must be between {min} and {max}.";
        }

        public ValidationResult Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ValidationResult.Invalid(_message);

            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                return ValidationResult.Invalid("Value must be a number.");

            return number < _min || number > _max
                ? ValidationResult.Invalid(_message)
                : ValidationResult.Valid;
        }
    }
}
