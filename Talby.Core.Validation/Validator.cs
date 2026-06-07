namespace Talby.Core.Validation;

public interface IValidator
{
    ValueTask<ValidationResult> ValidateAsync(
        IValidationContext context,
        CancellationToken cancel = default
    );
}
