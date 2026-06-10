namespace Talby.Core.Validation;

/// <summary>
/// Validates that a string length falls within a configured range.
/// </summary>
public sealed class LengthValidator : ValueValidator<string>
{
    /// <summary>
    /// Gets the error code reported when length validation fails.
    /// </summary>
    public const string ErrorCode = "Length";

    /// <summary>
    /// Creates a validator that enforces both minimum and maximum length.
    /// </summary>
    /// <param name="minLength">The minimum allowed length.</param>
    /// <param name="maxLength">The maximum allowed length.</param>
    /// <param name="severity">The severity reported on failure.</param>
    /// <returns>A length validator configured for the supplied range.</returns>
    public static LengthValidator WithRange(
        int minLength,
        int maxLength,
        ValidationSeverity severity = ValidationSeverity.Error
    )
    {
        if (minLength < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minLength),
                Resources.LengthValidatorMinimumLengthCannotBeNegativeMessage
            );
        }

        if (maxLength < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxLength),
                Resources.LengthValidatorMaximumLengthCannotBeNegativeMessage
            );
        }

        if (maxLength >= 0 && minLength > maxLength)
        {
            throw new ArgumentException(
                Resources.LengthValidatorMinimumLengthCannotBeGreaterThanMaximumLengthMessage
            );
        }

        return new LengthValidator(
            minLength,
            maxLength,
            WithRangeValidatorFunc(minLength, maxLength, severity)
        )
        {
            Severity = severity,
        };
    }

    /// <summary>
    /// Creates a validator that enforces a minimum length.
    /// </summary>
    /// <param name="minLength">The minimum allowed length.</param>
    /// <param name="severity">The severity reported on failure.</param>
    /// <returns>A length validator configured for the supplied minimum.</returns>
    public static LengthValidator WithMinimumLength(
        int minLength,
        ValidationSeverity severity = ValidationSeverity.Error
    )
    {
        if (minLength < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minLength),
                "Minimum length cannot be negative."
            );
        }

        return new LengthValidator(
            minLength,
            null,
            WithMinimumLengthValidatorFunc(minLength, severity)
        )
        {
            Severity = severity,
        };
    }

    /// <summary>
    /// Creates a validator that enforces a maximum length.
    /// </summary>
    /// <param name="maxLength">The maximum allowed length.</param>
    /// <param name="severity">The severity reported on failure.</param>
    /// <returns>A length validator configured for the supplied maximum.</returns>
    public static LengthValidator WithMaximumLength(
        int maxLength,
        ValidationSeverity severity = ValidationSeverity.Error
    )
    {
        if (maxLength < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxLength),
                "Maximum length cannot be negative."
            );
        }

        return new LengthValidator(
            null,
            maxLength,
            WithMaximumLengthValidatorFunc(maxLength, severity)
        )
        {
            Severity = severity,
        };
    }

    private LengthValidator(
        int? minValue,
        int? maxValue,
        Func<string, ValidationPath, ValidationFailure?> validatorFunc
    )
    {
        MinLength = minValue;
        MaxLength = maxValue;
        this.validatorFunc = validatorFunc;
    }

    /// <summary>
    /// Gets the minimum allowed length, if configured.
    /// </summary>
    public int? MinLength { get; }

    /// <summary>
    /// Gets the maximum allowed length, if configured.
    /// </summary>
    public int? MaxLength { get; }

    private readonly Func<string, ValidationPath, ValidationFailure?> validatorFunc;

    private static Func<string, ValidationPath, ValidationFailure?> WithMinimumLengthValidatorFunc(
        int minLength,
        ValidationSeverity severity
    )
    {
        return (value, path) =>
        {
            if (value.Length < minLength)
            {
                return new ValidationFailure(
                    path,
                    () =>
                        string.Format(
                            Resources.LengthValidatorValueMustBeAtLeastMessageFormat,
                            minLength
                        ),
                    severity,
                    ErrorCode,
                    AttemptedValue: value,
                    AdditionalData: new Dictionary<string, object?> { { "MinLength", minLength } }
                );
            }

            return null;
        };
    }

    private static Func<string, ValidationPath, ValidationFailure?> WithMaximumLengthValidatorFunc(
        int maxLength,
        ValidationSeverity severity
    )
    {
        return (value, path) =>
        {
            if (value.Length > maxLength)
            {
                return new ValidationFailure(
                    path,
                    () =>
                        string.Format(
                            Resources.LengthValidatorValueMustBeAtMostMessageFormat,
                            maxLength
                        ),
                    severity,
                    ErrorCode,
                    AttemptedValue: value,
                    AdditionalData: new Dictionary<string, object?> { { "MaxLength", maxLength } }
                );
            }

            return null;
        };
    }

    private static Func<string, ValidationPath, ValidationFailure?> WithRangeValidatorFunc(
        int minLength,
        int maxLength,
        ValidationSeverity severity
    )
    {
        return (value, path) =>
        {
            if (value.Length < minLength || value.Length > maxLength)
            {
                return new ValidationFailure(
                    path,
                    () =>
                        string.Format(
                            Resources.LengthValidatorValueMustBeBetweenMessageFormat,
                            minLength,
                            maxLength
                        ),
                    severity,
                    ErrorCode,
                    AttemptedValue: value,
                    AdditionalData: new Dictionary<string, object?>
                    {
                        { "MinLength", minLength },
                        { "MaxLength", maxLength },
                    }
                );
            }

            return null;
        };
    }

    /// <summary>
    /// Validates the supplied string length.
    /// </summary>
    /// <param name="context">The validation context being validated.</param>
    /// <param name="value">The string to validate.</param>
    /// <returns>The validation result.</returns>
    protected override ValidationResult Validate(IValidationContext context, string value)
    {
        if (validatorFunc(value, context.Path) is { } validationFailure)
        {
            return ValidationResult.Failures(validationFailure);
        }

        return ValidationResult.Success(value);
    }
}
