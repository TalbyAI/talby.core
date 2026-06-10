using System.Collections;

namespace Talby.Core.Validation.Validators;

/// <summary>
/// Validates that a value is not empty.
/// </summary>
/// <typeparam name="T">The validated value type.</typeparam>
public sealed class NotEmptyValidator<T> : ValueValidator<T>
{
    /// <summary>
    /// Gets the error code reported when the value is empty.
    /// </summary>
    public const string ErrorCode = "NotEmpty";

    /// <summary>
    /// Gets the shared validator instance.
    /// </summary>
    public static readonly NotEmptyValidator<T> Instance = new();

    /// <summary>
    /// Validates the supplied value.
    /// </summary>
    /// <param name="context">The validation context.</param>
    /// <param name="value">The value to validate.</param>
    /// <returns>The validation result.</returns>
    protected override ValidationResult Validate(IValidationContext context, T value)
    {
        if (IsEmpty(value))
        {
            return ValidationResult.Failures(CreateFailure(context.Path, value));
        }

        return ValidationResult.Success(value);
    }

    /// <summary>
    /// Validates a null value.
    /// </summary>
    /// <param name="context">The validation context.</param>
    /// <returns>The validation result.</returns>
    protected override ValidationResult ValidateNull(IValidationContext context)
    {
        return ValidationResult.Failures(CreateFailure(context.Path, context.ValidationTarget));
    }

    private ValidationFailure CreateFailure(ValidationPath path, object? attemptedValue)
    {
        return new ValidationFailure(
            path,
            () => Resources.NotEmptyValidatorMessage,
            Severity,
            ErrorCode,
            AttemptedValue: attemptedValue
        );
    }

    private static bool IsEmpty(T value)
    {
        if (value is string stringValue)
        {
            return stringValue.Length == 0;
        }

        if (value is ICollection collection)
        {
            return collection.Count == 0;
        }

        return EqualityComparer<T>.Default.Equals(value, default!);
    }
}
