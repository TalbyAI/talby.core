using Talby.Core.Validation;

namespace Talby.Core.Validation.UnitTests;

public sealed class IsNotNullValidatorTests
{
    // Scenario: Given a non-null target, When ValidateAsync runs, Then it returns the target and succeeds.
    [Fact]
    public async Task IsNotNullValidator_ValidateAsync_Returns_success_for_non_null_targets()
    {
        var target = new object();
        var sut = IsNotNullValidator.Instance;
        var result = await sut.ValidateAsync(new ValidationContext(target));

        Assert.Same(IsNotNullValidator.Instance, sut);
        Assert.True(result.IsValid);
        Assert.Same(target, result.ResultValue);
        Assert.Empty(result.Errors);
        Assert.Equal("IsNotNull", IsNotNullValidator.ErrorCode);
    }

    // Scenario: Given a null target, When ValidateAsync runs, Then it fails and reports the expected validation failure.
    [Fact]
    public async Task IsNotNullValidator_ValidateAsync_Returns_failure_for_null_targets()
    {
        var sut = IsNotNullValidator.Instance;
        var result = await sut.ValidateAsync(new ValidationContext(null));

        var failure = Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Equal(ValidationSeverity.Error, result.Severity);
        Assert.Equal(ValidationPath.Root, failure.Path);
        Assert.Equal(ValidationSeverity.Error, failure.Severity);
        Assert.Equal(IsNotNullValidator.ErrorCode, failure.ErrorCode);
        Assert.Null(failure.AttemptedValue);
        Assert.Equal("Is required", failure.ErrorMessageFunc());
    }
}
