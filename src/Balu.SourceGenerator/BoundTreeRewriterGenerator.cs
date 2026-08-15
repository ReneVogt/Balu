using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Balu.SourceGenerator;

sealed class BoundTreeRewriterGenerator : BaseGenerator
{

    readonly NodeKindMapping mapping;
    readonly INamedTypeSymbol boundNodeType, boundNodeKindType, immutableArrayType;

    internal BoundTreeRewriterGenerator(NodeKindMapping mapping, INamedTypeSymbol boundNodeType, INamedTypeSymbol boundNodeKindType, INamedTypeSymbol immutableArrayType)
    {
        this.mapping = mapping;
        this.boundNodeType = boundNodeType;
        this.boundNodeKindType = boundNodeKindType;
        this.immutableArrayType = immutableArrayType;
    }

    public override void Generate(GeneratorExecutionContext context)
    {
        var kindsToVisit = ImmutableArray.CreateBuilder<(IFieldSymbol Kind, INamedTypeSymbol Type, NodeModel Model, ImmutableArray<IPropertySymbol> PropertiesToRewrite)>();
        foreach (var (kind, type) in mapping.Mappings)
        {
            var model = NodeModel.Create(type, context);
            if (model is null) continue;

            var propertiesToRewrite = model.Parameters
                                           .Select(parameter => parameter.Property)
                                           .Where(property => property.Type is INamedTypeSymbol propertyType &&
                                                              (propertyType.IsDerivedFrom(boundNodeType) ||
                                                               propertyType.IsGenericListOf(immutableArrayType, boundNodeType)))
                                           .ToImmutableArray();
            if (propertiesToRewrite.Length > 0 &&
                !model.ValidateAccessibility(context,
                                             boundNodeKindType,
                                             constructorRequired: true,
                                             model.Parameters.Select(parameter => parameter.Property).ToImmutableArray(),
                                             "the generated bound tree rewriter"))
                continue;

            kindsToVisit.Add((kind, type, model, propertiesToRewrite));
        }

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
                foreach (var (kind, type, _, _) in kindsToVisit)
                    Writer.WriteLine($"BoundNodeKind.{kind.ToIdentifier()} => {$"VisitBound{kind.Name}".ToIdentifier()}(({type.ToFullyQualifiedName()})node),");

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

            foreach (var (kind, type, model, propertiesToRewrite) in kindsToVisit)
            {
                if (propertiesToRewrite.Length == 0)
                {
                    Writer.WriteLine($"protected virtual BoundNode {$"VisitBound{kind.Name}".ToIdentifier()}({type.ToFullyQualifiedName()} node) => node;");
                    continue;
                }

                using(new CurlyIndenter(Writer, $"protected virtual BoundNode {$"VisitBound{kind.Name}".ToIdentifier()}({type.ToFullyQualifiedName()} node)"))
                {
                    foreach (var property in propertiesToRewrite)
                    {
                        var propertyName = property.Name.ToIdentifier();
                        var rewrittenName = $"rewritten{property.Name}".ToIdentifier();
                        if (((INamedTypeSymbol)property.Type).IsDerivedFrom(boundNodeType))
                        {
                            if (property.NullableAnnotation == NullableAnnotation.Annotated)
                                Writer.WriteLine(
                                    $"var {rewrittenName} = node.{propertyName} is null ? null : ({property.Type.ToFullyQualifiedName()})Visit(node.{propertyName});");
                            else
                                Writer.WriteLine($"var {rewrittenName} = ({property.Type.ToFullyQualifiedName()})Visit(node.{propertyName});");
                        }
                        else
                            Writer.WriteLine($"var {rewrittenName} = RewriteList(node.{propertyName});");
                    }

                    Writer.WriteLine();
                    Writer.Write("return ");
                    Writer.Write(string.Join(" && ", propertiesToRewrite.Select(property => $"node.{property.Name.ToIdentifier()} == {$"rewritten{property.Name}".ToIdentifier()}")));
                    Writer.Write($" ? node : new {type.ToFullyQualifiedName()}(");
                    for (int i = 0; i < model.Parameters.Length; i++)
                    {
                        if (i > 0) Writer.Write(", ");
                        var property = model.Parameters[i].Property;
                        if (propertiesToRewrite.Contains(property))
                            Writer.Write($"rewritten{property.Name}".ToIdentifier());
                        else
                            Writer.Write($"node.{property.Name.ToIdentifier()}");
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
