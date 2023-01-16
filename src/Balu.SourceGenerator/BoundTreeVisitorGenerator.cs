using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Balu.SourceGenerator;

sealed class BoundTreeVisitorGenerator : BaseGenerator
{
    readonly CSharpCompilation compilation;
    readonly INamedTypeSymbol boundNodeType, boundNodeKindType;

    internal BoundTreeVisitorGenerator(CSharpCompilation compilation, INamedTypeSymbol boundNodeType, INamedTypeSymbol boundNodeKindType)
    {
        this.compilation = compilation;
        this.boundNodeType = boundNodeType;
        this.boundNodeKindType = boundNodeKindType;
    }

    public override void Generate(GeneratorExecutionContext context)
    {
        var kindNames = boundNodeKindType.MemberNames.ToImmutableArray();
        var types = compilation.Assembly.GetAllTypes();
        var boundNodeTypes = types.Where(t => !t.IsAbstract && t.IsDerivedFrom(boundNodeType) && SymbolEqualityComparer.Default.Equals(boundNodeType.ContainingNamespace, t.ContainingNamespace));
        var kindsToVisit = kindNames.Where(kindName => boundNodeTypes.Any(nodeType => nodeType.Name == $"Bound{kindName}")).ToImmutableArray();

        Writer.WriteLine("using System;");
        Writer.WriteLine();
        Writer.WriteLine("namespace Balu.Binding;");
        Writer.WriteLine();
        using(new CurlyIndenter(Writer, "abstract class BoundTreeVisitor"))
        {
            using(new CurlyIndenter(Writer, "public virtual void Visit(BoundNode node)"))
            {
                using(new CurlyIndenter(Writer, "switch (node.Kind)"))
                {
                    foreach (var kind in kindsToVisit)
                    {
                        Writer.WriteLine($"case BoundNodeKind.{kind}:");
                        Writer.Indent++;
                        Writer.WriteLine($"VisitBound{kind}((Bound{kind})node);");
                        Writer.WriteLine("break;");
                        Writer.Indent--;
                    }

                    Writer.WriteLine("default:");
                    Writer.Indent++;
                    Writer.WriteLine("throw new ArgumentException($\"Unexpected bound node kind '{node.Kind}'.\");");
                    Writer.Indent--;
                }
            }

            Writer.WriteLine();

            using(new CurlyIndenter(Writer, "void VisitChildren(BoundNode node)"))
            {
                Writer.WriteLine("for (int i=0; i<node.ChildrenCount; i++) Visit(node.GetChild(i));");
            }

            Writer.WriteLine();

            foreach (var kind in kindsToVisit)
                Writer.WriteLine($"protected virtual void VisitBound{kind}(Bound{kind} node) => VisitChildren(node);");
        }

        context.AddSource(
            "BoundTreeVisitor.g.cs",
            SourceText.From(Writer.InnerWriter.ToString(), Encoding.UTF8));
    }
}