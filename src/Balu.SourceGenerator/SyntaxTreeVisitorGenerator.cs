using Microsoft.CodeAnalysis;

namespace Balu.SourceGenerator;

[Generator]
public sealed class SyntaxTreeVisitorGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }
    public void Execute(GeneratorExecutionContext context)
    {
        context.AddSource(
            "SyntaxTreeVisitor.g.cs",
            "/* SyntaxTreeVisitor to come */");
    }
}