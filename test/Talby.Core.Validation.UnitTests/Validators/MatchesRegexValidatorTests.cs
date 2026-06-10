using System.Text.RegularExpressions;

namespace Talby.Core.Validation.UnitTests.Validators;

public sealed class MatchesRegexValidatorTests
{
    // Scenario: Given a null regex pattern, When the validator is created, Then it throws before validation can run.
    [Fact]
    public void MatchesRegexValidator_ctor_Throws_argument_null_exception_for_null_pattern()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new MatchesRegexValidator(pattern: null!)
        );

        Assert.Equal("pattern", exception.ParamName);
    }

    // Scenario: Given a matching string, When ValidateAsync runs, Then it returns the original string and succeeds.
    [Fact]
    public async Task MatchesRegexValidator_ValidateAsync_Returns_success_for_matching_targets()
    {
        var pattern = new Regex("^hello$");
        var sut = new MatchesRegexValidator(pattern);

        var result = await sut.ValidateAsync(
            new ValidationContext("hello"),
            CancellationToken.None
        );

        Assert.Same(pattern, sut.Pattern);
        Assert.True(result.IsValid);
        Assert.Equal("hello", result.ResultValue);
        Assert.Empty(result.Errors);
    }

    // Scenario: Given a non-matching string, When ValidateAsync runs, Then it returns a failure with the attempted value and pattern.
    [Fact]
    public async Task MatchesRegexValidator_ValidateAsync_Returns_failure_for_non_matching_targets()
    {
        var pattern = new Regex("^hello$");
        var sut = new MatchesRegexValidator(pattern);

        var result = await sut.ValidateAsync(
            new ValidationContext("goodbye"),
            CancellationToken.None
        );

        var failure = Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Equal(ValidationSeverity.Error, result.Severity);
        Assert.Equal(ValidationPath.Root, failure.Path);
        Assert.Equal(ValidationSeverity.Error, failure.Severity);
        Assert.Equal(MatchesRegexValidator.ErrorCode, failure.ErrorCode);
        Assert.Equal("goodbye", failure.AttemptedValue);
        Assert.NotNull(failure.AdditionalData);
        Assert.Equal("^hello$", Assert.Single(failure.AdditionalData).Value);
        Assert.Equal("Must match the required pattern.", failure.ErrorMessageFunc());
    }

    // Scenario: Given a custom error message factory, When validation fails, Then it uses the custom message instead of the resource string.
    [Fact]
    public async Task MatchesRegexValidator_ValidateAsync_Uses_custom_error_message_when_provided()
    {
        var pattern = new Regex("^hello$");
        var sut = new MatchesRegexValidator(pattern)
        {
            ErrorMessageFunc = () => "Custom match error",
        };

        var result = await sut.ValidateAsync(
            new ValidationContext("goodbye"),
            CancellationToken.None
        );

        var failure = Assert.Single(result.Errors);

        Assert.Equal("Custom match error", failure.ErrorMessageFunc());
    }
}
