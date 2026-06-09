using System.Diagnostics.CodeAnalysis;
using Talby.Core.Validation;

namespace Talby.Core.Validation.UnitTests;

public sealed class ValueValidatorTests
{
    // Scenario: Given a null context, When ValidateAsync runs, Then it throws ArgumentNullException.
    [Fact]
    public async Task ValueValidator_ValidateAsync_Throws_when_context_is_null()
    {
        var sut = new RecordingValueValidator(_ => ValidationResult.Valid);

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.ValidateAsync(null!, CancellationToken.None).AsTask()
        );

        Assert.Equal("context", exception.ParamName);
    }

    // Scenario: Given no previous validators and a successful Validate result, When ValidateAsync runs, Then it returns the validated value.
    [Fact]
    public async Task ValueValidator_ValidateAsync_Returns_success_when_validate_succeeds()
    {
        var inputTarget = new object();
        var expectedValue = new object();
        var sut = new RecordingValueValidator(_ => ValidationResult.Success(expectedValue));

        var result = await sut.ValidateAsync(
            new ValidationContext(inputTarget),
            CancellationToken.None
        );

        Assert.Equal(ValidationSeverity.Error, sut.Severity);
        Assert.True(result.IsValid);
        Assert.Same(expectedValue, result.ResultValue);
        Assert.Empty(result.Errors);
        Assert.Equal(1, sut.ValidateCallCount);
        Assert.Same(inputTarget, sut.LastValidateContext?.ValidationTarget);
    }

    // Scenario: Given no previous validators and a failing Validate result, When ValidateAsync runs, Then it returns the reported failure.
    [Fact]
    public async Task ValueValidator_ValidateAsync_Returns_failure_when_validate_fails()
    {
        var failure = CreateFailure("value", ValidationSeverity.Warning, "value is not valid");
        var sut = new RecordingValueValidator(_ => ValidationResult.Failures(failure))
        {
            Severity = ValidationSeverity.Warning,
        };

        var result = await sut.ValidateAsync(
            new ValidationContext(new object()),
            CancellationToken.None
        );

        Assert.Equal(ValidationSeverity.Warning, sut.Severity);
        Assert.False(result.IsValid);
        Assert.Equal(ValidationSeverity.Warning, result.Severity);
        Assert.Single(result.Errors);
        Assert.Same(failure, result.Errors.Single());
        Assert.Equal(1, sut.ValidateCallCount);
    }

    // Scenario: Given previous validators succeed, When ValidateAsync runs, Then each previous result becomes the next context target.
    [Fact]
    public async Task ValueValidator_ValidateAsync_Uses_previous_validator_results_before_try_validate()
    {
        var initialTarget = new object();
        var firstValidatedValue = new object();
        var finalValidatedValue = new object();
        var firstPreviousResult = ValidationResult.Success(firstValidatedValue);
        var secondPreviousResult = ValidationResult.Success(finalValidatedValue);
        var firstPreviousValidator = new RecordingValidator(firstPreviousResult);
        var secondPreviousValidator = new RecordingValidator(secondPreviousResult);
        var sut = new RecordingValueValidatorWithPreviousValidators(
            [firstPreviousValidator, secondPreviousValidator],
            context => ValidationResult.Success(context.ValidationTarget)
        );

        var result = await sut.ValidateAsync(
            new ValidationContext(initialTarget),
            CancellationToken.None
        );

        Assert.True(result.IsValid);
        Assert.Same(finalValidatedValue, result.ResultValue);
        Assert.Equal(1, firstPreviousValidator.CallCount);
        Assert.Equal(1, secondPreviousValidator.CallCount);
        Assert.Same(initialTarget, firstPreviousValidator.LastContext?.ValidationTarget);
        Assert.Same(firstValidatedValue, secondPreviousValidator.LastContext?.ValidationTarget);
        Assert.Same(finalValidatedValue, sut.LastValidateContext?.ValidationTarget);
        Assert.Equal(1, sut.ValidateCallCount);
    }

    // Scenario: Given a previous validator fails, When ValidateAsync runs, Then it returns that failure without calling Validate or later validators.
    [Fact]
    public async Task ValueValidator_ValidateAsync_Returns_first_previous_failure_without_continuing()
    {
        var failure = CreateFailure(
            "value",
            ValidationSeverity.Error,
            "previous validation failed"
        );
        var failingPreviousValidator = new RecordingValidator(ValidationResult.Failures([failure]));
        var laterPreviousValidator = new RecordingValidator(ValidationResult.Success(new object()));
        var sut = new RecordingValueValidatorWithPreviousValidators(
            [failingPreviousValidator, laterPreviousValidator],
            _ => ValidationResult.Valid
        );

        var result = await sut.ValidateAsync(
            new ValidationContext(new object()),
            CancellationToken.None
        );

        Assert.False(result.IsValid);
        Assert.Same(failure, result.Errors.Single());
        Assert.Equal(1, failingPreviousValidator.CallCount);
        Assert.Equal(0, laterPreviousValidator.CallCount);
        Assert.Equal(0, sut.ValidateCallCount);
    }

    private static ValidationFailure CreateFailure(
        string propertyName,
        ValidationSeverity severity,
        string message
    )
    {
        return new ValidationFailure(
            ValidationPath.Root.ForProperty(propertyName),
            () => message,
            severity
        );
    }

    private sealed class RecordingValidator(ValidationResult result) : IValidator
    {
        public int CallCount { get; private set; }

        public IValidationContext? LastContext { get; private set; }

        public ValueTask<ValidationResult> ValidateAsync(
            IValidationContext context,
            CancellationToken cancel = default
        )
        {
            CallCount++;
            LastContext = context;
            return ValueTask.FromResult(result);
        }
    }

    private class RecordingValueValidator(Func<IValidationContext, ValidationResult> behavior)
        : ValueValidator
    {
        public int ValidateCallCount { get; private set; }

        public IValidationContext? LastValidateContext { get; private set; }

        protected override ValidationResult Validate(IValidationContext context)
        {
            ValidateCallCount++;
            LastValidateContext = context;
            return behavior(context);
        }
    }

    private sealed class RecordingValueValidatorWithPreviousValidators(
        IReadOnlyList<IValidator> previousValidators,
        Func<IValidationContext, ValidationResult> behavior
    ) : RecordingValueValidator(behavior)
    {
        protected override IEnumerable<IValidator> PreviousValidators => previousValidators;
    }
}
