using System.Text.RegularExpressions;

namespace Talby.Core.Validation;

public sealed class MatchesRegexValidator : ValueValidator<string>
{
    public const string ErrorCode = "MatchesRegex";

    public MatchesRegexValidator(Regex pattern)
    {
        Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
    }

    public Regex Pattern { get; }
    public Func<string>? ErrorMessageFunc { get; init; }

    protected override ValidationResult Validate(IValidationContext context, string value)
    {
        if (!Pattern.IsMatch(value))
        {
            var errorFunc = ErrorMessageFunc ?? (() => Resources.MatchesRegexMessage);

            return ValidationResult.Failures(
                new ValidationFailure(
                    context.Path,
                    errorFunc,
                    Severity,
                    ErrorCode,
                    AttemptedValue: value,
                    AdditionalData: new Dictionary<string, object?>
                    {
                        { "Pattern", Pattern.ToString() },
                    }
                )
            );
        }

        return ValidationResult.Success(value);
    }
}
