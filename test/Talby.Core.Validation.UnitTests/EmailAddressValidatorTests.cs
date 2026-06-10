namespace Talby.Core.Validation.UnitTests;

public sealed class EmailAddressValidatorTests
{
    // Scenario: Given a valid email address, When ValidateAsync runs, Then it returns success and preserves the value.
    [Fact]
    public async Task EmailAddressValidator_ValidateAsync_Returns_success_for_valid_email_addresses()
    {
        var target = "user@example.com";
        var sut = new EmailAddressValidator();

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Same(target, result.ResultValue);
        Assert.Empty(result.Errors);
    }

    // Scenario: Given an invalid email address, When ValidateAsync runs, Then it returns the expected failure.
    [Fact]
    public async Task EmailAddressValidator_ValidateAsync_Returns_failure_for_invalid_email_addresses()
    {
        var target = "not-an-email";
        var sut = new EmailAddressValidator();

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        var failure = Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Equal(ValidationSeverity.Error, result.Severity);
        Assert.Equal(ValidationPath.Root, failure.Path);
        Assert.Equal(ValidationSeverity.Error, failure.Severity);
        Assert.Equal(EmailAddressValidator.ErrorCode, failure.ErrorCode);
        Assert.Equal(target, failure.AttemptedValue);
        Assert.Equal("Must be a valid email address.", failure.ErrorMessageFunc());
    }

    // Scenario: Given a null email target, When ValidateAsync runs, Then it returns success and leaves null handling to presence validators.
    [Fact]
    public async Task EmailAddressValidator_ValidateAsync_Returns_success_for_null_email_targets()
    {
        var sut = new EmailAddressValidator();

        var result = await sut.ValidateAsync(new ValidationContext(null), CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Empty(result.Errors);
    }

    // Scenario: Given a custom error message factory, When validation fails, Then it uses the custom message instead of the resource string.
    [Fact]
    public async Task EmailAddressValidator_ValidateAsync_Uses_custom_error_message_when_provided()
    {
        var sut = new EmailAddressValidator
        {
            ErrorMessageFunc = () => "Custom email error",
        };

        var result = await sut.ValidateAsync(new ValidationContext("not-an-email"), CancellationToken.None);

        var failure = Assert.Single(result.Errors);

        Assert.Equal("Custom email error", failure.ErrorMessageFunc());
    }
}
