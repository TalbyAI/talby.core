using System.Diagnostics;

namespace Talby.Core.Validation;

/// <summary>
/// Provides synchronous validation with optional prerequisite validators.
/// </summary>
public abstract class ValueValidator : IValidator
{
    /// <summary>
    /// Gets or sets the severity reported when validation fails.
    /// </summary>
    public ValidationSeverity Severity { get; init; } = ValidationSeverity.Error;

    /// <summary>
    /// Validates the supplied context.
    /// </summary>
    /// <param name="context">The validation context to validate.</param>
    /// <returns>The validation result.</returns>
    protected abstract ValidationResult Validate(IValidationContext context);

    /// <summary>
    /// Gets the validators that must succeed before this validator runs.
    /// </summary>
    protected virtual IEnumerable<IValidator> PreviousValidators => Array.Empty<IValidator>();

    /// <summary>
    /// Validates the supplied context asynchronously.
    /// </summary>
    /// <param name="context">The validation context to validate.</param>
    /// <param name="cancel">A token that can be used to cancel validation.</param>
    /// <returns>The validation result.</returns>
    public async ValueTask<ValidationResult> ValidateAsync(
        IValidationContext context,
        CancellationToken cancel
    )
    {
        ArgumentNullException.ThrowIfNull(context);

        var previousResult = await context
            .PipeSequentialValidators(PreviousValidators, cancel)
            .ConfigureAwait(false);

        if (!previousResult.IsValid)
        {
            return previousResult;
        }

        context = context.WithTarget(previousResult.ResultValue);

        var actualResult = Validate(context);

        if (previousResult.Errors.Count > 0)
        {
            var combinedErrors = previousResult.Errors.Concat(actualResult.Errors);
            return ValidationResult.Success(actualResult.ResultValue, combinedErrors);
        }

        return actualResult;
    }
}

/// <summary>
/// Provides asynchronous validation with optional prerequisite validators.
/// </summary>
public abstract class AsyncValueValidator : IValidator
{
    /// <summary>
    /// Gets or sets the severity reported when validation fails.
    /// </summary>
    public ValidationSeverity Severity { get; init; } = ValidationSeverity.Error;

    /// <summary>
    /// Validates the supplied context.
    /// </summary>
    /// <param name="context">The validation context to validate.</param>
    /// <param name="cancel">A token that can be used to cancel validation.</param>
    /// <returns>The validation result.</returns>
    protected abstract ValueTask<ValidationResult> Validate(
        IValidationContext context,
        CancellationToken cancel
    );

    /// <summary>
    /// Gets the validators that must succeed before this validator runs.
    /// </summary>
    protected virtual IEnumerable<IValidator> PreviousValidators => Array.Empty<IValidator>();

    /// <summary>
    /// Validates the supplied context asynchronously.
    /// </summary>
    /// <param name="context">The validation context to validate.</param>
    /// <param name="cancel">A token that can be used to cancel validation.</param>
    /// <returns>The validation result.</returns>
    public async ValueTask<ValidationResult> ValidateAsync(
        IValidationContext context,
        CancellationToken cancel
    )
    {
        ArgumentNullException.ThrowIfNull(context);

        var previousResult = await context
            .PipeSequentialValidators(PreviousValidators, cancel)
            .ConfigureAwait(false);

        if (!previousResult.IsValid)
        {
            return previousResult;
        }

        context = context.WithTarget(previousResult.ResultValue);

        var actualResult = await Validate(context, cancel).ConfigureAwait(false);

        if (previousResult.Errors.Count > 0)
        {
            var combinedErrors = previousResult.Errors.Concat(actualResult.Errors);
            return ValidationResult.Success(actualResult.ResultValue, combinedErrors);
        }

        return actualResult;
    }
}

/// <summary>
/// Provides synchronous validation for values of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The validated value type.</typeparam>
public abstract class ValueValidator<T> : ValueValidator
{
    private static readonly IEnumerable<IValidator> previousValidators =
    [
        IsOfTypeValidator<T>.Instance,
    ];

    /// <summary>
    /// Gets the validators that must succeed before this validator runs.
    /// </summary>
    protected override IEnumerable<IValidator> PreviousValidators => previousValidators;

    /// <summary>
    /// Validates a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="context">The validation context to validate.</param>
    /// <returns>The validation result.</returns>
    protected sealed override ValidationResult Validate(IValidationContext context)
    {
        if (context.ValidationTarget is T value)
        {
            return Validate(context, value);
        }
        else if (context.ValidationTarget is null)
        {
            return ValidateNull(context);
        }
        else
        {
            // This should never happen due to the IsOfTypeValidator<T> in PreviousValidators
            throw new UnreachableException(
                $"Expected validation target to be of type {typeof(T)}, but got {context.ValidationTarget.GetType()}."
            );
        }
    }

    /// <summary>
    /// Validates a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="context">The validation context to validate.</param>
    /// <param name="value">The validated value.</param>
    /// <returns>The validation result.</returns>
    protected abstract ValidationResult Validate(IValidationContext context, T value);

    /// <summary>
    /// Validates a null value.
    /// </summary>
    /// <param name="context">The validation context to validate.</param>
    /// <returns>The validation result.</returns>
    protected virtual ValidationResult ValidateNull(IValidationContext context)
    {
        return ValidationResult.Success(context.ValidationTarget);
    }
}

/// <summary>
/// Provides asynchronous validation for values of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The validated value type.</typeparam>
public abstract class AsyncValueValidator<T> : AsyncValueValidator
{
    private static readonly IEnumerable<IValidator> previousValidators =
    [
        IsOfTypeValidator<T>.Instance,
    ];

    /// <summary>
    /// Gets the validators that must succeed before this validator runs.
    /// </summary>
    protected override IEnumerable<IValidator> PreviousValidators => previousValidators;

    /// <summary>
    /// Validates a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="context">The validation context to validate.</param>
    /// <param name="cancel">A token that can be used to cancel validation.</param>
    /// <returns>The validation result.</returns>
    protected sealed override ValueTask<ValidationResult> Validate(
        IValidationContext context,
        CancellationToken cancel
    )
    {
        if (context.ValidationTarget is T value)
        {
            return Validate(context, value, cancel);
        }
        else if (context.ValidationTarget is null)
        {
            return ValidateNull(context, cancel);
        }
        else
        {
            // This should never happen due to the IsOfTypeValidator<T> in PreviousValidators
            throw new UnreachableException(
                $"Expected validation target to be of type {typeof(T)}, but got {context.ValidationTarget.GetType()}."
            );
        }
    }

    /// <summary>
    /// Validates a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="context">The validation context to validate.</param>
    /// <param name="value">The validated value.</param>
    /// <param name="cancel">A token that can be used to cancel validation.</param>
    /// <returns>The validation result.</returns>
    protected abstract ValueTask<ValidationResult> Validate(
        IValidationContext context,
        T value,
        CancellationToken cancel
    );

    /// <summary>
    /// Validates a null value.
    /// </summary>
    /// <param name="context">The validation context to validate.</param>
    /// <param name="cancel">A token that can be used to cancel validation.</param>
    /// <returns>The validation result.</returns>
    protected virtual ValueTask<ValidationResult> ValidateNull(
        IValidationContext context,
        CancellationToken cancel
    )
    {
        return new ValueTask<ValidationResult>(ValidationResult.Success(context.ValidationTarget));
    }
}
