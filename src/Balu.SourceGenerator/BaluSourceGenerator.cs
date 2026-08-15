using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Balu.SourceGenerator;

[Generator]
public sealed class BaluSourceGenerator : ISourceGenerator
{
    public const string BoundNodeTypeName = "Balu.Binding.BoundNode";
    public const string BoundNodeKindTypeName = "Balu.Binding.BoundNodeKind";
    public const string SyntaxNodeTypeName = "Balu.Syntax.SyntaxNode";
    public const string SyntaxKindTypeName = "Balu.Syntax.SyntaxKind";
    public const string SyntaxTokenTypeName = "Balu.Syntax.SyntaxToken";
    public const string SeparatedSyntaxListTypeName = "Balu.Syntax.SeparatedSyntaxList`1";
    public const string ImmutableArrayTypeName = "System.Collections.Immutable.ImmutableArray`1";

    static readonly DiagnosticDescriptor MissingTypeDiagnostic = new(id: "BLS0000",
                                                                     title: "Missing type",
                                                                     messageFormat: "The type '{0}' was not found in the compilation.",
                                                                     category: "Balu source generation",
                                                                     DiagnosticSeverity.Error,
                                                                     isEnabledByDefault: true);
    static readonly DiagnosticDescriptor UnsupportedNodeTypeDiagnostic = new(id: "BLS0002",
                                                                               title: "Unsupported node type",
                                                                               messageFormat: "The node type '{0}' must be a non-record, non-generic class declared at namespace scope and must not be file-local.",
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
        var immutableArrayType = FindType(context, ImmutableArrayTypeName);
        var syntaxNodeType = FindType(context, SyntaxNodeTypeName);
        var syntaxNodeKindType = FindType(context, SyntaxKindTypeName);
        var syntaxTokenType = FindType(context, SyntaxTokenTypeName);
        var separatedListType = FindType(context, SeparatedSyntaxListTypeName);

        if (boundNodeType is null || boundNodeKindType is null || immutableArrayType is null || syntaxNodeType is null ||
            syntaxNodeKindType is null || syntaxTokenType is null || separatedListType is null) return;

        var types = compilation.Assembly.GetAllTypes();
        foreach (var type in types.Where(type => !type.IsSupportedNodeType() &&
                                                (type.IsDerivedFrom(syntaxNodeType) || type.IsDerivedFrom(boundNodeType))))
        {
            var location = type.Locations.FirstOrDefault(candidate => candidate.IsInSource);
            context.ReportDiagnostic(Diagnostic.Create(UnsupportedNodeTypeDiagnostic, location, type.ToDisplayString()));
        }

        var syntaxMapping = NodeKindMapping.Create(context,
                                                   types,
                                                   syntaxNodeType,
                                                   syntaxNodeKindType,
                                                   nodePrefix: string.Empty,
                                                   nodeSuffix: "Syntax",
                                                   kind => !(kind.Name.EndsWith("Token", System.StringComparison.Ordinal) ||
                                                             kind.Name.EndsWith("Keyword", System.StringComparison.Ordinal) ||
                                                             kind.Name.EndsWith("Trivia", System.StringComparison.Ordinal)),
                                                   syntaxTokenType);
        var boundMapping = NodeKindMapping.Create(context,
                                                  types,
                                                  boundNodeType,
                                                  boundNodeKindType,
                                                  nodePrefix: "Bound",
                                                  nodeSuffix: string.Empty,
                                                  _ => true);

        var generators = new BaseGenerator[]
        {
            new SyntaxNodeChildrenGenerator(syntaxMapping.Nodes, syntaxNodeType, separatedListType, immutableArrayType),
            new SyntaxTreeVisitorGenerator(syntaxMapping),
            new BoundNodeChildrenGenerator(boundMapping.Nodes, boundNodeType, immutableArrayType),
            new BoundTreeVisitorGenerator(boundMapping),
            new BoundTreeRewriterGenerator(boundMapping, boundNodeType, boundNodeKindType, immutableArrayType)
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
