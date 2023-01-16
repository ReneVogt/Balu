using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Balu.SourceGenerator;

[Generator]
public sealed class BaluSourceGenerator : ISourceGenerator
{
    public const string BoundNodeTypeName = "Balu.Binding.BoundNode";
    public const string BoundNodeKindTypeName = "Balu.Binding.BoundNodeKind";
    public const string BoundLoopStatementTypeName = "Balu.Binding.BoundLoopStatement";
    public const string SyntaxNodeTypeName = "Balu.Syntax.SyntaxNode";
    public const string SyntaxKindTypeName = "Balu.Syntax.SyntaxKind";
    public const string SeparatedSyntaxListTypeName = "Balu.Syntax.SeparatedSyntaxList`1";
    public const string ImmutableArrayTypeName = "System.Collections.Immutable.ImmutableArray`1";

    static readonly DiagnosticDescriptor MissingTypeDiagnostic = new(id: "BLS0000",
                                                                     title: "Missing type",
                                                                     messageFormat: "The type '{0}' was not found in the compilation.",
                                                                     category: "Balu source generation",
                                                                     DiagnosticSeverity.Error,
                                                                     isEnabledByDefault: true);

    public void Initialize(GeneratorInitializationContext context)
    {
    }
    public void Execute(GeneratorExecutionContext context)
    {
        var compilation = (CSharpCompilation)context.Compilation;
        var boundNodeType = FindType(context, BoundNodeTypeName);
        var boundNodeKindType = FindType(context, BoundNodeKindTypeName);
        var boundLoopStatementType = FindType(context, BoundLoopStatementTypeName);
        var immutableArrayType = FindType(context, ImmutableArrayTypeName);
        var syntaxNodeType = FindType(context, SyntaxNodeTypeName);
        var syntaxNodeKindType = FindType(context, SyntaxKindTypeName);
        var separatedListType = FindType(context, SeparatedSyntaxListTypeName);

        if (boundNodeType is null || boundNodeKindType is null || boundLoopStatementType is null || immutableArrayType is null || syntaxNodeType is null ||
            syntaxNodeKindType is null || separatedListType is null) return;

        var generators = new BaseGenerator[]
        {
            new SyntaxNodeChildrenGenerator(compilation, syntaxNodeType, separatedListType, immutableArrayType),
            new SyntaxTreeVisitorGenerator(compilation, syntaxNodeType, syntaxNodeKindType), 
            new BoundNodeChildrenGenerator(compilation, boundNodeType, immutableArrayType), 
            new BoundTreeVisitorGenerator(compilation, boundNodeType, boundNodeKindType),
            new BoundTreeRewriterGenerator(compilation, boundNodeType, boundNodeKindType, boundLoopStatementType, immutableArrayType)
        };

        foreach (var generator in generators.TakeWhile(_ => !context.CancellationToken.IsCancellationRequested))
            generator.Generate(context);
    }
    static INamedTypeSymbol? FindType(GeneratorExecutionContext context, string typeName)
    {
        var type = context.Compilation.GetTypeByMetadataName(typeName);
        if (type is null)
            context.ReportDiagnostic(Diagnostic.Create(MissingTypeDiagnostic, null, DiagnosticSeverity.Error, null, null, typeName));
        return type;
    }
}