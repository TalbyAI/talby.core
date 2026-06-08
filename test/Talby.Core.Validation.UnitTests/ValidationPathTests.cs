using Talby.Core.Validation;

namespace Talby.Core.Validation.UnitTests;

public sealed class ValidationPathTests
{
    // Scenario: expose the built-in root path as the expected root token and type.
    [Fact]
    public void ValidationPath_Root_exposes_the_expected_root_token_and_type()
    {
        var result = ValidationPath.Root;

        Assert.IsType<ValidationPath.RootPath>(result);
        Assert.Equal(ValidationPath.RootPathString, result.Path);
        Assert.True(result.IsRootPath);
        Assert.False(result.IsChildPath);
        Assert.False(result.IsPropertyPath);
        Assert.False(result.IsIndexPath);
        Assert.Equal(
            "root",
            result.Match(static _ => "root", static _ => "property", static _ => "index")
        );
        Assert.Equal("root", result.MatchRootPath("default", static _ => "root"));
        Assert.Equal("default", result.MatchPropertyPath("default", static _ => "property"));
        Assert.Equal("default", result.MatchIndexPath("default", static _ => "index"));

        var rootActionCalled = false;
        result.MatchRootPath(
            _ => rootActionCalled = true,
            () => Assert.Fail("default action should not run for root paths")
        );

        Assert.True(rootActionCalled);
    }

    // Scenario: trim valid property names and create property paths from the root.
    [Fact]
    public void ValidationPath_ForProperty_trims_the_property_name_and_creates_a_property_path()
    {
        var result = ValidationPath.Root.ForProperty("  display_name  ");

        var propertyPath = Assert.IsType<ValidationPath.PropertyPath>(result);
        Assert.Same(ValidationPath.Root, propertyPath.Parent);
        Assert.Equal("display_name", propertyPath.Property);
        Assert.Equal("$.display_name", propertyPath.Path);
        Assert.False(propertyPath.IsRootPath);
        Assert.True(propertyPath.IsChildPath);
        Assert.True(propertyPath.IsPropertyPath);
        Assert.False(propertyPath.IsIndexPath);
    }

