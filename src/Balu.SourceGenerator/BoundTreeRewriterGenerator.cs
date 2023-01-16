using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Balu.SourceGenerator;

sealed class BoundTreeRewriterGenerator : BaseGenerator
{
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
                    Writer.WriteLine($"BoundNodeKind.{kind} => VisitBound{kind}(({type!.GetFullName()})node),");

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
                var properties = type!.GetMembers()
                                      .OfType<IPropertySymbol>()
                                      .Where(property => property.Type is INamedTypeSymbol propertyType &&
                                                         (propertyType.IsDerivedFrom(boundNodeType) ||
                                                          propertyType.IsGenericListOf(immutableArrayType, boundNodeType)))
                                      .ToImmutableArray();

                if (properties.Length == 0)
                {
                    Writer.WriteLine($"protected virtual BoundNode VisitBound{kind}({type.GetFullName()} node) => node;");
                    continue;
                }

                using(new CurlyIndenter(Writer, $"protected virtual BoundNode VisitBound{kind}({type.GetFullName()} node)"))
                {
                    foreach (var property in properties)
                    {
                        if (((INamedTypeSymbol)property.Type).IsDerivedFrom(boundNodeType))
                        {
                            if (property.NullableAnnotation == NullableAnnotation.Annotated)
                                Writer.WriteLine(
                                    $"var rewritten{property.Name} = node.{property.Name} is null ? null : ({property.Type.GetFullName()})Visit(node.{property.Name});");
                            else
                                Writer.WriteLine($"var rewritten{property.Name} = ({property.Type.GetFullName()})Visit(node.{property.Name});");
                        }
                        else
                            Writer.WriteLine($"var rewritten{property.Name} = RewriteList(node.{property.Name});");
                    }

                    Writer.WriteLine();
                    Writer.Write("return ");
                    Writer.Write(string.Join(" && ", properties.Select(property => $"node.{property.Name} == rewritten{property.Name}")));
                    Writer.Write($" ? node : new {type.GetFullName()}(node.Syntax");

                    var allProperties = type.GetMembers()
                                            .OfType<IPropertySymbol>()
                                            .Where(property => property.Type is INamedTypeSymbol)
                                            .ToImmutableArray();
                    var constructorParameter = type.InstanceConstructors.First(ctor => ctor.Parameters.Length > 1).Parameters;
                    int arguments = 1;
                    foreach (IPropertySymbol property in allProperties.Where(property => SymbolEqualityComparer.Default.Equals(property.Type, constructorParameter[arguments].Type)))
                    {
                        arguments++;

                        Writer.Write(", ");
                        if (properties.Contains(property))
                            Writer.Write($"rewritten{property.Name}");
                        else
                            Writer.Write($"node.{property.Name}");

                        if (arguments >= constructorParameter.Length) break;
                    }

                    if (type.IsDerivedFrom(boundLoopStatementType))
                        Writer.Write(", node.BreakLabel, node.ContinueLabel");

                    Writer.WriteLine(");");
                }
            }
        }


        context.AddSource(
            "BoundTreeRewriter.g.cs",
            SourceText.From(Writer.InnerWriter.ToString(), Encoding.UTF8));
    }
}
