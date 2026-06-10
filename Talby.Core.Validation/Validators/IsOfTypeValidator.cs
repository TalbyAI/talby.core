namespace Talby.Core.Validation.Validators;

/// <summary>
/// Validates that the target is of the expected type.
/// </summary>
/// <typeparam name="T">The expected target type.</typeparam>
public sealed class IsOfTypeValidator<T> : ValueValidator
{
    /// <summary>
    /// Gets the error code reported when the value is not of the expected type.
    /// </summary>
    public const string ErrorCode = "IsOfType";

    /// <summary>
    /// Gets the shared validator instance.
    /// </summary>
    public static readonly IsOfTypeValidator<T> Instance = new();

    /// <summary>
    /// Validates the supplied context.
    /// </summary>
    /// <param name="context">The validation context.</param>
    /// <returns>The validation result.</returns>
    protected override ValidationResult Validate(IValidationContext context)
    {
        switch (context.ValidationTarget)
        {
            case T value:
                return ValidationResult.Success(value);

            case null when default(T) is null:
                return ValidationResult.Success(null);

            default:
                return ValidationResult.Failures(
                    new ValidationFailure(
                        context.Path,
                        () =>
                            string.Format(Resources.IsOfTypeValidatorMessageFormat, typeof(T).Name),
                        Severity,
                        ErrorCode,
                        AttemptedValue: context.ValidationTarget
                    )
                );
        }
    }
}
