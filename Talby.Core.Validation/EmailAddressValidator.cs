using System.Net.Mail;

namespace Talby.Core.Validation;

public sealed class EmailAddressValidator : ValueValidator<string>
{
    public const string ErrorCode = "EmailAddress";

    public Func<string>? ErrorMessageFunc { get; init; }

    protected override ValidationResult Validate(IValidationContext context, string value)
    {
        if (!IsValidEmailAddress(value))
        {
            var errorFunc = ErrorMessageFunc ?? (() => Resources.EmailAddressValidatorMessage);

            return ValidationResult.Failures(
                new ValidationFailure(
                    context.Path,
                    errorFunc,
                    Severity,
                    ErrorCode,
                    AttemptedValue: value
                )
            );
        }

        return ValidationResult.Success(value);
    }

    private static bool IsValidEmailAddress(string value)
    {
        try
        {
            var address = new MailAddress(value);
            return string.Equals(address.Address, value, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
