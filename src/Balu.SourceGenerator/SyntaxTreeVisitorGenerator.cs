using System;
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
public sealed class SyntaxTreeVisitorGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }
    public void Execute(GeneratorExecutionContext context)
    {
        using var source = new StringWriter();
        using var writer = new IndentedTextWriter(source, "    ");

        var compilation = (CSharpCompilation)context.Compilation;
        var syntaxNodeType = compilation.GetTypeByMetadataName("Balu.Syntax.SyntaxNode");
        var nodeKindType = compilation.GetTypeByMetadataName("Balu.Syntax.SyntaxKind");
        if (syntaxNodeType is null || nodeKindType is null)
            return;

        var kindNames = nodeKindType.MemberNames.Where(name => !(name.EndsWith("Token", StringComparison.InvariantCulture) || name.EndsWith("Keyword", StringComparison.InvariantCulture))).ToImmutableArray();
        var types = compilation.Assembly.GetAllTypes();
        var syntaxNodeTypes = types.Where(t => !t.IsAbstract && t.IsDerivedFrom(syntaxNodeType));
        var kindsToVisit = kindNames.Where(kindName => syntaxNodeTypes.Any(nodeType => nodeType.Name == $"{kindName}Syntax")).ToImmutableArray();

        writer.WriteLine("using System;");
        writer.WriteLine();
        writer.WriteLine("namespace Balu.Syntax;");
        writer.WriteLine();
        using(new CurlyIndenter(writer, "public abstract class SyntaxTreeVisitor"))
        {
            using(new CurlyIndenter(writer, "public virtual void Visit(SyntaxNode node)"))
            {
                using(new CurlyIndenter(writer, "switch (node.Kind)"))
                {
                    foreach (var kind in kindsToVisit)
                    {
                        writer.WriteLine($"case SyntaxKind.{kind}:");
                        writer.Indent++;
                        writer.WriteLine($"Visit{kind}(({kind}Syntax)node);");
                        writer.WriteLine("break;");
                        writer.Indent--;
                    }

                    writer.WriteLine("default:");
                    writer.Indent++;
                    writer.WriteLine("VisitToken(node as SyntaxToken ??  throw new ArgumentException($\"Unknown syntax kind '{node.Kind}'.\"));");
                    writer.WriteLine("break;");
                    writer.Indent--;
                }
            }

            writer.WriteLine();

            using(new CurlyIndenter(writer, "void VisitChildren(SyntaxNode node)"))
            {
                writer.WriteLine("for (int i=0; i<node.ChildrenCount; i++) Visit(node.GetChild(i));");
            }

            writer.WriteLine();

            foreach (var kind in kindsToVisit)
                writer.WriteLine($"protected virtual void Visit{kind}({kind}Syntax node) => VisitChildren(node);");
            writer.WriteLine("protected virtual void VisitToken(SyntaxToken node) => VisitChildren(node);");
        }

        context.AddSource(
            "SyntaxTreeVisitor.g.cs",
            SourceText.From(source.ToString(), Encoding.UTF8));
    }
}