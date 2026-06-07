using System.Diagnostics.CodeAnalysis;

namespace Talby.Core.Validation;

public abstract class ValueValidator : IValidator
{
    public async ValueTask<ValidationResult> ValidateAsync(
        IValidationContext context,
        CancellationToken cancel = default
    )
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var previous in PreviousValidators)
        {
            var previousResult = await previous
                .ValidateAsync(context, cancel)
                .ConfigureAwait(false);

            if (!previousResult.IsValid)
            {
                return previousResult;
            }

            context = context.WithTarget(previousResult.ResultValue);
        }

        if (TryValidate(context, out var value, out var failure))
        {
            return ValidationResult.Success(value);
        }

        return ValidationResult.Failures([failure]);
    }

    protected abstract bool TryValidate(
        IValidationContext context,
        out object? validatedValue,
        [NotNullWhen(false)] out ValidationFailure? failure
    );

    protected virtual IEnumerable<IValidator> PreviousValidators => Array.Empty<IValidator>();

    public ValidationSeverity Severity { get; init; } = ValidationSeverity.Error;
}
