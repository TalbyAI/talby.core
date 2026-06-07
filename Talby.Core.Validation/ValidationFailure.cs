namespace Talby.Core.Validation;

public enum ValidationSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
}

public sealed record ValidationFailure(
    ValidationPath Path,
    Func<string> ErrorMessageFunc,
    ValidationSeverity Severity = ValidationSeverity.Error,
    string? ErrorCode = null,
    object? AttemptedValue = null,
    IReadOnlyDictionary<string, object?>? AdditionalData = null,
    Exception? Exception = null
);
