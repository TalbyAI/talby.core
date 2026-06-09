namespace Talby.Core.Validation;

public interface IValidator
{
    ValueTask<ValidationResult> ValidateAsync(
        IValidationContext context,
        CancellationToken cancel = default
    );
}

internal static class Validator
{
    public static async Task<ValidationResult> PipeSequentialValidators(
        this IValidationContext context,
        IEnumerable<IValidator> validators,
        CancellationToken cancel = default
    )
    {
        ArgumentNullException.ThrowIfNull(context);

        var list = new List<ValidationFailure>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(context, cancel).ConfigureAwait(false);

            list.AddRange(result.Errors);
            if (!result.IsValid)
            {
                return ValidationResult.Failures(list);
            }

            context = context.WithTarget(result.ResultValue);
        }

        return ValidationResult.Success(context.ValidationTarget, list);
    }
}
