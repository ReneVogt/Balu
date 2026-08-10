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
    readonly INamedTypeSymbol boundNodeType, boundNodeKindType, immutableArrayType;

    internal BoundTreeRewriterGenerator(CSharpCompilation compilation, INamedTypeSymbol boundNodeType, INamedTypeSymbol boundNodeKindType, INamedTypeSymbol immutableArrayType)
    {
        this.compilation = compilation;
        this.boundNodeType = boundNodeType;
        this.boundNodeKindType = boundNodeKindType;
        this.immutableArrayType = immutableArrayType;
    }

    public override void Generate(GeneratorExecutionContext context)
    {
        var kindNames = boundNodeKindType.MemberNames.ToImmutableArray();
        var types = compilation.Assembly.GetAllTypes();
        var boundNodeTypes = types.Where(t => t.IsSupportedNodeType() && !t.IsAbstract && t.IsDerivedFrom(boundNodeType) && SymbolEqualityComparer.Default.Equals(t.ContainingNamespace, boundNodeType.ContainingNamespace));
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
                var model = NodeModel.Create(type!, context);
                if (model is null) continue;

                var propertiesToRewrite = model.Parameters
                                               .Select(parameter => parameter.Property)
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
                    Writer.Write($" ? node : new {type.Name}(");
                    for (int i = 0; i < model.Parameters.Length; i++)
                    {
                        if (i > 0) Writer.Write(", ");
                        var property = model.Parameters[i].Property;
                        if (propertiesToRewrite.Contains(property))
                            Writer.Write($"rewritten{property.Name}");
                        else
                            Writer.Write($"node.{property.Name}");
                    }
                    Writer.WriteLine(");");
                }
            }
        }


        context.AddSource(
            "BoundTreeRewriter.g.cs",
            SourceText.From(Writer.InnerWriter.ToString(), Encoding.UTF8));
    }
}
