using System.Runtime.CompilerServices;
using Talby.Core.Validation;

namespace Talby.Core.Validation.UnitTests;

public sealed class IsLengthInRangeValidatorTests
{
    // Scenario: Given a string within the configured range, When ValidateAsync runs, Then it returns success and the original string.
    [Fact]
    public async Task IsLengthInRangeValidator_ValidateAsync_Returns_success_for_values_within_range()
    {
        var target = "test";
        var sut = IsLengthInRangeValidator.WithRange(2, 5, ValidationSeverity.Warning);

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Empty(result.Errors);
        Assert.Equal(2, sut.MinLength);
        Assert.Equal(5, sut.MaxLength);
        Assert.Equal(ValidationSeverity.Warning, sut.Severity);
    }

    // Scenario: Given a string shorter than the configured range, When ValidateAsync runs, Then it returns the range failure.
    [Fact]
    public async Task IsLengthInRangeValidator_ValidateAsync_Returns_failure_when_value_is_shorter_than_the_configured_range()
    {
        var target = "a";
        var sut = IsLengthInRangeValidator.WithRange(2, 5, ValidationSeverity.Warning);

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        var failure = Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Equal(ValidationSeverity.Warning, result.Severity);
        Assert.Equal(ValidationPath.Root, failure.Path);
        Assert.Equal(ValidationSeverity.Warning, failure.Severity);
        Assert.Equal(IsLengthInRangeValidator.ErrorCode, failure.ErrorCode);
        Assert.Same(target, failure.AttemptedValue);
        Assert.Equal("Must be between 2 and 5 characters long.", failure.ErrorMessageFunc());
        Assert.NotNull(failure.AdditionalData);
        Assert.Contains(
            failure.AdditionalData!,
            entry => entry.Key == "MinLength" && Equals(entry.Value, 2)
        );
        Assert.Contains(
            failure.AdditionalData!,
            entry => entry.Key == "MaxLength" && Equals(entry.Value, 5)
        );
    }

    // Scenario: Given a string longer than the configured range, When ValidateAsync runs, Then it returns the range failure.
    [Fact]
    public async Task IsLengthInRangeValidator_ValidateAsync_Returns_failure_when_value_is_longer_than_the_configured_range()
    {
        var target = "abcdef";
        var sut = IsLengthInRangeValidator.WithRange(2, 5, ValidationSeverity.Warning);

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        var failure = Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Equal(ValidationSeverity.Warning, result.Severity);
        Assert.Equal(ValidationPath.Root, failure.Path);
        Assert.Equal(ValidationSeverity.Warning, failure.Severity);
        Assert.Equal(IsLengthInRangeValidator.ErrorCode, failure.ErrorCode);
        Assert.Same(target, failure.AttemptedValue);
        Assert.Equal("Must be between 2 and 5 characters long.", failure.ErrorMessageFunc());
        Assert.NotNull(failure.AdditionalData);
        Assert.Contains(
            failure.AdditionalData!,
            entry => entry.Key == "MinLength" && Equals(entry.Value, 2)
        );
        Assert.Contains(
            failure.AdditionalData!,
            entry => entry.Key == "MaxLength" && Equals(entry.Value, 5)
        );
    }

    // Scenario: Given a string shorter than the minimum length, When ValidateAsync runs, Then it returns the minimum length failure.
    [Fact]
    public async Task IsLengthInRangeValidator_ValidateAsync_Returns_failure_when_value_is_shorter_than_minimum_length()
    {
        var target = "ab";
        var sut = IsLengthInRangeValidator.WithMinimumLength(3, ValidationSeverity.Warning);

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        var failure = Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Equal(ValidationSeverity.Warning, result.Severity);
        Assert.Equal(ValidationPath.Root, failure.Path);
        Assert.Equal(ValidationSeverity.Warning, failure.Severity);
        Assert.Equal(IsLengthInRangeValidator.ErrorCode, failure.ErrorCode);
        Assert.Same(target, failure.AttemptedValue);
        Assert.Equal("Must be at least 3 characters long.", failure.ErrorMessageFunc());
        Assert.NotNull(failure.AdditionalData);
        var minimumLength = Assert.Single(failure.AdditionalData!);
        Assert.Equal("MinLength", minimumLength.Key);
        Assert.Equal(3, minimumLength.Value);
    }

