namespace Talby.Core.Validation;

/// <summary>
/// Validates that a value is not null.
/// </summary>
public sealed class NotNullValidator : ValueValidator
{
    /// <summary>
    /// Gets the error code reported when the value is null.
    /// </summary>
    public const string ErrorCode = "NotNull";

    /// <summary>
    /// Gets the shared validator instance.
    /// </summary>
    public static readonly NotNullValidator Instance = new();

    /// <summary>
    /// Validates the supplied context.
    /// </summary>
    /// <param name="context">The validation context.</param>
    /// <returns>The validation result.</returns>
    protected override ValidationResult Validate(IValidationContext context)
    {
        if (context.ValidationTarget is { } value)
        {
            return ValidationResult.Success(value);
        }

        return ValidationResult.Failures(
            new ValidationFailure(
                context.Path,
                () => Resources.NotNullValidatorMessage,
                Severity,
                ErrorCode,
                AttemptedValue: context.ValidationTarget
            )
        );
    }
}
