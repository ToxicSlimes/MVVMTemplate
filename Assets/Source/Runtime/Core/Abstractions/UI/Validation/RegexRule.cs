using System.Text.RegularExpressions;
using Core.Abstractions.UI.Validation;

namespace Core.Abstractions.UI.Validation
{
    public class RegexRule : IValidationRule
    {
        private readonly Regex _regex;
        private readonly string _message;

        public RegexRule(string pattern, string message = "Invalid format.")
        {
            _regex = new Regex(pattern, RegexOptions.Compiled);
            _message = message;
        }

        public ValidationResult Validate(string value) =>
            string.IsNullOrEmpty(value) || !_regex.IsMatch(value)
                ? ValidationResult.Invalid(_message)
                : ValidationResult.Valid;
    }
}
