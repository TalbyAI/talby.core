using Microsoft.CodeAnalysis;

namespace Talby.Core.SourceGenerators;

[Generator]
public sealed class TalbyCoreIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static initializationContext =>
        {
            initializationContext.AddSource(
                GenerateDiscriminatedUnionAttribute.HintName,
                GenerateDiscriminatedUnionAttribute.Source
            );
        });
    }
}
