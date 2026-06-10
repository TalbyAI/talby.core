namespace Talby.Core.Validation.UnitTests.Validators;

public sealed class NotNullValidatorTests
{
    // Scenario: Given a non-null target, When ValidateAsync runs, Then it returns the target and succeeds.
    [Fact]
    public async Task NotNullValidator_ValidateAsync_Returns_success_for_non_null_targets()
    {
        var target = new object();
        var sut = NotNullValidator.Instance;
        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        Assert.Same(NotNullValidator.Instance, sut);
        Assert.True(result.IsValid);
        Assert.Same(target, result.ResultValue);
        Assert.Empty(result.Errors);
        Assert.Equal("NotNull", NotNullValidator.ErrorCode);
    }

    // Scenario: Given a null target, When ValidateAsync runs, Then it fails and reports the expected validation failure.
    [Fact]
    public async Task NotNullValidator_ValidateAsync_Returns_failure_for_null_targets()
    {
        var sut = NotNullValidator.Instance;
        var result = await sut.ValidateAsync(new ValidationContext(null), CancellationToken.None);

        var failure = Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Equal(ValidationSeverity.Error, result.Severity);
        Assert.Equal(ValidationPath.Root, failure.Path);
        Assert.Equal(ValidationSeverity.Error, failure.Severity);
        Assert.Equal(NotNullValidator.ErrorCode, failure.ErrorCode);
        Assert.Null(failure.AttemptedValue);
        Assert.Equal("Is required", failure.ErrorMessageFunc());
    }
}
