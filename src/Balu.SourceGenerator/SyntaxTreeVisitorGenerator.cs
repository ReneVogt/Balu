using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Balu.SourceGenerator;

sealed class SyntaxTreeVisitorGenerator : BaseGenerator
{
    readonly CSharpCompilation compilation;
    readonly INamedTypeSymbol syntaxNodeType, syntaxNodeKindType;

    internal SyntaxTreeVisitorGenerator(CSharpCompilation compilation, INamedTypeSymbol syntaxNodeType, INamedTypeSymbol syntaxNodeKindType)
    {
        this.compilation = compilation;
        this.syntaxNodeType = syntaxNodeType;
        this.syntaxNodeKindType = syntaxNodeKindType;
    }

    public override void Generate(GeneratorExecutionContext context)
    {
        var kindNames = syntaxNodeKindType.MemberNames.Where(name => !(name.EndsWith("Token", StringComparison.InvariantCulture) || name.EndsWith("Keyword", StringComparison.InvariantCulture))).ToImmutableArray();
        var types = compilation.Assembly.GetAllTypes();
        var syntaxNodeTypes = types.Where(t => !t.IsAbstract && t.IsDerivedFrom(syntaxNodeType) && SymbolEqualityComparer.Default.Equals(t.ContainingNamespace, syntaxNodeType.ContainingNamespace));
        var kindsToVisit = kindNames.Where(kindName => syntaxNodeTypes.Any(nodeType => nodeType.Name == $"{kindName}Syntax")).ToImmutableArray();

        Writer.WriteLine("using System;");
        Writer.WriteLine();
        Writer.WriteLine("namespace Balu.Syntax;");
        Writer.WriteLine();
        using(new CurlyIndenter(Writer, "public abstract class SyntaxTreeVisitor"))
        {
            using(new CurlyIndenter(Writer, "public virtual void Visit(SyntaxNode node)"))
            {
                using(new CurlyIndenter(Writer, "switch (node.Kind)"))
                {
                    foreach (var kind in kindsToVisit)
                    {
                        Writer.WriteLine($"case SyntaxKind.{kind}:");
                        Writer.Indent++;
                        Writer.WriteLine($"Visit{kind}(({kind}Syntax)node);");
                        Writer.WriteLine("break;");
                        Writer.Indent--;
                    }

                    Writer.WriteLine("default:");
                    Writer.Indent++;
                    Writer.WriteLine("VisitToken(node as SyntaxToken ??  throw new ArgumentException($\"Unknown syntax kind '{node.Kind}'.\"));");
                    Writer.WriteLine("break;");
                    Writer.Indent--;
                }
            }

            Writer.WriteLine();

            using(new CurlyIndenter(Writer, "void VisitChildren(SyntaxNode node)"))
            {
                Writer.WriteLine("for (int i=0; i<node.ChildrenCount; i++) Visit(node.GetChild(i));");
            }

            Writer.WriteLine();

            foreach (var kind in kindsToVisit)
                Writer.WriteLine($"protected virtual void Visit{kind}({kind}Syntax node) => VisitChildren(node);");
            Writer.WriteLine("protected virtual void VisitToken(SyntaxToken node) => VisitChildren(node);");
        }

        context.AddSource(
            "SyntaxTreeVisitor.g.cs",
            SourceText.From(Writer.InnerWriter.ToString(), Encoding.UTF8));
    }
}