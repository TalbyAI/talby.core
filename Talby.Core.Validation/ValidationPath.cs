using System.Text.RegularExpressions;
using Talby.Core.SourceGenerators;

namespace Talby.Core.Validation;

[GenerateDiscriminatedUnion]
public abstract partial record ValidationPath
{
    public abstract string Path { get; }

    #region [ Cases ]

    public record RootPath() : ValidationPath
    {
        public override string Path => RootPathString;
    }

    public abstract record ChildPath(ValidationPath Parent) : ValidationPath;

    public record PropertyPath(ValidationPath Parent, string Property) : ChildPath(Parent)
    {
        private readonly Lazy<string> _path = new(() => $"{Parent.Path}.{Property}");

        public override string Path => _path.Value;
    }

    public record IndexPath(ValidationPath Parent, int Index) : ChildPath(Parent)
    {
        private readonly Lazy<string> _path = new(() => $"{Parent.Path}[{Index}]");

        public override string Path => _path.Value;
    }

    #endregion [ Cases ]

    public override string ToString() => Path;

    #region [ Root ]

    public const string RootPathString = "$";

    public static readonly ValidationPath Root = new RootPath();

    #endregion [ Root ]

    #region [ Property ]

    public ValidationPath ForProperty(string propertyName) =>
        new PropertyPath(this, ValidatedPropertyName(propertyName));

    [GeneratedRegex(@"^[a-zA-Z_][a-zA-Z0-9_]*$")]
    private static partial Regex PropertyNameRegex();

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

    public ValidationPath ForIndex(int index) => new IndexPath(this, ValidatedIndex(index));

    public static int ValidatedIndex(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be non-negative.");
        }

        return index;
    }

    #endregion [ Index ]

    public void Match(Action<RootPath> matchRootPathAction, Action<ChildPath> matchChildPathAction)
    {
        switch (this)
        {
            case RootPath r:
                matchRootPathAction(r);
                break;
            case ChildPath c:
                matchChildPathAction(c);
                break;
            default:
                throw new InvalidOperationException("Unknown ValidationPath type.");
        }
    }
}
