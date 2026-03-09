namespace Core.Abstractions.UI.Validation
{
    public readonly struct ValidationResult
    {
        public readonly bool IsValid;
        public readonly string ErrorMessage;

        public ValidationResult(bool isValid, string errorMessage = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public static ValidationResult Valid => new(true);
        public static ValidationResult Invalid(string message) => new(false, message);
    }
}
