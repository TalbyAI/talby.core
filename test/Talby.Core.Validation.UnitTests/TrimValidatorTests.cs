using System.Runtime.CompilerServices;
using Talby.Core.Validation;

namespace Talby.Core.Validation.UnitTests;

public sealed class TrimValidatorTests
{
    // Scenario: Given a string with surrounding whitespace, When ValidateAsync runs, Then it trims the value and returns success.
    [Fact]
    public async Task TrimValidator_ValidateAsync_Trims_surrounding_whitespace_by_default()
    {
        var sut = TrimValidator.Instance;
        var context = new ValidationContext("  hello  ");

        var result = await sut.ValidateAsync(context, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Equal("hello", result.ResultValue);
        Assert.Empty(result.Errors);
    }

    // Scenario: Given null trim characters, When ValidateAsync runs, Then it trims surrounding whitespace by default.
    [Fact]
    public async Task TrimValidator_ValidateAsync_Trims_surrounding_whitespace_when_trim_characters_are_null()
    {
        var sut = new TrimValidator(charsToTrim: null);
        var context = new ValidationContext("  hello  ");

        var result = await sut.ValidateAsync(context, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Equal("hello", result.ResultValue);
        Assert.Empty(result.Errors);
    }

    // Scenario: Given a string with configured trim characters, When ValidateAsync runs, Then it trims only those characters and returns success.
    [Fact]
    public async Task TrimValidator_ValidateAsync_Trims_configured_characters()
    {
        var sut = new TrimValidator(new[] { 'x' });
        var context = new ValidationContext("xx value xx");

        var result = await sut.ValidateAsync(context, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Equal(" value ", result.ResultValue);
        Assert.Empty(result.Errors);
    }

    // Scenario: Given a non-string target, When ValidateAsync runs, Then it fails before trimming because the value must be a string.
    [Fact]
    public async Task TrimValidator_ValidateAsync_Rejects_non_string_targets()
    {
        var target = new object();
        var sut = TrimValidator.Instance;

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        var failure = Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Equal(ValidationSeverity.Error, result.Severity);
        Assert.Equal(IsOfTypeValidator<string>.ErrorCode, failure.ErrorCode);
        Assert.Same(target, failure.AttemptedValue);
        Assert.Equal(ValidationPath.Root, failure.Path);
    }

    // Scenario: Given a non-string target, When TryValidate is invoked directly, Then it returns success without trimming.
    [Fact]
    public void TrimValidator_TryValidate_Returns_success_for_non_string_targets()
    {
        var sut = TrimValidator.Instance;
        var target = new object();
        var isValid = TrimValidatorAccessors.TryValidate(
            sut,
            new ValidationContext(target),
            out var validatedValue,
            out var failure
        );

        Assert.True(isValid);
        Assert.Null(validatedValue);
        Assert.Null(failure);
    }
}

file static class TrimValidatorAccessors
{
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "TryValidate")]
    public static extern bool TryValidate(
        TrimValidator instance,
        IValidationContext context,
        out object? validatedValue,
        out ValidationFailure? failure
    );
}
