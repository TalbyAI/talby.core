namespace Talby.Core.Validation.UnitTests;

public sealed class ValidationContextTests
{
    // Scenario: create a root validation context with no parent and the supplied target.
    [Fact]
    public void ValidationContext_Constructor_with_target_initializes_a_root_context()
    {
        var expectedTarget = new object();

        var context = new ValidationContext(expectedTarget);

        Assert.Null(context.ParentContext);
        Assert.Same(ValidationPath.Root, context.Path);
        Assert.Same(expectedTarget, context.ValidationTarget);
    }

    // Scenario: create a child validation context when the parent path matches the child path.
    [Fact]
    public void ValidationContext_Constructor_with_parent_and_matching_child_path_initializes_the_context()
    {
        var parentContext = new ValidationContext("parent-target");
        var childPath = ValidationPath.Root.ForProperty("name");
        var childTarget = new object();

        var context = new ValidationContext(parentContext, childPath, childTarget);

        Assert.Same(parentContext, context.ParentContext);
        Assert.Same(childPath, context.Path);
        Assert.Same(childTarget, context.ValidationTarget);
    }

    // Scenario: reject a null parent context before the validation context is created.
    [Fact]
    public void ValidationContext_Constructor_with_parent_throws_when_parent_context_is_null()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ValidationContext(null!, ValidationPath.Root.ForProperty("name"), "target")
        );

        Assert.Equal("parentContext", exception.ParamName);
    }

    // Scenario: reject a null path before the validation context is created.
    [Fact]
    public void ValidationContext_Constructor_with_parent_throws_when_path_is_null()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ValidationContext(new ValidationContext("parent-target"), null!, "target")
        );

        Assert.Equal("path", exception.ParamName);
    }

    // Scenario: reject a root path when a parent context is supplied.
    [Fact]
    public void ValidationContext_Constructor_with_parent_rejects_a_root_path()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new ValidationContext(
                new ValidationContext("parent-target"),
                ValidationPath.Root,
                "target"
            )
        );

        Assert.Equal("path", exception.ParamName);
        Assert.StartsWith(
            "Path cannot be the root path when a parent context is provided.",
            exception.Message
        );
    }

    // Scenario: reject a child path that does not belong to the supplied parent context.
    [Fact]
    public void ValidationContext_Constructor_with_parent_rejects_a_mismatched_child_path()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new ValidationContext(
                new ValidationContext("parent-target"),
                ValidationPath.Root.ForProperty("address").ForProperty("street"),
                "target"
            )
        );

        Assert.Equal("path", exception.ParamName);
        Assert.StartsWith(
            "Path must be a child path when a parent context is provided.",
            exception.Message
        );
    }

    // Scenario: clone the context with a new target while preserving path and parent context.
    [Fact]
    public void ValidationContext_WithTarget_returns_a_new_context_with_the_updated_target()
    {
        var parentContext = new ValidationContext("parent-target");
        var path = ValidationPath.Root.ForProperty("name");
        var original = new ValidationContext(parentContext, path, "original-target");
        var updatedTarget = new object();

        var updated = original.WithTarget(updatedTarget);

        Assert.IsType<ValidationContext>(updated);
        Assert.NotSame(original, updated);
        Assert.Same(original.ParentContext, updated.ParentContext);
        Assert.Same(original.Path, updated.Path);
        Assert.Same(updatedTarget, updated.ValidationTarget);
        Assert.Same("original-target", original.ValidationTarget);
    }
}
