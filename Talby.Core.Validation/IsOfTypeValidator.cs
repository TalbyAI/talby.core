namespace Talby.Core.Validation;

public sealed class IsOfTypeValidator<T> : ValueValidator
{
    public const string ErrorCode = "IsOfType";

    public static readonly IsOfTypeValidator<T> Instance = new();

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
