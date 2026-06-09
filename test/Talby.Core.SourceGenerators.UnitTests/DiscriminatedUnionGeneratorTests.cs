using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Talby.Core.SourceGenerators.UnitTests;

public sealed class DiscriminatedUnionGeneratorTests
{
    private const string AttributeHintName = "GenerateDiscriminatedUnionAttribute.g.cs";

    private static readonly MetadataReference[] MetadataReferences = LoadMetadataReferences();

    // Scenario: Given the generator runs on any compilation, when it executes, then the marker attribute is emitted as post-initialization source.
    [Fact]
    public void GenerateDiscriminatedUnionAttribute_IsEmittedAsPostInitializationSource()
    {
        var result = RunGenerator(
            ("MarkerHost.cs", "namespace Demo; public sealed class MarkerHost { }")
        );

        var markerSource = GetGeneratedSource(result, AttributeHintName);

        Assert.Contains(
            "internal sealed class GenerateDiscriminatedUnionAttribute : System.Attribute",
            markerSource
        );
        Assert.Contains(
            "[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct",
            markerSource
        );
        Assert.Empty(
            result
                .OutputCompilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
        );
    }

    // Scenario: Given a ValidationPath-shaped union, when the generator runs, then it emits the branch-aware root Match surface and keeps the documentation contract.
    [Fact]
    public void ValidationPath_Surface_matches_the_branch_aware_contract()
    {
        var result = RunGenerator(
            ("MarkerStub.cs", AttributeStubSource),
            ("ValidationPath.cs", ValidationPathSource)
        );
        var unionSource = GetUnionSource(result, "ValidationPath");

        Assert.Contains("/// <summary>", unionSource);
        Assert.Contains(
            "Provides generated discriminated-union members for ValidationPath.",
            unionSource
        );
        Assert.Contains("Gets whether this instance is a RootPath.", unionSource);
        Assert.Contains(
            "Invokes the matching action for the RootPath, PropertyPath, IndexPath, ChildPath cases.",
            unionSource
        );
        Assert.Contains(
            "public bool IsRootPath => this is global::Talby.Core.Validation.ValidationPath.RootPath;",
            unionSource
        );
        Assert.Contains(
            "public bool IsChildPath => this is global::Talby.Core.Validation.ValidationPath.ChildPath;",
            unionSource
        );
        Assert.Contains(
            "public bool IsPropertyPath => this is global::Talby.Core.Validation.ValidationPath.PropertyPath;",
            unionSource
        );
        Assert.Contains(
            "public bool IsIndexPath => this is global::Talby.Core.Validation.ValidationPath.IndexPath;",
            unionSource
        );
        Assert.Contains("public TResult Match<TResult>(", unionSource);
        Assert.Contains("public void Match(", unionSource);
        Assert.Contains(
            "global::System.Func<global::Talby.Core.Validation.ValidationPath.RootPath, TResult> onRootPath",
            unionSource
        );
        Assert.Contains(
            "global::System.Func<global::Talby.Core.Validation.ValidationPath.PropertyPath, TResult>? onPropertyPath = null",
            unionSource
        );
        Assert.Contains(
            "global::System.Func<global::Talby.Core.Validation.ValidationPath.IndexPath, TResult>? onIndexPath = null",
            unionSource
        );
        Assert.Contains(
            "global::System.Func<global::Talby.Core.Validation.ValidationPath.ChildPath, TResult>? onChildPath = null",
            unionSource
        );
        Assert.Contains(
            "global::System.Action<global::Talby.Core.Validation.ValidationPath.RootPath> onRootPath",
            unionSource
        );
        Assert.Contains(
            "global::System.Action<global::Talby.Core.Validation.ValidationPath.PropertyPath>? onPropertyPath = null",
            unionSource
        );
        Assert.Contains(
            "global::System.Action<global::Talby.Core.Validation.ValidationPath.IndexPath>? onIndexPath = null",
            unionSource
        );
        Assert.Contains(
            "global::System.Action<global::Talby.Core.Validation.ValidationPath.ChildPath>? onChildPath = null",
            unionSource
        );
        Assert.DoesNotContain("MatchChildPath", unionSource);
        Assert.DoesNotContain("matchFunc", unionSource);
        Assert.DoesNotContain("matchAction", unionSource);
        Assert.Contains("if (onChildPath is not null)", unionSource);
        Assert.Contains("onPropertyPath is null", unionSource);
        Assert.Contains("onIndexPath is null", unionSource);
        Assert.Contains("Invalid ValidationPath Match handler combination.", unionSource);
        Assert.Contains(
            "throw new global::System.InvalidOperationException(\"Unknown ValidationPath type.\")",
            unionSource
        );

        AssertOrder(
            unionSource,
            "public bool IsRootPath",
            "public bool IsChildPath",
            "public bool IsPropertyPath",
            "public bool IsIndexPath"
        );
        AssertOrder(
            unionSource,
            "global::System.Func<global::Talby.Core.Validation.ValidationPath.RootPath, TResult> onRootPath",
            "global::System.Func<global::Talby.Core.Validation.ValidationPath.PropertyPath, TResult>? onPropertyPath = null",
            "global::System.Func<global::Talby.Core.Validation.ValidationPath.IndexPath, TResult>? onIndexPath = null",
            "global::System.Func<global::Talby.Core.Validation.ValidationPath.ChildPath, TResult>? onChildPath = null"
        );
    }

