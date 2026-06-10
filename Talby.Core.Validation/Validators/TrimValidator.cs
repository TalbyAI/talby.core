namespace Talby.Core.Validation.Validators;

/// <summary>
/// Trims leading and trailing characters from string values.
/// </summary>
public sealed class TrimValidator : ValueValidator<string>
{
    /// <summary>
    /// Gets the shared validator instance.
    /// </summary>
    public static readonly TrimValidator Instance = new();

    /// <summary>
    /// Creates a validator that trims the supplied characters.
    /// </summary>
    /// <param name="charsToTrim">The characters to trim from the value.</param>
    public TrimValidator(params IEnumerable<char>? charsToTrim)
    {
        if (charsToTrim?.ToArray() is var arr && arr is null or { Length: 0 })
        {
            this.charsToTrim = null;
        }
        else
        {
            this.charsToTrim = arr;
        }
    }

    private readonly char[]? charsToTrim;

    /// <summary>
    /// Trims the configured characters from the supplied string.
    /// </summary>
    /// <param name="context">The validation context.</param>
    /// <param name="value">The string to trim.</param>
    /// <returns>The validation result.</returns>
    protected override ValidationResult Validate(IValidationContext context, string value)
    {
        value = charsToTrim is null ? value.Trim() : value.Trim(charsToTrim);

        return ValidationResult.Success(value);
    }
}
