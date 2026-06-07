using System.Diagnostics.CodeAnalysis;

namespace Talby.Core.Validation;

public sealed class IsOfTypeValidator<T> : ValueValidator
{
    public const string ErrorCode = "IsOfType";

    public static readonly IsOfTypeValidator<T> Instance = new();

    protected override bool TryValidate(
        IValidationContext context,
        out object? validatedValue,
        [NotNullWhen(false)] out ValidationFailure? failure
    )
    {
        switch (context.ValidationTarget)
        {
            // If the value is of type T, validation succeeds
            case T value:
                validatedValue = value;
                failure = null;
                return true;

            // Allow null values to be validated as long as T is a reference type or nullable value type
            case null when default(T) is null:
                validatedValue = null;
                failure = null;
                return true;

            // For any other type, validation fails
            default:
                validatedValue = null;
                failure = new ValidationFailure(
                    context.Path,
                    () => string.Format(Resources.IsOfTypeValidatorMessageFormat, typeof(T).Name),
                    Severity,
                    ErrorCode,
                    AttemptedValue: context.ValidationTarget
                );
                return false;
        }
    }
}
