using System.Diagnostics;

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

    // Scenario: Given a non-generic async value validator with no previous validators, When ValidateAsync runs, Then it succeeds and returns the validated value.
    [Fact]
    public async Task AsyncValueValidator_ValidateAsync_Returns_success_when_validate_succeeds()
    {
        var inputTarget = new object();
        var expectedValue = new object();
        var sut = new RecordingAsyncValueValidator(_ =>
            ValueTask.FromResult(ValidationResult.Success(expectedValue))
        );

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

    // Scenario: Given a generic value validator and a matching string target, When ValidateAsync runs, Then it succeeds and returns the string.
    [Fact]
    public async Task ValueValidatorOfString_ValidateAsync_Returns_success_for_matching_string_targets()
    {
        var target = "hello";
        var sut = new RecordingTypedValueValidator<string>(
            (_, value) => ValidationResult.Success(value)
        );

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Same(target, result.ResultValue);
        Assert.Empty(result.Errors);
        Assert.Equal(1, sut.ValidateCallCount);
        Assert.Same(target, sut.LastValidateContext?.ValidationTarget);
    }

    // Scenario: Given a generic value validator and a null string target, When ValidateAsync runs, Then it succeeds without calling Validate.
    [Fact]
    public async Task ValueValidatorOfString_ValidateAsync_Returns_success_for_null_string_targets()
    {
        var sut = new RecordingTypedValueValidator<string>((_, _) => ValidationResult.Valid);

        var result = await sut.ValidateAsync(new ValidationContext(null), CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Empty(result.Errors);
        Assert.Equal(0, sut.ValidateCallCount);
    }

    // Scenario: Given a generic value validator whose previous validators add info-level warnings, When ValidateAsync runs, Then it keeps the warnings and returns the validated value.
    [Fact]
    public async Task ValueValidatorOfString_ValidateAsync_Combines_previous_info_errors_with_validate_output()
    {
        var expectedValue = "validated";
        var warning = CreateFailure(
            "value",
            ValidationSeverity.Info,
            "previous validator emitted a note"
        );
        var previousValidator = new RecordingValidator(
            ValidationResult.Success(expectedValue, [warning])
        );
        var sut = new RecordingTypedValueValidator<string>(
            (_, value) => ValidationResult.Success(value),
            [previousValidator]
        );

        var result = await sut.ValidateAsync(
            new ValidationContext(new object()),
            CancellationToken.None
        );

        Assert.True(result.IsValid);
        Assert.Same(expectedValue, result.ResultValue);
        Assert.Collection(result.Errors, actualWarning => Assert.Same(warning, actualWarning));
        Assert.Equal(1, previousValidator.CallCount);
        Assert.Equal(1, sut.ValidateCallCount);
    }

    // Scenario: Given a generic value validator using its default previous validators, When ValidateAsync runs, Then it succeeds through the built-in type guard.
    [Fact]
    public async Task ValueValidatorOfString_ValidateAsync_Uses_default_previous_validators()
    {
        var target = "hello";
        var sut = new DefaultPreviousTypedValueValidator<string>(
            (_, value) => ValidationResult.Success(value)
        );

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Same(target, result.ResultValue);
        Assert.Empty(result.Errors);
        Assert.Equal(1, sut.ValidateCallCount);
        Assert.Same(target, sut.LastValidateContext?.ValidationTarget);
    }

    // Scenario: Given a generic value validator and a mismatched target type, When Validate is invoked directly, Then it throws UnreachableException.
    [Fact]
    public void ValueValidatorOfString_InvokeValidate_Throws_when_target_has_wrong_type()
    {
        var sut = new RecordingTypedValueValidator<string>((_, _) => ValidationResult.Valid);

        var exception = Assert.Throws<UnreachableException>(() =>
            sut.InvokeValidate(new ValidationContext(new object()))
        );

        Assert.Contains(typeof(string).Name, exception.Message);
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

    // Scenario: Given a generic async value validator and a matching string target, When ValidateAsync runs, Then it succeeds and returns the string.
    [Fact]
    public async Task AsyncValueValidatorOfString_ValidateAsync_Returns_success_for_matching_string_targets()
    {
        var target = "hello";
        var sut = new RecordingTypedAsyncValueValidator<string>(
            (_, value) => ValueTask.FromResult(ValidationResult.Success(value))
        );

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Same(target, result.ResultValue);
        Assert.Empty(result.Errors);
        Assert.Equal(1, sut.ValidateCallCount);
        Assert.Same(target, sut.LastValidateContext?.ValidationTarget);
    }

    // Scenario: Given a generic async value validator and a null string target, When ValidateAsync runs, Then it succeeds without calling Validate.
    [Fact]
    public async Task AsyncValueValidatorOfString_ValidateAsync_Returns_success_for_null_string_targets()
    {
        var sut = new RecordingTypedAsyncValueValidator<string>(
            (_, _) => ValueTask.FromResult(ValidationResult.Valid)
        );

        var result = await sut.ValidateAsync(new ValidationContext(null), CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Empty(result.Errors);
        Assert.Equal(0, sut.ValidateCallCount);
    }

    // Scenario: Given a generic async value validator whose previous validators add info-level warnings, When ValidateAsync runs, Then it keeps the warnings and returns the validated value.
    [Fact]
    public async Task AsyncValueValidatorOfString_ValidateAsync_Combines_previous_info_errors_with_validate_output()
    {
        var expectedValue = "validated";
        var warning = CreateFailure(
            "value",
            ValidationSeverity.Info,
            "previous validator emitted a note"
        );
        var previousValidator = new RecordingValidator(
            ValidationResult.Success(expectedValue, [warning])
        );
        var sut = new RecordingTypedAsyncValueValidator<string>(
            (_, value) => ValueTask.FromResult(ValidationResult.Success(value)),
            [previousValidator]
        );

        var result = await sut.ValidateAsync(
            new ValidationContext(new object()),
            CancellationToken.None
        );

        Assert.True(result.IsValid);
        Assert.Same(expectedValue, result.ResultValue);
        Assert.Collection(result.Errors, actualWarning => Assert.Same(warning, actualWarning));
        Assert.Equal(1, previousValidator.CallCount);
        Assert.Equal(1, sut.ValidateCallCount);
    }

    // Scenario: Given a previous async validator fails, When ValidateAsync runs, Then it returns that failure without calling Validate or later validators.
    [Fact]
    public async Task AsyncValueValidatorOfString_ValidateAsync_Returns_first_previous_failure_without_continuing()
    {
        var failure = CreateFailure(
            "value",
            ValidationSeverity.Error,
            "previous validation failed"
        );
        var failingPreviousValidator = new RecordingValidator(ValidationResult.Failures([failure]));
        var laterPreviousValidator = new RecordingValidator(ValidationResult.Success(new object()));
        var sut = new RecordingTypedAsyncValueValidator<string>(
            (_, _) => ValueTask.FromResult(ValidationResult.Valid),
            [failingPreviousValidator, laterPreviousValidator]
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

    // Scenario: Given a generic async value validator using its default previous validators, When ValidateAsync runs, Then it succeeds through the built-in type guard.
    [Fact]
    public async Task AsyncValueValidatorOfString_ValidateAsync_Uses_default_previous_validators()
    {
        var target = "hello";
        var sut = new DefaultPreviousTypedAsyncValueValidator<string>(
            (_, value) => ValueTask.FromResult(ValidationResult.Success(value))
        );

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Same(target, result.ResultValue);
        Assert.Empty(result.Errors);
        Assert.Equal(1, sut.ValidateCallCount);
        Assert.Same(target, sut.LastValidateContext?.ValidationTarget);
    }

    // Scenario: Given a generic async value validator and a mismatched target type, When Validate is invoked directly, Then it throws UnreachableException.
    [Fact]
    public async Task AsyncValueValidatorOfString_InvokeValidate_Throws_when_target_has_wrong_type()
    {
        var sut = new RecordingTypedAsyncValueValidator<string>(
            (_, _) => ValueTask.FromResult(ValidationResult.Valid)
        );

        var exception = await Assert.ThrowsAsync<UnreachableException>(() =>
            sut.InvokeValidate(new ValidationContext(new object()), CancellationToken.None).AsTask()
        );

        Assert.Contains(typeof(string).Name, exception.Message);
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

    private sealed class RecordingAsyncValueValidator(
        Func<IValidationContext, ValueTask<ValidationResult>> behavior
    ) : AsyncValueValidator
    {
        public int ValidateCallCount { get; private set; }

        public IValidationContext? LastValidateContext { get; private set; }

        protected override ValueTask<ValidationResult> Validate(
            IValidationContext context,
            CancellationToken cancel
        )
        {
            ValidateCallCount++;
            LastValidateContext = context;
            return behavior(context);
        }
    }

    private class RecordingTypedValueValidator<T> : ValueValidator<T>
    {
        private readonly Func<IValidationContext, T, ValidationResult> _behavior;
        private readonly IReadOnlyList<IValidator> _previousValidators;

        public RecordingTypedValueValidator(
            Func<IValidationContext, T, ValidationResult> behavior,
            IReadOnlyList<IValidator>? previousValidators = null
        )
        {
            _behavior = behavior;
            _previousValidators = previousValidators ?? [];
        }

        public int ValidateCallCount { get; private set; }

        public IValidationContext? LastValidateContext { get; private set; }

        protected override IEnumerable<IValidator> PreviousValidators => _previousValidators;

        public ValidationResult InvokeValidate(IValidationContext context)
        {
            return Validate(context);
        }

        protected override ValidationResult Validate(IValidationContext context, T value)
        {
            ValidateCallCount++;
            LastValidateContext = context;
            return _behavior(context, value);
        }
    }

    private sealed class DefaultPreviousTypedValueValidator<T>(
        Func<IValidationContext, T, ValidationResult> behavior
    ) : ValueValidator<T>
    {
        private readonly Func<IValidationContext, T, ValidationResult> _behavior = behavior;

        public int ValidateCallCount { get; private set; }

        public IValidationContext? LastValidateContext { get; private set; }

        public ValidationResult InvokeValidate(IValidationContext context)
        {
            return Validate(context);
        }

        protected override ValidationResult Validate(IValidationContext context, T value)
        {
            ValidateCallCount++;
            LastValidateContext = context;
            return _behavior(context, value);
        }
    }

    private class RecordingTypedAsyncValueValidator<T> : AsyncValueValidator<T>
    {
        private readonly Func<IValidationContext, T, ValueTask<ValidationResult>> _behavior;
        private readonly IReadOnlyList<IValidator> _previousValidators;

        public RecordingTypedAsyncValueValidator(
            Func<IValidationContext, T, ValueTask<ValidationResult>> behavior,
            IReadOnlyList<IValidator>? previousValidators = null
        )
        {
            _behavior = behavior;
            _previousValidators = previousValidators ?? [];
        }

        public int ValidateCallCount { get; private set; }

        public IValidationContext? LastValidateContext { get; private set; }

        protected override IEnumerable<IValidator> PreviousValidators => _previousValidators;

        public ValueTask<ValidationResult> InvokeValidate(
            IValidationContext context,
            CancellationToken cancel
        )
        {
            return Validate(context, cancel);
        }

        protected override ValueTask<ValidationResult> Validate(
            IValidationContext context,
            T value,
            CancellationToken cancel
        )
        {
            ValidateCallCount++;
            LastValidateContext = context;
            return _behavior(context, value);
        }
    }

    private sealed class DefaultPreviousTypedAsyncValueValidator<T>(
        Func<IValidationContext, T, ValueTask<ValidationResult>> behavior
    ) : AsyncValueValidator<T>
    {
        private readonly Func<IValidationContext, T, ValueTask<ValidationResult>> _behavior =
            behavior;

        public int ValidateCallCount { get; private set; }

        public IValidationContext? LastValidateContext { get; private set; }

        public ValueTask<ValidationResult> InvokeValidate(
            IValidationContext context,
            CancellationToken cancel
        )
        {
            return Validate(context, cancel);
        }

        protected override ValueTask<ValidationResult> Validate(
            IValidationContext context,
            T value,
            CancellationToken cancel
        )
        {
            ValidateCallCount++;
            LastValidateContext = context;
            return _behavior(context, value);
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
