using System.Diagnostics.CodeAnalysis;

namespace Talby.Core.Validation;

public sealed class TrimValidator : ValueValidator
{
    public static readonly TrimValidator Instance = new();

    private static readonly IEnumerable<IValidator> previousValidators =
    [
        IsOfTypeValidator<string>.Instance,
    ];

    public TrimValidator(params IEnumerable<char>? charsToTrim)
    {
        this.charsToTrim = charsToTrim is null ? null : charsToTrim.ToArray();
    }

    private readonly char[]? charsToTrim;

    protected override bool TryValidate(
        IValidationContext context,
        out object? validatedValue,
        [NotNullWhen(false)] out ValidationFailure? failure
    )
    {
        if (context.ValidationTarget is string value)
        {
            validatedValue = charsToTrim is null ? value.Trim() : value.Trim(charsToTrim);
            failure = null;
            return true;
        }

        validatedValue = null;
        failure = null;
        return true;
    }

    protected override IEnumerable<IValidator> PreviousValidators => previousValidators;
}
