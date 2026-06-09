using System.Text.RegularExpressions;
using Talby.Core.SourceGenerators;

namespace Talby.Core.Validation;

/// <summary>
/// Represents a validation path within a validation tree.
/// </summary>
[GenerateDiscriminatedUnion]
public abstract partial record ValidationPath
{
    /// <summary>
    /// Gets the formatted path string.
    /// </summary>
    public abstract string Path { get; }

    #region [ Cases ]

    /// <summary>
    /// Represents the root validation path.
    /// </summary>
    public record RootPath() : ValidationPath
    {
        /// <inheritdoc />
        public override string Path => RootPathString;

        /// <summary>
        /// Returns the formatted path string.
        /// </summary>
        public override string ToString() => Path;
    }

    /// <summary>
    /// Represents a validation path that has a parent path.
    /// </summary>
    public abstract record ChildPath(ValidationPath Parent) : ValidationPath;

    /// <summary>
    /// Represents a validation path that targets a property.
    /// </summary>
    public record PropertyPath(ValidationPath Parent, string Property) : ChildPath(Parent)
    {
        /// <inheritdoc />
        public override string Path => $"{Parent.Path}.{Property}";

        /// <summary>
        /// Returns the formatted path string.
        /// </summary>
        public override string ToString() => Path;
    }

    /// <summary>
    /// Represents a validation path that targets an index.
    /// </summary>
    public record IndexPath(ValidationPath Parent, int Index) : ChildPath(Parent)
    {
        /// <inheritdoc />
        public override string Path => $"{Parent.Path}[{Index}]";

        /// <summary>
        /// Returns the formatted path string.
        /// </summary>
        public override string ToString() => Path;
    }

    #endregion [ Cases ]

    #region [ Root ]

    /// <summary>
    /// Gets the token used for the root path.
    /// </summary>
    public const string RootPathString = "$";

    /// <summary>
    /// Gets the root validation path instance.
    /// </summary>
    public static readonly ValidationPath Root = new RootPath();

    #endregion [ Root ]

    #region [ Property ]

    /// <summary>
    /// Creates a child path for the supplied property name.
    /// </summary>
    public ValidationPath ForProperty(string propertyName) =>
        new PropertyPath(this, ValidatedPropertyName(propertyName));

    [GeneratedRegex(@"^[a-zA-Z_][a-zA-Z0-9_]*$")]
    private static partial Regex PropertyNameRegex();

    /// <summary>
    /// Normalizes and validates a property name.
    /// </summary>
    public static string ValidatedPropertyName(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException("Property name must be provided.", nameof(propertyName));
        }

        propertyName = propertyName.Trim();

        if (!PropertyNameRegex().IsMatch(propertyName))
        {
            throw new ArgumentException(
                "Property name must start with a letter or underscore and contain only letters, digits, or underscores.",
                nameof(propertyName)
            );
        }

        return propertyName;
    }

    #endregion [ Property ]

    #region [ Index ]

    /// <summary>
    /// Creates a child path for the supplied index.
    /// </summary>
    public ValidationPath ForIndex(int index) => new IndexPath(this, ValidatedIndex(index));

    /// <summary>
    /// Validates an index used in a validation path.
    /// </summary>
    public static int ValidatedIndex(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be non-negative.");
        }

        return index;
    }

    #endregion [ Index ]
}
