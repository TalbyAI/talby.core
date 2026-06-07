using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Talby.Core.Validation;

public sealed class MatchesRegexValidator : ValueValidator
{
    public const string ErrorCode = "MatchesRegex";

    private static readonly IEnumerable<IValidator> previousValidators =
    [
        IsOfTypeValidator<string>.Instance,
    ];

    public MatchesRegexValidator(Regex pattern)
    {
        Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
    }

    public Regex Pattern { get; }
    public Func<string>? ErrorMessageFunc { get; init; }

    protected override bool TryValidate(
        IValidationContext context,
        out object? validatedValue,
        [NotNullWhen(false)] out ValidationFailure? failure
    )
    {
        if (context.ValidationTarget is string value && !Pattern.IsMatch(value))
        {
            validatedValue = null;
            failure = new(
                context.Path,
                ErrorMessageFunc ?? (() => Resources.MatchesRegexMessage),
                Severity,
                ErrorCode,
                AttemptedValue: value,
                AdditionalData: new Dictionary<string, object?>
                {
                    { "Pattern", Pattern.ToString() },
                }
            );
            return false;
        }

        validatedValue = null;
        failure = null;
        return true;
    }

    protected override IEnumerable<IValidator> PreviousValidators => previousValidators;
}
