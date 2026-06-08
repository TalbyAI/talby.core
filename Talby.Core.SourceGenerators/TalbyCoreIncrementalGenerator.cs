using Microsoft.CodeAnalysis;

namespace Talby.Core.SourceGenerators;

[Generator]
public sealed class TalbyCoreIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.CompilationProvider;

        context.RegisterSourceOutput(pipeline, static (_, _) => { });
    }
}
