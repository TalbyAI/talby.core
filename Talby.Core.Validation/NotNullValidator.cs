namespace Talby.Core.Validation;

public sealed class NotNullValidator : ValueValidator
{
    public const string ErrorCode = "NotNull";

    public static readonly NotNullValidator Instance = new();

    protected override ValidationResult Validate(IValidationContext context)
    {
        if (context.ValidationTarget is { } value)
        {
            return ValidationResult.Success(value);
        }

        return ValidationResult.Failures(
            new ValidationFailure(
                context.Path,
                () => Resources.NotNullValidatorMessage,
                Severity,
                ErrorCode,
                AttemptedValue: context.ValidationTarget
            )
        );
    }
}
