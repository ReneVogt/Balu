using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Balu.SourceGenerator;

[Generator]
public sealed class BoundTreeRewriterGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }
    public void Execute(GeneratorExecutionContext context)
    {
        using var source = new StringWriter();
        using var writer = new IndentedTextWriter(source, "    ");

        var compilation = (CSharpCompilation)context.Compilation;
        var boundNodeType = compilation.GetTypeByMetadataName("Balu.Binding.BoundNode");
        var nodeKindType = compilation.GetTypeByMetadataName("Balu.Binding.BoundNodeKind");
        var loopStatementType = compilation.GetTypeByMetadataName("Balu.Binding.BoundLoopStatement");
        var immutableArrayType = compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableArray`1");

        if (boundNodeType is null || nodeKindType is null || immutableArrayType is null || loopStatementType is null)
            return;

        var kindNames = nodeKindType.MemberNames.ToImmutableArray();
        var types = compilation.Assembly.GetAllTypes();
        var boundNodeTypes = types.Where(t => !t.IsAbstract && t.IsDerivedFrom(boundNodeType));
        var kindsToVisit = kindNames
                           .Select(kindName => (kind: kindName,
                                                   type: boundNodeTypes.SingleOrDefault(nodeType => nodeType.Name == $"Bound{kindName}")))
                           .Where(x => x.type is not null)
                           .ToImmutableArray();

        writer.WriteLine("using System;");
        writer.WriteLine("using System.Collections.Immutable;");
        writer.WriteLine("using System.Linq;");
        writer.WriteLine("#nullable enable");
        writer.WriteLine();
        writer.WriteLine("namespace Balu.Binding;");
        writer.WriteLine();
        using(new CurlyIndenter(writer, "abstract class BoundTreeRewriter"))
        {
            using(new CurlyIndenter(writer, "public virtual BoundNode Visit(BoundNode node) => node.Kind switch", semicolon: true))
            {
                foreach (var (kind, type) in kindsToVisit)
                    writer.WriteLine($"BoundNodeKind.{kind} => VisitBound{kind}(({type!.GetFullName()})node),");

                writer.WriteLine("_ => throw new ArgumentException($\"Unexpected bound node kind '{node.Kind}'.\")");
            }

            writer.WriteLine(@"
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
                    writer.WriteLine($"protected virtual BoundNode VisitBound{kind}({type.GetFullName()} node) => node;");
                    continue;
                }

                using(new CurlyIndenter(writer, $"protected virtual BoundNode VisitBound{kind}({type.GetFullName()} node)"))
                {
                    foreach (var property in properties)
                    {
                        if (((INamedTypeSymbol)property.Type).IsDerivedFrom(boundNodeType))
                        {
                            if (property.NullableAnnotation == NullableAnnotation.Annotated)
                                writer.WriteLine(
                                    $"var rewritten{property.Name} = node.{property.Name} is null ? null : ({property.Type.GetFullName()})Visit(node.{property.Name});");
                            else
                                writer.WriteLine($"var rewritten{property.Name} = ({property.Type.GetFullName()})Visit(node.{property.Name});");
                        }
                        else
                            writer.WriteLine($"var rewritten{property.Name} = RewriteList(node.{property.Name});");
                    }

                    writer.WriteLine();
                    writer.Write("return ");
                    writer.Write(string.Join(" && ", properties.Select(property => $"node.{property.Name} == rewritten{property.Name}")));
                    writer.Write($" ? node : new {type.GetFullName()}(node.Syntax");

                    var allProperties = type.GetMembers()
                                            .OfType<IPropertySymbol>()
                                            .Where(property => property.Type is INamedTypeSymbol)
                                            .ToImmutableArray();
                    var constructorParameter = type.InstanceConstructors.First(ctor => ctor.Parameters.Length > 1).Parameters;
                    int arguments = 1;
                    foreach (IPropertySymbol property in allProperties.Where(property => SymbolEqualityComparer.Default.Equals(property.Type, constructorParameter[arguments].Type)))
                    {
                        arguments++;

                        writer.Write(", ");
                        if (properties.Contains(property))
                            writer.Write($"rewritten{property.Name}");
                        else
                            writer.Write($"node.{property.Name}");

                        if (arguments >= constructorParameter.Length) break;
                    }

                    if (type.IsDerivedFrom(loopStatementType))
                        writer.Write(", node.BreakLabel, node.ContinueLabel");

                    writer.WriteLine(");");
                }
            }
        }


        context.AddSource(
            "BoundTreeRewriter.g.cs",
            SourceText.From(source.ToString(), Encoding.UTF8));
    }
}
