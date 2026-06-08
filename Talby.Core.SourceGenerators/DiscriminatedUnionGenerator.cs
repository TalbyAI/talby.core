using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Talby.Core.SourceGenerators;

/// <summary>
/// Discovers discriminated-union roots and classifies their nested cases.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class DiscriminatedUnionGenerator : IIncrementalGenerator
{
    private const string AttributeMetadataName =
        "Talby.Core.SourceGenerators.GenerateDiscriminatedUnionAttribute";

    private static readonly DiagnosticDescriptor NoValidCasesDiagnostic = new(
        id: "TCSG001",
        title: "Annotated discriminated union has no valid cases",
        messageFormat: "Type '{0}' is annotated with [GenerateDiscriminatedUnion] but has no valid nested cases",
        category: "Talby.Core.SourceGenerators",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor NestedRootDiagnostic = new(
        id: "TCSG002",
        title: "Annotated discriminated union root must not be nested",
        messageFormat: "Type '{0}' is annotated with [GenerateDiscriminatedUnion] but is nested inside '{1}'",
        category: "Talby.Core.SourceGenerators",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static postInitializationContext =>
            postInitializationContext.AddSource(
                GenerateDiscriminatedUnionAttribute.HintName,
                SourceText.From(GenerateDiscriminatedUnionAttribute.Source, Encoding.UTF8)
            )
        );

        var annotatedRoots = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeMetadataName,
            static (syntaxNode, _) => syntaxNode is TypeDeclarationSyntax,
            static (syntaxContext, cancellationToken) =>
                DiscriminatedUnionRootModel.Create(
                    syntaxContext.SemanticModel.Compilation,
                    (INamedTypeSymbol)syntaxContext.TargetSymbol,
                    cancellationToken
                )
        );

        context.RegisterSourceOutput(
            annotatedRoots,
            static (sourceProductionContext, rootModel) =>
            {
                if (rootModel.RootSymbol.ContainingType is not null)
                {
                    var location =
                        rootModel.RootSymbol.Locations.FirstOrDefault(location =>
                            location.IsInSource
                        ) ?? Location.None;

                    sourceProductionContext.ReportDiagnostic(
                        Diagnostic.Create(
                            NestedRootDiagnostic,
                            location,
                            rootModel.RootSymbol.ToDisplayString(
                                SymbolDisplayFormat.MinimallyQualifiedFormat
                            ),
                            rootModel.RootSymbol.ContainingType.ToDisplayString(
                                SymbolDisplayFormat.MinimallyQualifiedFormat
                            )
                        )
                    );

                    return;
                }

                if (rootModel.ValidCases.IsEmpty)
                {
                    var location =
                        rootModel.RootSymbol.Locations.FirstOrDefault(location =>
                            location.IsInSource
                        ) ?? Location.None;

                    sourceProductionContext.ReportDiagnostic(
                        Diagnostic.Create(
                            NoValidCasesDiagnostic,
                            location,
                            rootModel.RootSymbol.ToDisplayString(
                                SymbolDisplayFormat.MinimallyQualifiedFormat
                            )
                        )
                    );

                    return;
                }

                sourceProductionContext.AddSource(
                    DiscriminatedUnionSourceWriter.GetHintName(rootModel.RootSymbol),
                    SourceText.From(
                        DiscriminatedUnionSourceWriter.Generate(rootModel),
                        Encoding.UTF8
                    )
                );
            }
        );
    }
}

internal sealed record DiscriminatedUnionCaseModel(
    INamedTypeSymbol Symbol,
    bool IsValidCase,
    DiscriminatedUnionInheritanceKind InheritanceKind,
    int DiscoveryIndex
);

