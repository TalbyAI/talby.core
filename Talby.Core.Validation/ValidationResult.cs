using System.Collections.Immutable;

namespace Talby.Core.Validation;

/// <summary>
/// Represents the outcome of a validation operation.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// Creates a successful validation result for the supplied value.
    /// </summary>
    /// <param name="value">The validated value.</param>
    /// <param name="errors">Any validation failures to carry with the result.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult Success(
        object? value,
        params ReadOnlySpan<ValidationFailure> errors
    ) => new(value, errors);

    /// <summary>
    /// Creates a successful validation result for the supplied value.
    /// </summary>
    /// <param name="value">The validated value.</param>
    /// <param name="errors">Any validation failures to carry with the result.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult Success(
        object? value,
        params IEnumerable<ValidationFailure>? errors
    ) => new(value, errors);

    /// <summary>
    /// Gets a reusable successful validation result with no value.
    /// </summary>
    public static readonly ValidationResult Valid = Success(null);

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="errors">The validation failures.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult Failures(params ReadOnlySpan<ValidationFailure> errors) =>
        new(null, errors);

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="errors">The validation failures.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult Failures(params IEnumerable<ValidationFailure>? errors) =>
        new(null, errors);

    /// <summary>
    /// Creates a validation result.
    /// </summary>
    /// <param name="resultValue">The value produced by validation.</param>
    /// <param name="errors">Any validation failures to include.</param>
    public ValidationResult(object? resultValue, params ReadOnlySpan<ValidationFailure> errors)
    {
        ResultValue = resultValue;
        Errors = errors.ToImmutableArray();
        lazySeverity = new(() => ComputeSeverity(Errors));
    }

    /// <summary>
    /// Creates a validation result.
    /// </summary>
    /// <param name="resultValue">The value produced by validation.</param>
    /// <param name="errors">Any validation failures to include.</param>
    public ValidationResult(object? resultValue, params IEnumerable<ValidationFailure>? errors)
    {
        ResultValue = resultValue;
        Errors = errors?.ToImmutableArray() ?? [];
        lazySeverity = new(() => ComputeSeverity(Errors));
    }

    /// <summary>
    /// Gets the value produced by validation.
    /// </summary>
    public object? ResultValue { get; }
    /// <summary>
    /// Gets the validation failures.
    /// </summary>
    public IReadOnlyCollection<ValidationFailure> Errors { get; }

    private Lazy<ValidationSeverity> lazySeverity;

    /// <summary>
    /// Gets the highest severity across all validation failures.
    /// </summary>
    public ValidationSeverity Severity => lazySeverity.Value;
    /// <summary>
    /// Gets a value indicating whether validation succeeded.
    /// </summary>
    public bool IsValid => Severity == ValidationSeverity.Info;

    private static ValidationSeverity ComputeSeverity(IReadOnlyCollection<ValidationFailure> errors)
    {
        if (errors.Count == 0)
        {
            return ValidationSeverity.Info;
        }

        return errors.Max(e => e.Severity);
    }
}
