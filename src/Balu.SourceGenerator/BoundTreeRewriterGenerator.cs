using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Balu.SourceGenerator;

sealed class BoundTreeRewriterGenerator : BaseGenerator
{

    static readonly DiagnosticDescriptor UnableToConstructBoundNode = new(id: "BLS0001",
                                                                     title: "BoundNode construction failure",
                                                                     messageFormat: "Cannot generate rewrite method for '{0}', constructor arguments do not match.",
                                                                     category: "Balu source generation",
                                                                     DiagnosticSeverity.Error,
                                                                     isEnabledByDefault: true);

    readonly CSharpCompilation compilation;
    readonly INamedTypeSymbol boundNodeType, boundNodeKindType, boundLoopStatementType, immutableArrayType;

    internal BoundTreeRewriterGenerator(CSharpCompilation compilation, INamedTypeSymbol boundNodeType, INamedTypeSymbol boundNodeKindType, INamedTypeSymbol boundLoopStatementType, INamedTypeSymbol immutableArrayType)
    {
        this.compilation = compilation;
        this.boundNodeType = boundNodeType;
        this.boundNodeKindType = boundNodeKindType;
        this.boundLoopStatementType = boundLoopStatementType;
        this.immutableArrayType = immutableArrayType;
    }

    public override void Generate(GeneratorExecutionContext context)
    {
        var kindNames = boundNodeKindType.MemberNames.ToImmutableArray();
        var types = compilation.Assembly.GetAllTypes();
        var boundNodeTypes = types.Where(t => !t.IsAbstract && t.IsDerivedFrom(boundNodeType) && SymbolEqualityComparer.Default.Equals(t.ContainingNamespace, boundNodeType.ContainingNamespace));
        var kindsToVisit = kindNames
                           .Select(kindName => (kind: kindName,
                                                   type: boundNodeTypes.SingleOrDefault(nodeType => nodeType.Name == $"Bound{kindName}")))
                           .Where(x => x.type is not null)
                           .ToImmutableArray();

        Writer.WriteLine("using System;");
        Writer.WriteLine("using System.Collections.Immutable;");
        Writer.WriteLine("using System.Linq;");
        Writer.WriteLine("#nullable enable");
        Writer.WriteLine();
        Writer.WriteLine("namespace Balu.Binding;");
        Writer.WriteLine();
        using(new CurlyIndenter(Writer, "abstract class BoundTreeRewriter"))
        {
            using(new CurlyIndenter(Writer, "public virtual BoundNode Visit(BoundNode node) => node.Kind switch", semicolon: true))
            {
                foreach (var (kind, type) in kindsToVisit)
                    Writer.WriteLine($"BoundNodeKind.{kind} => VisitBound{kind}(({type!.Name})node),");

                Writer.WriteLine("_ => throw new ArgumentException($\"Unexpected bound node kind '{node.Kind}'.\")");
            }

            Writer.WriteLine(@"
    private protected ImmutableArray<T> RewriteList<T>(ImmutableArray<T> nodes) where T : BoundNode
    {
        ImmutableArray<T>.Builder? resultBuilder = null;
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = (T)Visit(nodes[i]);
            if (node != nodes[i] && resultBuilder is null)
            {
                resultBuilder = ImmutableArray.CreateBuilder<T>(nodes.Length);
                resultBuilder.AddRange(nodes.Take(i));
            }
            resultBuilder?.Add(node);
        }

        return resultBuilder?.ToImmutable() ?? nodes;
    }
");

            foreach (var (kind, type) in kindsToVisit)
            {
                var propertiesToRewrite = type!.GetMembers()
                                      .OfType<IPropertySymbol>()
                                      .Where(property => property.Type is INamedTypeSymbol propertyType &&
                                                         (propertyType.IsDerivedFrom(boundNodeType) ||
                                                          propertyType.IsGenericListOf(immutableArrayType, boundNodeType)))
                                      .ToImmutableArray();

                if (propertiesToRewrite.Length == 0)
                {
                    Writer.WriteLine($"protected virtual BoundNode VisitBound{kind}({type.Name} node) => node;");
                    continue;
                }

                using(new CurlyIndenter(Writer, $"protected virtual BoundNode VisitBound{kind}({type.Name} node)"))
                {
                    foreach (var property in propertiesToRewrite)
                    {
                        if (((INamedTypeSymbol)property.Type).IsDerivedFrom(boundNodeType))
                        {
                            if (property.NullableAnnotation == NullableAnnotation.Annotated)
                                Writer.WriteLine(
                                    $"var rewritten{property.Name} = node.{property.Name} is null ? null : ({property.Type.Name})Visit(node.{property.Name});");
                            else
                                Writer.WriteLine($"var rewritten{property.Name} = ({property.Type.Name})Visit(node.{property.Name});");
                        }
                        else
                            Writer.WriteLine($"var rewritten{property.Name} = RewriteList(node.{property.Name});");
                    }

                    Writer.WriteLine();
                    Writer.Write("return ");
                    Writer.Write(string.Join(" && ", propertiesToRewrite.Select(property => $"node.{property.Name} == rewritten{property.Name}")));
                    Writer.Write($" ? node : new {type.Name}(node.Syntax");

                    var allProperties = type.GetMembers()
                                            .OfType<IPropertySymbol>()
                                            .Where(property => property.Type is INamedTypeSymbol)
                                            .ToImmutableArray();
                    var constructorParameters = type.InstanceConstructors.First(ctor => !ctor.IsOverride).Parameters;
                    int propertyIndex, argumentIndex;
                    for (propertyIndex = 0, argumentIndex = 1;
                         propertyIndex < allProperties.Length && argumentIndex < constructorParameters.Length; propertyIndex++)
                    {
                        var property = allProperties[propertyIndex];
                        if (!SymbolEqualityComparer.Default.Equals(property.Type, constructorParameters[argumentIndex].Type))
                            continue;
                        Writer.Write(", ");
                        if (propertiesToRewrite.Contains(property))
                            Writer.Write($"rewritten{property.Name}");
                        else
                            Writer.Write($"node.{property.Name}");
                        argumentIndex++;
                    }

                    if (type.IsDerivedFrom(boundLoopStatementType))
                    {
                        Writer.Write(", node.BreakLabel, node.ContinueLabel");
                        argumentIndex += 2;
                    }

                    if (argumentIndex != constructorParameters.Length || allProperties.Skip(propertyIndex).Any(propertiesToRewrite.Contains))
                        context.ReportDiagnostic(Diagnostic.Create(UnableToConstructBoundNode, null, DiagnosticSeverity.Error, null, null, type.Name));

                    Writer.WriteLine(");");
                }
            }
        }


        context.AddSource(
            "BoundTreeRewriter.g.cs",
            SourceText.From(Writer.InnerWriter.ToString(), Encoding.UTF8));
    }
}
