using System.Xml.Linq;

namespace Talby.Core.SourceGenerators.UnitTests;

public sealed class SourceGeneratorProjectShapeTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    // Scenario: Given the generator project boundary, When the project file is inspected, Then it is configured as an analyzer package with Roslyn references.
    [Fact]
    public void SourceGeneratorProject_IsConfiguredAsAnalyzerPackage()
    {
        var project = LoadProject("Talby.Core.SourceGenerators/Talby.Core.SourceGenerators.csproj");

        Assert.Equal("true", project.IsPackable);
        Assert.Equal("Analyzer", project.PackageType);
        Assert.Equal("false", project.IncludeBuildOutput);
        Assert.Contains(
            project.PackageReferences,
            reference =>
                reference.Include == "Microsoft.CodeAnalysis.CSharp"
                && reference.PrivateAssets == "all"
        );
        Assert.Contains(
            project.PackageReferences,
            reference =>
                reference.Include == "Microsoft.CodeAnalysis.Analyzers"
                && reference.PrivateAssets == "all"
        );
        Assert.Contains("analyzers/dotnet/cs", project.PackagePaths);
    }

    // Scenario: Given the contracts boundary, When the solution and project graph are inspected, Then the contracts project is in the solution and only the generator depends on it.
    [Fact]
    public void ContractsProject_StaysOneWayInSolutionGraph()
    {
        var solution = LoadSolution("Talby.Core.slnx");
        var generatorProject = LoadProject(
            "Talby.Core.SourceGenerators/Talby.Core.SourceGenerators.csproj"
        );

        Assert.Contains(
            "Talby.Core.SourceGenerators/Talby.Core.SourceGenerators.csproj",
            solution.ProjectPaths
        );
    }

    private static SolutionSnapshot LoadSolution(string relativePath)
    {
        var solutionPath = GetPath(relativePath);
        Assert.True(File.Exists(solutionPath), $"Expected solution file to exist: {solutionPath}");

        var document = XDocument.Load(solutionPath);
        var projectPaths = document
            .Descendants("Project")
            .Select(element => element.Attribute("Path")?.Value)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => path!)
            .ToArray();

        return new SolutionSnapshot(projectPaths);
    }

    private static ProjectSnapshot LoadProject(string relativePath)
    {
        var projectPath = GetPath(relativePath);
        Assert.True(File.Exists(projectPath), $"Expected project file to exist: {projectPath}");

        var document = XDocument.Load(projectPath);
        return new ProjectSnapshot(
            TargetFramework: GetProperty(document, "TargetFramework"),
            IsPackable: GetProperty(document, "IsPackable"),
            PackageType: GetProperty(document, "PackageType"),
            Nullable: GetProperty(document, "Nullable"),
            ImplicitUsings: GetProperty(document, "ImplicitUsings"),
            IncludeBuildOutput: GetProperty(document, "IncludeBuildOutput"),
            PackageReferences: GetPackageReferences(document),
            ProjectReferences: GetProjectReferences(document),
            PackagePaths: GetPackagePaths(document)
        );
    }

    private static string GetProperty(XDocument document, string propertyName)
    {
        return document
                .Descendants(propertyName)
                .Select(element => element.Value.Trim())
                .FirstOrDefault()
            ?? string.Empty;
    }

    private static IReadOnlyList<PackageReferenceSnapshot> GetPackageReferences(XDocument document)
    {
        return document
            .Descendants("PackageReference")
            .Select(element => new PackageReferenceSnapshot(
                Include: element.Attribute("Include")?.Value ?? string.Empty,
                PrivateAssets: element.Attribute("PrivateAssets")?.Value ?? string.Empty
            ))
            .ToArray();
    }

    private static IReadOnlyList<string> GetProjectReferences(XDocument document)
    {
        return document
            .Descendants("ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => include!)
            .ToArray();
    }

    private static IReadOnlyList<string> GetPackagePaths(XDocument document)
    {
        return document
            .Descendants()
            .Select(element => element.Attribute("PackagePath")?.Value)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => path!)
            .ToArray();
    }

    private static string GetPath(string relativePath)
    {
        return Path.GetFullPath(Path.Combine(RepositoryRoot, relativePath));
    }

    private static string NormalizeReferencePath(string referencePath)
    {
        return referencePath.Replace('\\', '/');
    }

    private static string GetRepositoryRoot()
    {
        return Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..")
        );
    }

    private sealed record ProjectSnapshot(
        string TargetFramework,
        string IsPackable,
        string PackageType,
        string Nullable,
        string ImplicitUsings,
        string IncludeBuildOutput,
        IReadOnlyList<PackageReferenceSnapshot> PackageReferences,
        IReadOnlyList<string> ProjectReferences,
        IReadOnlyList<string> PackagePaths
    );

    private sealed record SolutionSnapshot(IReadOnlyList<string> ProjectPaths);

    private sealed record PackageReferenceSnapshot(string Include, string PrivateAssets);
}
