namespace Talby.Core.Validation;

/// <summary>
/// Validates a value asynchronously.
/// </summary>
public interface IValidator
{
    /// <summary>
    /// Validates the supplied context.
    /// </summary>
    /// <param name="context">The context to validate.</param>
    /// <param name="cancel">A token that can be used to cancel validation.</param>
    /// <returns>The validation result.</returns>
    ValueTask<ValidationResult> ValidateAsync(
        IValidationContext context,
        CancellationToken cancel = default
    );
}

/// <summary>
/// Runs validators in sequence and threads successful results into the next validator.
/// </summary>
internal static class Validator
{
    /// <summary>
    /// Runs the supplied validators in sequence.
    /// </summary>
    /// <param name="context">The initial validation context.</param>
    /// <param name="validators">The validators to run.</param>
    /// <param name="cancel">A token that can be used to cancel validation.</param>
    /// <returns>The combined validation result.</returns>
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
