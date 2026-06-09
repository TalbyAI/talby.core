namespace Talby.Core.Validation;

public interface IValidationContext
{
    object? ValidationTarget { get; }
    ValidationPath Path { get; }
    IValidationContext? ParentContext { get; }

    IValidationContext WithTarget(object? newTarget);
}

public sealed class ValidationContext : IValidationContext
{
    public ValidationContext(
        IValidationContext parentContext,
        ValidationPath path,
        object? validatingTarget
    )
    {
        ParentContext = parentContext ?? throw new ArgumentNullException(nameof(parentContext));
        Path = path ?? throw new ArgumentNullException(nameof(path));
        ValidationTarget = validatingTarget;

        path.Match(
            onRootPath: _ =>
                throw new ArgumentException(
                    "Path cannot be the root path when a parent context is provided.",
                    nameof(path)
                ),
            onChildPath: child =>
            {
                if (!parentContext.Path.Equals(child.Parent))
                {
                    throw new ArgumentException(
                        "Path must be a child path when a parent context is provided.",
                        nameof(path)
                    );
                }
            }
        );
    }

    public ValidationContext(object? validatingTarget)
    {
        ParentContext = null;
        Path = ValidationPath.Root;
        ValidationTarget = validatingTarget;
    }

    private ValidationContext(object? newTarget, ValidationContext source)
    {
        ParentContext = source.ParentContext;
        Path = source.Path;
        ValidationTarget = newTarget;
    }

    public object? ValidationTarget { get; }
    public ValidationPath Path { get; }
    public IValidationContext? ParentContext { get; }

    public IValidationContext WithTarget(object? newTarget)
    {
        return new ValidationContext(newTarget, this);
    }
}
