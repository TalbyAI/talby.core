using System.Net.Mail;

namespace Talby.Core.Validation;

/// <summary>
/// Validates that a string is a valid email address.
/// </summary>
public sealed class EmailAddressValidator : ValueValidator<string>
{
    /// <summary>
    /// Gets the error code reported for invalid email addresses.
    /// </summary>
    public const string ErrorCode = "EmailAddress";

    /// <summary>
    /// Gets or sets the custom error message factory.
    /// </summary>
    public Func<string>? ErrorMessageFunc { get; init; }

    /// <summary>
    /// Validates the supplied email address.
    /// </summary>
    /// <param name="context">The validation context.</param>
    /// <param name="value">The email address to validate.</param>
    /// <returns>The validation result.</returns>
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
