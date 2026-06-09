using System.Collections;
using System.Collections.Generic;

namespace Talby.Core.Validation;

public sealed class NotEmptyValidator<T> : ValueValidator<T>
{
    public const string ErrorCode = "NotEmpty";

    public static readonly NotEmptyValidator<T> Instance = new();

    protected override ValidationResult Validate(IValidationContext context, T value)
    {
        if (IsEmpty(value))
        {
            return ValidationResult.Failures(CreateFailure(context.Path, value));
        }

        return ValidationResult.Success(value);
    }

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
