namespace Talby.Core.Validation;

public sealed class IsLengthInRangeValidator : ValueValidator<string>
{
    public const string ErrorCode = "IsLengthInRange";

    public static IsLengthInRangeValidator WithRange(
        int minLength,
        int maxLength,
        ValidationSeverity severity = ValidationSeverity.Error
    )
    {
        if (minLength < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minLength),
                Resources.IsLengthInRangeMinimumLengthCannotBeNegativeMessage
            );
        }

        if (maxLength < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxLength),
                Resources.IsLengthInRangeMaximumLengthCannotBeNegativeMessage
            );
        }

        if (maxLength >= 0 && minLength > maxLength)
        {
            throw new ArgumentException(
                Resources.IsLengthInRangeMinimumLengthCannotBeGreaterThanMaximumLengthMessage
            );
        }

        return new IsLengthInRangeValidator(
            minLength,
            maxLength,
            WithRangeValidatorFunc(minLength, maxLength, severity)
        )
        {
            Severity = severity,
        };
    }

    public static IsLengthInRangeValidator WithMinimumLength(
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

        return new IsLengthInRangeValidator(
            minLength,
            null,
            WithMinimumLengthValidatorFunc(minLength, severity)
        )
        {
            Severity = severity,
        };
    }

    public static IsLengthInRangeValidator WithMaximumLength(
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

        return new IsLengthInRangeValidator(
            null,
            maxLength,
            WithMaximumLengthValidatorFunc(maxLength, severity)
        )
        {
            Severity = severity,
        };
    }

    private IsLengthInRangeValidator(
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
                            Resources.IsLengthInRangeValueMustBeAtLeastMessageFormat,
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
                            Resources.IsLengthInRangeValueMustBeAtMostMessageFormat,
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
                            Resources.IsLengthInRangeValueMustBeBetweenMessageFormat,
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