    // Scenario: Given a string longer than the maximum length, When ValidateAsync runs, Then it returns the maximum length failure.
    [Fact]
    public async Task IsLengthInRangeValidator_ValidateAsync_Returns_failure_when_value_is_longer_than_maximum_length()
    {
        var target = "abcd";
        var sut = IsLengthInRangeValidator.WithMaximumLength(3, ValidationSeverity.Warning);

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        var failure = Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Equal(ValidationSeverity.Warning, result.Severity);
        Assert.Equal(ValidationPath.Root, failure.Path);
        Assert.Equal(ValidationSeverity.Warning, failure.Severity);
        Assert.Equal(IsLengthInRangeValidator.ErrorCode, failure.ErrorCode);
        Assert.Same(target, failure.AttemptedValue);
        Assert.Equal("Must be at most 3 characters long.", failure.ErrorMessageFunc());
        Assert.NotNull(failure.AdditionalData);
        var maximumLength = Assert.Single(failure.AdditionalData!);
        Assert.Equal("MaxLength", maximumLength.Key);
        Assert.Equal(3, maximumLength.Value);
    }

    // Scenario: Given a non-string target, When the protected TryValidate method runs, Then it returns success without a failure.
    [Fact]
    public void IsLengthInRangeValidator_TryValidate_Returns_success_for_non_string_targets()
    {
        var target = new object();
        var sut = IsLengthInRangeValidator.WithRange(1, 3);

        var result = TryValidate(
            sut,
            new ValidationContext(target),
            out var validatedValue,
            out var failure
        );

        Assert.True(result);
        Assert.Null(validatedValue);
        Assert.Null(failure);
    }