    // Scenario: reject invalid property names before a property path is created.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1name")]
    [InlineData("name-with-dash")]
    public void ValidationPath_ValidatedPropertyName_rejects_invalid_values(string? propertyName)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            ValidationPath.ValidatedPropertyName(propertyName!)
        );

        Assert.Equal("propertyName", exception.ParamName);
    }

    // Scenario: create index paths from the root and keep the validated index value.
    [Fact]
    public void ValidationPath_ForIndex_creates_an_index_path()
    {
        var result = ValidationPath.Root.ForIndex(3);

        var indexPath = Assert.IsType<ValidationPath.IndexPath>(result);
        Assert.Same(ValidationPath.Root, indexPath.Parent);
        Assert.Equal(3, indexPath.Index);
        Assert.Equal("$[3]", indexPath.Path);
        Assert.False(indexPath.IsRootPath);
        Assert.True(indexPath.IsChildPath);
        Assert.False(indexPath.IsPropertyPath);
        Assert.True(indexPath.IsIndexPath);
    }

    // Scenario: reject negative indexes before creating index paths.
    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void ValidationPath_ValidatedIndex_rejects_negative_values(int index)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            ValidationPath.ValidatedIndex(index)
        );

        Assert.Equal("index", exception.ParamName);
    }

    // Scenario: dispatch every matcher branch and keep the defensive default path covered.
    [Fact]
    public void ValidationPath_Matchers_dispatch_known_cases_and_reject_unknown_derived_paths()
    {
        var root = ValidationPath.Root;
        var property = root.ForProperty("name");
        var index = root.ForIndex(1);
        var unknown = new UnknownValidationPath(ValidationPath.Root, "unknown");
        var basePath = new UnknownValidationPath(ValidationPath.Root, "base-path");

        Assert.Equal(
            "root",
            root.Match(static _ => "root", static _ => "property", static _ => "index")
        );
        Assert.Equal(
            "property",
            property.Match(static _ => "root", static _ => "property", static _ => "index")
        );
        Assert.Equal(
            "index",
            index.Match(static _ => "root", static _ => "property", static _ => "index")
        );
        Assert.Throws<InvalidOperationException>(() =>
            unknown.Match(static _ => "root", static _ => "property", static _ => "index")
        );

        Assert.Equal(
            "root",
            root.Match(static _ => "root", static _ => "property", static _ => "index")
        );
        Assert.Equal(
            "property",
            property.Match(static _ => "root", static _ => "property", static _ => "index")
        );
        Assert.Equal(
            "index",
            index.Match(static _ => "root", static _ => "property", static _ => "index")
        );
        Assert.Throws<InvalidOperationException>(() =>
            unknown.Match(static _ => "root", static _ => "property", static _ => "index")
        );

        Assert.Equal("base-path", basePath.Path);
        Assert.False(basePath.IsRootPath);
        Assert.False(basePath.IsChildPath);
        Assert.False(basePath.IsPropertyPath);
        Assert.False(basePath.IsIndexPath);

        Assert.Equal("root", root.MatchRootPath("default", static _ => "root"));
        Assert.Equal("default", property.MatchRootPath("default", static _ => "root"));
        Assert.Equal("default", index.MatchRootPath("default", static _ => "root"));
        Assert.Equal("default", unknown.MatchRootPath("default", static _ => "root"));

        Assert.Equal("default", root.MatchPropertyPath("default", static _ => "property"));
        Assert.Equal("property", property.MatchPropertyPath("default", static _ => "property"));
        Assert.Equal("default", index.MatchPropertyPath("default", static _ => "property"));
        Assert.Equal("default", unknown.MatchPropertyPath("default", static _ => "property"));

        Assert.Equal("default", root.MatchIndexPath("default", static _ => "index"));
        Assert.Equal("default", property.MatchIndexPath("default", static _ => "index"));
        Assert.Equal("index", index.MatchIndexPath("default", static _ => "index"));
        Assert.Equal("default", unknown.MatchIndexPath("default", static _ => "index"));

        var branchLog = new List<string>();
        root.Match(_ => branchLog.Add("root"), _ => branchLog.Add("child"));
        property.Match(_ => branchLog.Add("root"), _ => branchLog.Add("child"));
        index.Match(_ => branchLog.Add("root"), _ => branchLog.Add("child"));

        Assert.Equal(["root", "child", "child"], branchLog);
        Assert.Throws<InvalidOperationException>(() =>
            unknown.Match(_ => branchLog.Add("root"), _ => branchLog.Add("child"))
        );

        var dispatchLog = new List<string>();
        root.Match(
            _ => dispatchLog.Add("root"),
            _ => dispatchLog.Add("property"),
            _ => dispatchLog.Add("index")
        );
        property.Match(
            _ => dispatchLog.Add("root"),
            _ => dispatchLog.Add("property"),
            _ => dispatchLog.Add("index")
        );
        index.Match(
            _ => dispatchLog.Add("root"),
            _ => dispatchLog.Add("property"),
            _ => dispatchLog.Add("index")
        );

        Assert.Equal(["root", "property", "index"], dispatchLog);
        Assert.Throws<InvalidOperationException>(() =>
            unknown.Match(
                _ => dispatchLog.Add("root"),
                _ => dispatchLog.Add("property"),
                _ => dispatchLog.Add("index")
            )
        );

        var rootActionCalled = false;
        root.MatchRootPath(_ => rootActionCalled = true);
        Assert.True(rootActionCalled);

        var propertyDefaultCalled = false;
        property.MatchRootPath(
            _ => Assert.Fail("match action should not run for child paths"),
            () => propertyDefaultCalled = true
        );
        Assert.True(propertyDefaultCalled);

        var baseRootDefaultCalled = false;
        basePath.MatchRootPath(
            _ => Assert.Fail("match action should not run for base paths"),
            () => baseRootDefaultCalled = true
        );
        Assert.True(baseRootDefaultCalled);

        var basePropertyDefaultCalled = false;
        basePath.MatchPropertyPath(
            _ => Assert.Fail("match action should not run for base paths"),
            () => basePropertyDefaultCalled = true
        );
        Assert.True(basePropertyDefaultCalled);

        var baseIndexDefaultCalled = false;
        basePath.MatchIndexPath(
            _ => Assert.Fail("match action should not run for base paths"),
            () => baseIndexDefaultCalled = true
        );
        Assert.True(baseIndexDefaultCalled);

        var propertyMatchCalled = false;
        property.MatchPropertyPath(_ => propertyMatchCalled = true);
        Assert.True(propertyMatchCalled);

        var indexDefaultCalled = false;
        index.MatchPropertyPath(
            _ => Assert.Fail("match action should not run for index paths"),
            () => indexDefaultCalled = true
        );
        Assert.True(indexDefaultCalled);

        var indexMatchCalled = false;
        index.MatchIndexPath(_ => indexMatchCalled = true);
        Assert.True(indexMatchCalled);

        var unknownDefaultCalled = false;
        unknown.MatchIndexPath(
            _ => Assert.Fail("match action should not run for unknown paths"),
            () => unknownDefaultCalled = true
        );
        Assert.True(unknownDefaultCalled);
    }

    private sealed record UnknownValidationPath(ValidationPath Parent, string PathText)
        : ValidationPath(Parent)
    {
        public override string Path => PathText;
    }
}
