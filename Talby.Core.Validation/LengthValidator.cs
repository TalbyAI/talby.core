namespace Talby.Core.Validation;

public sealed class LengthValidator : ValueValidator<string>
{
    public const string ErrorCode = "Length";

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

    public int? MinLength { get; }
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

    protected override ValidationResult Validate(IValidationContext context, string value)
    {
        if (validatorFunc(value, context.Path) is { } validationFailure)
        {
            return ValidationResult.Failures(validationFailure);
        }

        return ValidationResult.Success(value);
    }
}