    // Scenario: Given a negative minimum length, When WithRange is called, Then it throws an ArgumentOutOfRangeException.
    [Fact]
    public void IsLengthInRangeValidator_WithRange_Throws_when_minimum_length_is_negative()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            IsLengthInRangeValidator.WithRange(-1, 3)
        );

        Assert.Equal("minLength", exception.ParamName);
        Assert.StartsWith("Minimum length cannot be negative.", exception.Message);
    }

    // Scenario: Given a negative maximum length, When WithRange is called, Then it throws an ArgumentOutOfRangeException.
    [Fact]
    public void IsLengthInRangeValidator_WithRange_Throws_when_maximum_length_is_negative()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            IsLengthInRangeValidator.WithRange(1, -1)
        );

        Assert.Equal("maxLength", exception.ParamName);
        Assert.StartsWith("Maximum length cannot be negative.", exception.Message);
    }

    // Scenario: Given a minimum length greater than the maximum length, When WithRange is called, Then it throws an ArgumentException.
    [Fact]
    public void IsLengthInRangeValidator_WithRange_Throws_when_minimum_length_is_greater_than_maximum_length()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            IsLengthInRangeValidator.WithRange(4, 3)
        );

        Assert.StartsWith(
            "Minimum length cannot be greater than maximum length.",
            exception.Message
        );
    }

    // Scenario: Given a string meeting the minimum length, When ValidateAsync runs, Then it returns success and the original string.
    [Fact]
    public async Task IsLengthInRangeValidator_WithMinimumLength_Returns_success_for_values_meeting_the_minimum_length()
    {
        var target = "abc";
        var sut = IsLengthInRangeValidator.WithMinimumLength(3, ValidationSeverity.Info);

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Empty(result.Errors);
        Assert.Equal(3, sut.MinLength);
        Assert.Null(sut.MaxLength);
        Assert.Equal(ValidationSeverity.Info, sut.Severity);
    }

    // Scenario: Given a string shorter than the configured minimum length, When ValidateAsync runs, Then it returns the expected failure.
    [Fact]
    public async Task IsLengthInRangeValidator_WithMinimumLength_Returns_failure_for_values_shorter_than_the_minimum_length()
    {
        var target = "ab";
        var sut = IsLengthInRangeValidator.WithMinimumLength(3, ValidationSeverity.Warning);

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        var failure = Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Equal(ValidationSeverity.Warning, result.Severity);
        Assert.Equal(ValidationPath.Root, failure.Path);
        Assert.Equal(ValidationSeverity.Warning, failure.Severity);
        Assert.Equal(IsLengthInRangeValidator.ErrorCode, failure.ErrorCode);
        Assert.Same(target, failure.AttemptedValue);
        Assert.Equal("Must be at least 3 characters long.", failure.ErrorMessageFunc());
        Assert.NotNull(failure.AdditionalData);
        var minimumLength = Assert.Single(failure.AdditionalData!);
        Assert.Equal("MinLength", minimumLength.Key);
        Assert.Equal(3, minimumLength.Value);
    }

    // Scenario: Given a negative minimum length, When WithMinimumLength is called, Then it throws an ArgumentOutOfRangeException.
    [Fact]
    public void IsLengthInRangeValidator_WithMinimumLength_Throws_when_minimum_length_is_negative()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            IsLengthInRangeValidator.WithMinimumLength(-1)
        );

        Assert.Equal("minLength", exception.ParamName);
        Assert.StartsWith("Minimum length cannot be negative.", exception.Message);
    }

    // Scenario: Given a string meeting the maximum length, When ValidateAsync runs, Then it returns success and the original string.
    [Fact]
    public async Task IsLengthInRangeValidator_WithMaximumLength_Returns_success_for_values_meeting_the_maximum_length()
    {
        var target = "abc";
        var sut = IsLengthInRangeValidator.WithMaximumLength(3, ValidationSeverity.Info);

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Empty(result.Errors);
        Assert.Null(sut.MinLength);
        Assert.Equal(3, sut.MaxLength);
        Assert.Equal(ValidationSeverity.Info, sut.Severity);
    }

    // Scenario: Given a string longer than the configured maximum length, When ValidateAsync runs, Then it returns the expected failure.
    [Fact]
    public async Task IsLengthInRangeValidator_WithMaximumLength_Returns_failure_for_values_longer_than_the_maximum_length()
    {
        var target = "abcd";
        var sut = IsLengthInRangeValidator.WithMaximumLength(3, ValidationSeverity.Warning);

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        var failure = Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Equal(ValidationSeverity.Warning, result.Severity);
        Assert.Equal(ValidationPath.Root, failure.Path);
        Assert.Equal(ValidationSeverity.Warning, failure.Severity);
        Assert.Equal(IsLengthInRangeValidator.ErrorCode, failure.ErrorCode);
        Assert.Same(target, failure.AttemptedValue);
        Assert.Equal("Must be at most 3 characters long.", failure.ErrorMessageFunc());
        Assert.NotNull(failure.AdditionalData);
        var maximumLength = Assert.Single(failure.AdditionalData!);
        Assert.Equal("MaxLength", maximumLength.Key);
        Assert.Equal(3, maximumLength.Value);
    }

    // Scenario: Given a negative maximum length, When WithMaximumLength is called, Then it throws an ArgumentOutOfRangeException.
    [Fact]
    public void IsLengthInRangeValidator_WithMaximumLength_Throws_when_maximum_length_is_negative()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            IsLengthInRangeValidator.WithMaximumLength(-1)
        );

        Assert.Equal("maxLength", exception.ParamName);
        Assert.StartsWith("Maximum length cannot be negative.", exception.Message);
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "TryValidate")]
    private static extern bool TryValidate(
        IsLengthInRangeValidator validator,
        IValidationContext context,
        out object? validatedValue,
        out ValidationFailure? failure
    );
}
