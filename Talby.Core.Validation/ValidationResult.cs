using System.Collections.Immutable;

namespace Talby.Core.Validation;

public sealed class ValidationResult
{
    public static ValidationResult Success(
        object? value,
        params ReadOnlySpan<ValidationFailure> errors
    ) => new(value, errors);

    public static ValidationResult Success(
        object? value,
        params IEnumerable<ValidationFailure>? errors
    ) => new(value, errors);

    public static readonly ValidationResult Valid = Success(null);

    public static ValidationResult Failures(params ReadOnlySpan<ValidationFailure> errors) =>
        new(null, errors);

    public static ValidationResult Failures(params IEnumerable<ValidationFailure>? errors) =>
        new(null, errors);

    public ValidationResult(object? resultValue, params ReadOnlySpan<ValidationFailure> errors)
    {
        ResultValue = resultValue;
        Errors = errors.ToImmutableArray();
        lazySeverity = new(() => ComputeSeverity(Errors));
    }

    public ValidationResult(object? resultValue, params IEnumerable<ValidationFailure>? errors)
    {
        ResultValue = resultValue;
        Errors = errors?.ToImmutableArray() ?? [];
        lazySeverity = new(() => ComputeSeverity(Errors));
    }

    public object? ResultValue { get; }
    public IReadOnlyCollection<ValidationFailure> Errors { get; }

    private Lazy<ValidationSeverity> lazySeverity;

    public ValidationSeverity Severity => lazySeverity.Value;
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
