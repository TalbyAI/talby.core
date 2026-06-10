using System.Text.RegularExpressions;

namespace Talby.Core.Validation.Validators;

/// <summary>
/// Validates that a string matches a regular expression.
/// </summary>
public sealed class MatchesRegexValidator : ValueValidator<string>
{
    /// <summary>
    /// Gets the error code reported when the value does not match the pattern.
    /// </summary>
    public const string ErrorCode = "MatchesRegex";

    /// <summary>
    /// Creates a validator for the supplied regular expression.
    /// </summary>
    /// <param name="pattern">The regular expression to match.</param>
    public MatchesRegexValidator(Regex pattern)
    {
        Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
    }

    /// <summary>
    /// Gets the regular expression used for validation.
    /// </summary>
    public Regex Pattern { get; }

    /// <summary>
    /// Gets or sets the custom error message factory.
    /// </summary>
    public Func<string>? ErrorMessageFunc { get; init; }

    /// <summary>
    /// Validates the supplied string value.
    /// </summary>
    /// <param name="context">The validation context.</param>
    /// <param name="value">The string to validate.</param>
    /// <returns>The validation result.</returns>
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
