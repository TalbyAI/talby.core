using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Talby.Core.SourceGenerators;

namespace Talby.Core.Validation;

public abstract class ValueValidator : IValidator
{
    public ValidationSeverity Severity { get; init; } = ValidationSeverity.Error;

    protected abstract ValidationResult Validate(IValidationContext context);

    protected virtual IEnumerable<IValidator> PreviousValidators => Array.Empty<IValidator>();

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

public abstract class AsyncValueValidator : IValidator
{
    public ValidationSeverity Severity { get; init; } = ValidationSeverity.Error;

    protected abstract ValueTask<ValidationResult> Validate(
        IValidationContext context,
        CancellationToken cancel
    );

    protected virtual IEnumerable<IValidator> PreviousValidators => Array.Empty<IValidator>();

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

public abstract class ValueValidator<T> : ValueValidator
{
    private static readonly IEnumerable<IValidator> previousValidators =
    [
        IsOfTypeValidator<T>.Instance,
    ];

    protected override IEnumerable<IValidator> PreviousValidators => previousValidators;

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

    protected abstract ValidationResult Validate(IValidationContext context, T value);

    protected virtual ValidationResult ValidateNull(IValidationContext context)
    {
        return ValidationResult.Success(context.ValidationTarget);
    }
}

public abstract class AsyncValueValidator<T> : AsyncValueValidator
{
    private static readonly IEnumerable<IValidator> previousValidators =
    [
        IsOfTypeValidator<T>.Instance,
    ];

    protected override IEnumerable<IValidator> PreviousValidators => previousValidators;

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

    protected abstract ValueTask<ValidationResult> Validate(
        IValidationContext context,
        T value,
        CancellationToken cancel
    );

    protected virtual ValueTask<ValidationResult> ValidateNull(
        IValidationContext context,
        CancellationToken cancel
    )
    {
        return new ValueTask<ValidationResult>(ValidationResult.Success(context.ValidationTarget));
    }
}
