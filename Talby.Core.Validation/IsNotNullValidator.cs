namespace Talby.Core.Validation;

public sealed class IsNotNullValidator : ValueValidator
{
    public const string ErrorCode = "IsNotNull";

    public static readonly IsNotNullValidator Instance = new();

    protected override ValidationResult Validate(IValidationContext context)
    {
        if (context.ValidationTarget is { } value)
        {
            return ValidationResult.Success(value);
        }

        return ValidationResult.Failures(
            new ValidationFailure(
                context.Path,
                () => Resources.IsNotNullValidatorMessage,
                Severity,
                ErrorCode,
                AttemptedValue: context.ValidationTarget
            )
        );
    }
}
