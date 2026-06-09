namespace Talby.Core.Validation.UnitTests;

public sealed class ValidationResultTests
{
    // Scenario: expose the built-in valid result as an empty info-level validation outcome.
    [Fact]
    public void ValidationResult_Valid_returns_an_empty_info_result()
    {
        var result = ValidationResult.Valid;

        Assert.Null(result.ResultValue);
        Assert.Equal(ValidationSeverity.Info, result.Severity);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    // Scenario: preserve the supplied value when a validation result has no errors.
    [Fact]
    public void ValidationResult_Success_preserves_the_result_value()
    {
        var expectedValue = new object();

        var result = ValidationResult.Success(expectedValue);

        Assert.Same(expectedValue, result.ResultValue);
        Assert.Equal(ValidationSeverity.Info, result.Severity);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    // Scenario: preserve all failures and report the highest severity when the span overload is used.
    [Fact]
    public void ValidationResult_Failures_from_span_preserves_errors_and_severity()
    {
        var warning = CreateFailure("title", ValidationSeverity.Warning, "title is discouraged");
        var error = CreateFailure("email", ValidationSeverity.Error, "email is invalid");

        var result = ValidationResult.Failures((ReadOnlySpan<ValidationFailure>)[warning, error]);

        Assert.Null(result.ResultValue);
        Assert.Equal(ValidationSeverity.Error, result.Severity);
        Assert.False(result.IsValid);
        Assert.Collection(
            result.Errors,
            actualWarning => Assert.Same(warning, actualWarning),
            actualError => Assert.Same(error, actualError)
        );
    }

    // Scenario: treat a null enumerable overload as an empty failure list.
    [Fact]
    public void ValidationResult_Failures_from_enumerable_treats_null_as_empty()
    {
        var result = ValidationResult.Failures((IEnumerable<ValidationFailure>?)null);

        Assert.Null(result.ResultValue);
        Assert.Equal(ValidationSeverity.Info, result.Severity);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    // Scenario: preserve enumerable failures and compute severity from their highest level.
    [Fact]
    public void ValidationResult_Failures_from_enumerable_preserves_errors_and_severity()
    {
        var warning = CreateFailure("name", ValidationSeverity.Warning, "name is too short");
        var error = CreateFailure("age", ValidationSeverity.Error, "age is invalid");

        var result = ValidationResult.Failures(new[] { warning, error });

        Assert.Null(result.ResultValue);
        Assert.Equal(ValidationSeverity.Error, result.Severity);
        Assert.False(result.IsValid);
        Assert.Collection(
            result.Errors,
            actualWarning => Assert.Same(warning, actualWarning),
            actualError => Assert.Same(error, actualError)
        );
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
}
