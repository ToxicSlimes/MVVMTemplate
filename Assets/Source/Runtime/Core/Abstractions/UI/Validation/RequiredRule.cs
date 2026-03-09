using Core.Abstractions.UI.Validation;

namespace Core.Abstractions.UI.Validation
{
    public class RequiredRule : IValidationRule
    {
        private readonly string _message;

        public RequiredRule(string message = "This field is required.")
        {
            _message = message;
        }

        public ValidationResult Validate(string value) =>
            string.IsNullOrWhiteSpace(value)
                ? ValidationResult.Invalid(_message)
                : ValidationResult.Valid;
    }
}
