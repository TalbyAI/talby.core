using System.Collections;

namespace Talby.Core.Validation.UnitTests.Validators;

public sealed class NotEmptyValidatorTests
{
    // Scenario: Given a non-empty string, When ValidateAsync runs, Then it returns success and preserves the value.
    [Fact]
    public async Task GivenNonEmptyString_WhenValidateAsync_ThenReturnsSuccess()
    {
        var target = "hello";
        var sut = NotEmptyValidator<string>.Instance;

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Same(target, result.ResultValue);
        Assert.Empty(result.Errors);
    }

    // Scenario: Given an empty string, When ValidateAsync runs, Then it fails with the not-empty message.
    [Fact]
    public async Task GivenEmptyString_WhenValidateAsync_ThenReturnsFailure()
    {
        var sut = NotEmptyValidator<string>.Instance;

        var result = await sut.ValidateAsync(
            new ValidationContext(string.Empty),
            CancellationToken.None
        );

        var failure = Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Equal(ValidationSeverity.Error, result.Severity);
        Assert.Equal(ValidationPath.Root, failure.Path);
        Assert.Equal(ValidationSeverity.Error, failure.Severity);
        Assert.Equal(NotEmptyValidator<string>.ErrorCode, failure.ErrorCode);
        Assert.Equal(string.Empty, failure.AttemptedValue as string);
        Assert.Equal("Is required", failure.ErrorMessageFunc());
    }

    // Scenario: Given a null string, When ValidateAsync runs, Then it fails with the not-empty message.
    [Fact]
    public async Task GivenNullString_WhenValidateAsync_ThenReturnsFailure()
    {
        var sut = NotEmptyValidator<string>.Instance;

        var result = await sut.ValidateAsync(new ValidationContext(null), CancellationToken.None);

        var failure = Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Equal(ValidationSeverity.Error, result.Severity);
        Assert.Equal(ValidationPath.Root, failure.Path);
        Assert.Equal(ValidationSeverity.Error, failure.Severity);
        Assert.Equal(NotEmptyValidator<string>.ErrorCode, failure.ErrorCode);
        Assert.Null(failure.AttemptedValue);
        Assert.Equal("Is required", failure.ErrorMessageFunc());
    }

    // Scenario: Given zero, When ValidateAsync runs, Then it fails because zero is the default value.
    [Fact]
    public async Task GivenZeroInt_WhenValidateAsync_ThenReturnsFailure()
    {
        var sut = NotEmptyValidator<int>.Instance;

        var result = await sut.ValidateAsync(new ValidationContext(0), CancellationToken.None);

        var failure = Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Null(result.ResultValue);
        Assert.Equal(ValidationSeverity.Error, result.Severity);
        Assert.Equal(ValidationPath.Root, failure.Path);
        Assert.Equal(ValidationSeverity.Error, failure.Severity);
        Assert.Equal(NotEmptyValidator<int>.ErrorCode, failure.ErrorCode);
        Assert.Equal(0, (int)failure.AttemptedValue!);
        Assert.Equal("Is required", failure.ErrorMessageFunc());
    }

    // Scenario: Given a non-zero integer, When ValidateAsync runs, Then it returns success and preserves the value.
    [Fact]
    public async Task GivenNonZeroInt_WhenValidateAsync_ThenReturnsSuccess()
    {
        var target = 5;
        var sut = NotEmptyValidator<int>.Instance;

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Equal(target, result.ResultValue);
        Assert.Empty(result.Errors);
    }

    // Scenario: Given a deferred enumerable, When ValidateAsync runs, Then it does not enumerate the sequence and returns success.
    [Fact]
    public async Task GivenDeferredEnumerable_WhenValidateAsync_ThenDoesNotEnumerate()
    {
        var target = new DeferredEnumerable();
        var sut = NotEmptyValidator<DeferredEnumerable>.Instance;

        var result = await sut.ValidateAsync(new ValidationContext(target), CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Same(target, result.ResultValue);
        Assert.Empty(result.Errors);
        Assert.Equal(0, target.MoveNextCount);
    }

    private sealed class DeferredEnumerable : IEnumerable<int>
    {
        public int MoveNextCount { get; private set; }

        public IEnumerator<int> GetEnumerator()
        {
            MoveNextCount++;
            yield return 1;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