    // Scenario: Given a nested type that inherits indirectly from the annotated root, when the generator runs, then it participates in the union tree as a valid case.
    [Fact]
    public void NestedTypes_WithIndirectInheritance_AreIncludedInTheUnionTree()
    {
        var result = RunGenerator(
            ("MarkerStub.cs", AttributeStubSource),
            ("Shape.cs", IndirectInheritanceSource)
        );
        var unionSource = GetUnionSource(result, "Shape");

        Assert.Contains("public bool IsNode => this is global::Demo.Shape.Node;", unionSource);
        Assert.Contains(
            "public bool IsLeafOne => this is global::Demo.Shape.LeafOne;",
            unionSource
        );
        Assert.Contains(
            "public bool IsLeafTwo => this is global::Demo.Shape.LeafTwo;",
            unionSource
        );
        Assert.Contains(
            "public TResult MatchLeafOne<TResult>(TResult defaultValue, global::System.Func<global::Demo.Shape.LeafOne, TResult> onMatch)",
            unionSource
        );
        Assert.Contains(
            "public TResult MatchLeafTwo<TResult>(TResult defaultValue, global::System.Func<global::Demo.Shape.LeafTwo, TResult> onMatch)",
            unionSource
        );
        Assert.DoesNotContain("MatchNode", unionSource);
    }

    // Scenario: Given a nested type that does not inherit from the annotated root, when the generator runs, then it is ignored while valid cases still generate.
    [Fact]
    public void NestedTypes_ThatDoNotInheritFromRoot_AreIgnoredWhenValidCasesExist()
    {
        var result = RunGenerator(
            ("MarkerStub.cs", AttributeStubSource),
            ("Envelope.cs", InvalidNestedWithValidCaseSource)
        );
        var unionSource = GetUnionSource(result, "Envelope");

        Assert.Contains(
            "public bool IsValidCase => this is global::Demo.Envelope.ValidCase;",
            unionSource
        );
        Assert.DoesNotContain("IsInvalidCase", unionSource);
        Assert.DoesNotContain("MatchInvalidCase", unionSource);
        Assert.Single(GetUnionSources(result));
    }

