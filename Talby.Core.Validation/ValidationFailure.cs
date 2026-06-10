namespace Talby.Core.Validation;

/// <summary>
/// Describes the severity of a validation failure.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>
    /// Indicates informational output only.
    /// </summary>
    Info = 0,
    /// <summary>
    /// Indicates a warning.
    /// </summary>
    Warning = 1,
    /// <summary>
    /// Indicates an error.
    /// </summary>
    Error = 2,
}

/// <summary>
/// Represents a single validation failure.
/// </summary>
/// <param name="Path">The validation path that failed.</param>
/// <param name="ErrorMessageFunc">The factory used to create the error message.</param>
/// <param name="Severity">The severity reported for the failure.</param>
/// <param name="ErrorCode">The error code reported for the failure.</param>
/// <param name="AttemptedValue">The value that was being validated.</param>
/// <param name="AdditionalData">Additional data associated with the failure.</param>
/// <param name="Exception">The exception associated with the failure, if any.</param>
public sealed record ValidationFailure(
    ValidationPath Path,
    Func<string> ErrorMessageFunc,
    ValidationSeverity Severity = ValidationSeverity.Error,
    string? ErrorCode = null,
    object? AttemptedValue = null,
    IReadOnlyDictionary<string, object?>? AdditionalData = null,
    Exception? Exception = null
);
