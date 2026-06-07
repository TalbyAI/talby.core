using System.Diagnostics.CodeAnalysis;

namespace Talby.Core.Validation;

public sealed class IsNotNullValidator : ValueValidator
{
    public const string ErrorCode = "IsNotNull";

    public static readonly IsNotNullValidator Instance = new();

    protected override bool TryValidate(
        IValidationContext context,
        out object? validatedValue,
        [NotNullWhen(false)] out ValidationFailure? failure
    )
    {
        if (context.ValidationTarget is { } value)
        {
            validatedValue = value;
            failure = null;
            return true;
        }

        validatedValue = null;
        failure = new ValidationFailure(
            context.Path,
            () => Resources.IsNotNullValidatorMessage,
            Severity,
            ErrorCode,
            AttemptedValue: context.ValidationTarget
        );
        return false;
    }
}