internal sealed record DiscriminatedUnionRootModel(
    INamedTypeSymbol RootSymbol,
    ImmutableArray<DiscriminatedUnionCaseModel> AllCases,
    ImmutableArray<DiscriminatedUnionCaseModel> ValidCases,
    ImmutableArray<DiscriminatedUnionCaseModel> LeafCases
)
{
    public bool HasValidCases => !ValidCases.IsEmpty;

    internal static DiscriminatedUnionRootModel Create(
        Compilation compilation,
        INamedTypeSymbol rootSymbol,
        CancellationToken cancellationToken
    )
    {
        var allCases = new List<DiscriminatedUnionCaseModel>();
        var discoveredSymbols = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var traversedSymbols = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var discoveryIndex = 0;

        TraverseTypeSymbol(
            compilation,
            rootSymbol,
            rootSymbol,
            allCases,
            discoveredSymbols,
            traversedSymbols,
            ref discoveryIndex,
            cancellationToken
        );

        var allCasesArray = allCases.ToImmutableArray();
        var validCases = allCasesArray.Where(caseModel => caseModel.IsValidCase).ToImmutableArray();
        var leafCases = GetLeafCases(validCases);

        return new DiscriminatedUnionRootModel(rootSymbol, allCasesArray, validCases, leafCases);
    }

    private static ImmutableArray<DiscriminatedUnionCaseModel> GetLeafCases(
        ImmutableArray<DiscriminatedUnionCaseModel> validCases
    )
    {
        if (validCases.IsDefaultOrEmpty)
        {
            return ImmutableArray<DiscriminatedUnionCaseModel>.Empty;
        }

        var leafCases = ImmutableArray.CreateBuilder<DiscriminatedUnionCaseModel>();

        foreach (var candidateCase in validCases)
        {
            var hasValidDescendant = validCases.Any(otherCase =>
                !SymbolEqualityComparer.Default.Equals(candidateCase.Symbol, otherCase.Symbol)
                && InheritsFrom(otherCase.Symbol, candidateCase.Symbol)
            );

            if (!hasValidDescendant)
            {
                leafCases.Add(candidateCase);
            }
        }

        return leafCases.ToImmutable();
    }

    private static void TraverseTypeSymbol(
        Compilation compilation,
        INamedTypeSymbol currentSymbol,
        INamedTypeSymbol rootSymbol,
        List<DiscriminatedUnionCaseModel> allCases,
        HashSet<INamedTypeSymbol> discoveredSymbols,
        HashSet<INamedTypeSymbol> traversedSymbols,
        ref int discoveryIndex,
        CancellationToken cancellationToken
    )
    {
        if (!traversedSymbols.Add(currentSymbol))
        {
            return;
        }

        foreach (var declaration in GetDeclarationsInSourceOrder(currentSymbol, cancellationToken))
        {
            var semanticModel = compilation.GetSemanticModel(declaration.SyntaxTree);

            foreach (var memberDeclaration in declaration.Members)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (memberDeclaration is not TypeDeclarationSyntax nestedTypeDeclaration)
                {
                    continue;
                }

                if (
                    semanticModel.GetDeclaredSymbol(nestedTypeDeclaration, cancellationToken)
                    is not INamedTypeSymbol nestedSymbol
                )
                {
                    continue;
                }

                var inheritanceKind = GetInheritanceKind(nestedSymbol, rootSymbol);
                var caseModel = new DiscriminatedUnionCaseModel(
                    nestedSymbol,
                    inheritanceKind is not null,
                    inheritanceKind ?? DiscriminatedUnionInheritanceKind.Direct,
                    discoveryIndex++
                );

                if (discoveredSymbols.Add(nestedSymbol))
                {
                    allCases.Add(caseModel);
                }

                TraverseTypeSymbol(
                    compilation,
                    nestedSymbol,
                    rootSymbol,
                    allCases,
                    discoveredSymbols,
                    traversedSymbols,
                    ref discoveryIndex,
                    cancellationToken
                );
            }
        }
    }

    private static IEnumerable<TypeDeclarationSyntax> GetDeclarationsInSourceOrder(
        INamedTypeSymbol symbol,
        CancellationToken cancellationToken
    )
    {
        return symbol
            .DeclaringSyntaxReferences.Select(reference => reference.GetSyntax(cancellationToken))
            .OfType<TypeDeclarationSyntax>()
            .OrderBy(
                static declaration => declaration.SyntaxTree?.FilePath ?? string.Empty,
                StringComparer.Ordinal
            )
            .ThenBy(static declaration => declaration.SpanStart)
            .ThenBy(static declaration => declaration.Span.Length);
    }

    private static DiscriminatedUnionInheritanceKind? GetInheritanceKind(
        INamedTypeSymbol candidateSymbol,
        INamedTypeSymbol rootSymbol
    )
    {
        var rootDefinition = rootSymbol.OriginalDefinition;

        if (
            candidateSymbol.BaseType is not null
            && SymbolEqualityComparer.Default.Equals(
                candidateSymbol.BaseType.OriginalDefinition,
                rootDefinition
            )
        )
        {
            return DiscriminatedUnionInheritanceKind.Direct;
        }

        for (
            var currentBaseType = candidateSymbol.BaseType;
            currentBaseType is not null;
            currentBaseType = currentBaseType.BaseType
        )
        {
            if (
                !SymbolEqualityComparer.Default.Equals(
                    currentBaseType.OriginalDefinition,
                    rootDefinition
                )
            )
            {
                continue;
            }

            return DiscriminatedUnionInheritanceKind.Indirect;
        }

        return null;
    }

    private static bool InheritsFrom(INamedTypeSymbol candidateSymbol, INamedTypeSymbol baseSymbol)
    {
        return GetInheritanceKind(candidateSymbol, baseSymbol) is not null;
    }
}

internal enum DiscriminatedUnionInheritanceKind
{
    Direct,
    Indirect,
}
