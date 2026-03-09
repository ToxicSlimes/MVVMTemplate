namespace Core.Abstractions.UI.Validation
{
    public interface IValidationRule
    {
        ValidationResult Validate(string value);
    }
}
