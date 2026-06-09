namespace Talby.Core.Validation.UnitTests;

public sealed class IsOfTypeValidatorTests
{
    // Scenario: Given a matching string target, When ValidateAsync runs, Then it succeeds and returns the string.
    [Fact]
    public async Task IsOfTypeValidator_ValidateAsync_Returns_success_for_matching_reference_type_targets()
    {
        var target = "hello";
        var sut = IsOfTypeValidator<string>.Instance;

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Same(target, result.ResultValue);
        Assert.Empty(result.Errors);
        Assert.Equal("IsOfType", IsOfTypeValidator<string>.ErrorCode);
    }

    // Scenario: Given a null string target, When ValidateAsync runs, Then it succeeds because string is nullable.
    [Fact]
    public async Task IsOfTypeValidator_ValidateAsync_Returns_success_for_null_reference_type_targets()
    {
        var sut = IsOfTypeValidator<string>.Instance;

        var result = await sut.ValidateAsync(new ValidationContext(null), CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Empty(result.Errors);
    }

    // Scenario: Given an int validator and a non-int target, When ValidateAsync runs, Then it fails with the expected message.
    [Fact]
    public async Task IsOfTypeValidator_ValidateAsync_Returns_failure_for_non_matching_targets()
    {
        var target = new object();
        var sut = IsOfTypeValidator<int>.Instance;

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        var failure = Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Equal(ValidationSeverity.Error, result.Severity);
        Assert.Equal(ValidationPath.Root, failure.Path);
        Assert.Equal(ValidationSeverity.Error, failure.Severity);
        Assert.Equal(IsOfTypeValidator<int>.ErrorCode, failure.ErrorCode);
        Assert.Same(target, failure.AttemptedValue);
        Assert.Equal("Must be of type Int32.", failure.ErrorMessageFunc());
    }
}
