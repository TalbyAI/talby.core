namespace Talby.Core.Validation;

public sealed class TrimValidator : ValueValidator<string>
{
    public static readonly TrimValidator Instance = new();

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

    protected override ValidationResult Validate(IValidationContext context, string value)
    {
        value = charsToTrim is null ? value.Trim() : value.Trim(charsToTrim);

        return ValidationResult.Success(value);
    }
}
