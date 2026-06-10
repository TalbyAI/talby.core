namespace Talby.Core.Validation;

/// <summary>
/// Describes the state being validated at a specific path.
/// </summary>
public interface IValidationContext
{
    /// <summary>
    /// Gets the value currently being validated.
    /// </summary>
    object? ValidationTarget { get; }
    /// <summary>
    /// Gets the current validation path.
    /// </summary>
    ValidationPath Path { get; }
    /// <summary>
    /// Gets the parent validation context, if any.
    /// </summary>
    IValidationContext? ParentContext { get; }

    /// <summary>
    /// Creates a new context that keeps the same path and parent but validates a different target.
    /// </summary>
    /// <param name="newTarget">The new validation target.</param>
    /// <returns>A new validation context.</returns>
    IValidationContext WithTarget(object? newTarget);
}

/// <summary>
/// Represents a validation context instance.
/// </summary>
public sealed class ValidationContext : IValidationContext
{
    /// <summary>
    /// Creates a validation context for a child path.
    /// </summary>
    /// <param name="parentContext">The parent context.</param>
    /// <param name="path">The current validation path.</param>
    /// <param name="validatingTarget">The value being validated.</param>
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

    /// <summary>
    /// Creates a root validation context.
    /// </summary>
    /// <param name="validatingTarget">The value being validated.</param>
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

    /// <summary>
    /// Gets the value currently being validated.
    /// </summary>
    public object? ValidationTarget { get; }
    /// <summary>
    /// Gets the current validation path.
    /// </summary>
    public ValidationPath Path { get; }
    /// <summary>
    /// Gets the parent validation context, if any.
    /// </summary>
    public IValidationContext? ParentContext { get; }

    /// <summary>
    /// Creates a new context that keeps the same path and parent but validates a different target.
    /// </summary>
    /// <param name="newTarget">The new validation target.</param>
    /// <returns>A new validation context.</returns>
    public IValidationContext WithTarget(object? newTarget)
    {
        return new ValidationContext(newTarget, this);
    }
}