    // Scenario: Given an annotated root whose nested types never qualify as cases, when the generator runs, then it reports the zero-valid-cases diagnostic and emits no union file.
    [Fact]
    public void NestedTypes_ThatDoNotInheritFromRoot_ReportDiagnosticWhenNoValidCasesExist()
    {
        var result = RunGenerator(
            ("MarkerStub.cs", AttributeStubSource),
            ("Envelope.cs", InvalidNestedOnlySource)
        );

        Assert.Empty(GetUnionSources(result));
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Id == "TCSG001");
    }

    // Scenario: Given an annotated root nested inside another type, when the generator runs, then it reports an error and emits no union file.
    [Fact]
    public void NestedAnnotatedRoots_AreRejected_WithADiagnostic()
    {
        var result = RunGenerator(
            ("MarkerStub.cs", AttributeStubSource),
            ("NestedRoot.cs", NestedAnnotatedRootSource)
        );

        Assert.Empty(GetUnionSources(result));
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Id == "TCSG002");
    }

    // Scenario: Given two annotated roots, when the generator runs, then each root receives its own generated file.
    [Fact]
    public void TwoAnnotatedRoots_ProduceOneGeneratedFilePerRoot()
    {
        var result = RunGenerator(
            ("MarkerStub.cs", AttributeStubSource),
            ("FirstRoot.cs", FirstRootSource),
            ("SecondRoot.cs", SecondRootSource)
        );

        var unionSources = GetUnionSources(result);

        Assert.Equal(2, unionSources.Length);
        Assert.Contains(
            unionSources,
            source => source.Contains("partial record FirstRoot", StringComparison.Ordinal)
        );
        Assert.Contains(
            unionSources,
            source => source.Contains("partial record SecondRoot", StringComparison.Ordinal)
        );
        Assert.Contains(
            "GenerateDiscriminatedUnionAttribute",
            GetGeneratedSource(result, AttributeHintName)
        );
    }

    private static string AttributeStubSource =>
        """
            using System;

            namespace Talby.Core.SourceGenerators;

            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
            internal sealed class GenerateDiscriminatedUnionAttribute : Attribute
            {
            }
            """;

    private static string FirstRootSource =>
        """
            using Talby.Core.SourceGenerators;

            namespace Demo;

            [global::Talby.Core.SourceGenerators.GenerateDiscriminatedUnion]
            public abstract partial record FirstRoot
            {
                public record FirstLeaf : FirstRoot;
            }
            """;

    private static string SecondRootSource =>
        """
            using Talby.Core.SourceGenerators;

            namespace Demo;

            [global::Talby.Core.SourceGenerators.GenerateDiscriminatedUnion]
            public abstract partial record SecondRoot
            {
                public record SecondLeaf : SecondRoot;
            }
            """;

    private static string IndirectInheritanceSource =>
        """
            using Talby.Core.SourceGenerators;

            namespace Demo;

            [global::Talby.Core.SourceGenerators.GenerateDiscriminatedUnion]
            public abstract partial record Shape
            {
                public abstract record Node : Shape;

                public record LeafOne : Node;

                public record LeafTwo : Node;
            }
            """;

    private static string InvalidNestedOnlySource =>
        """
            using Talby.Core.SourceGenerators;

            namespace Demo;

            [global::Talby.Core.SourceGenerators.GenerateDiscriminatedUnion]
            public abstract partial record Envelope
            {
                public record InvalidCase;
            }
            """;

    private static string NestedAnnotatedRootSource =>
        """
            using Talby.Core.SourceGenerators;

            namespace Demo;

            public sealed class Container
            {
                [global::Talby.Core.SourceGenerators.GenerateDiscriminatedUnion]
                public abstract partial record NestedRoot
                {
                    public record Leaf : NestedRoot;
                }
            }
            """;

    private static string InvalidNestedWithValidCaseSource =>
        """
            using Talby.Core.SourceGenerators;

            namespace Demo;

            [global::Talby.Core.SourceGenerators.GenerateDiscriminatedUnion]
            public abstract partial record Envelope
            {
                public record ValidCase : Envelope;

                public record InvalidCase;
            }
            """;

    private static string ValidationPathSource =>
        """
            using Talby.Core.SourceGenerators;

            namespace Talby.Core.Validation;

            [global::Talby.Core.SourceGenerators.GenerateDiscriminatedUnion]
            public abstract partial record ValidationPath
            {
                public abstract string Path { get; }

                public const string RootPathString = "$";

                public record RootPath() : ValidationPath
                {
                    public override string Path => RootPathString;
                }

                public abstract record ChildPath(ValidationPath Parent) : ValidationPath;

                public record PropertyPath(ValidationPath Parent, string Property) : ChildPath(Parent);

                public record IndexPath(ValidationPath Parent, int Index) : ChildPath(Parent);
            }
            """;

    private static GeneratorRunSnapshot RunGenerator(params (string Path, string Source)[] sources)
    {
        var generator = new DiscriminatedUnionGenerator();
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CreateCompilation(sources, parseOptions);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [generator.AsSourceGenerator()],
            parseOptions: parseOptions
        );

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics
        );

        return new GeneratorRunSnapshot(
            outputCompilation,
            diagnostics,
            driver.GetRunResult().Results.Single()
        );
    }

    private static CSharpCompilation CreateCompilation(
        (string Path, string Source)[] sources,
        CSharpParseOptions parseOptions
    )
    {
        var syntaxTrees = sources.Select(source =>
            CSharpSyntaxTree.ParseText(source.Source, parseOptions, source.Path)
        );

        return CSharpCompilation.Create(
            assemblyName: "DiscriminatedUnionGeneratorTests",
            syntaxTrees: syntaxTrees,
            references: MetadataReferences,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
    }

    private static string GetGeneratedSource(GeneratorRunSnapshot result, string hintName)
    {
        return result
            .GeneratedSources.Single(source => source.HintName == hintName)
            .SourceText.ToString();
    }

    private static string GetUnionSource(GeneratorRunSnapshot result, string rootName)
    {
        return GetUnionSources(result)
            .Single(source =>
                source.Contains($"partial record {rootName}", StringComparison.Ordinal)
            );
    }

    private static string[] GetUnionSources(GeneratorRunSnapshot result)
    {
        return result
            .GeneratedSources.Where(source => source.HintName != AttributeHintName)
            .Select(source => source.SourceText.ToString())
            .ToArray();
    }

    private static MetadataReference[] LoadMetadataReferences()
    {
        return ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();
    }

    private static void AssertOrder(string input, params string[] expectedFragments)
    {
        var searchStart = 0;

        foreach (var fragment in expectedFragments)
        {
            var index = input.IndexOf(fragment, searchStart, StringComparison.Ordinal);

            Assert.True(index >= 0, $"Expected to find '{fragment}' after index {searchStart}.");
            searchStart = index + fragment.Length;
        }
    }

    private sealed record GeneratorRunSnapshot(
        Compilation OutputCompilation,
        ImmutableArray<Diagnostic> Diagnostics,
        GeneratorRunResult RunResult
    )
    {
        public ImmutableArray<GeneratedSourceResult> GeneratedSources => RunResult.GeneratedSources;
    }
}
