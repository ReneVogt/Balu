using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Balu.SourceGenerator;

sealed class SyntaxTreeVisitorGenerator : BaseGenerator
{
    readonly NodeKindMapping mapping;

    internal SyntaxTreeVisitorGenerator(NodeKindMapping mapping)
    {
        this.mapping = mapping;
    }

    public override void Generate(GeneratorExecutionContext context)
    {
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
                    foreach (var (kind, node) in mapping.Mappings)
                    {
                        Writer.WriteLine($"case SyntaxKind.{kind.ToIdentifier()}:");
                        Writer.Indent++;
                        Writer.WriteLine($"{$"Visit{kind.Name}".ToIdentifier()}(({node.ToFullyQualifiedName()})node);");
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

            foreach (var (kind, node) in mapping.Mappings)
            {
                var accessibility = node.DeclaredAccessibility == Accessibility.Public ? "protected" : "private protected";
                Writer.WriteLine($"{accessibility} virtual void {$"Visit{kind.Name}".ToIdentifier()}({node.ToFullyQualifiedName()} node) => VisitChildren(node);");
            }
            Writer.WriteLine("protected virtual void VisitToken(SyntaxToken node) => VisitChildren(node);");
        }

        context.AddSource(
            "SyntaxTreeVisitor.g.cs",
            SourceText.From(Writer.InnerWriter.ToString(), Encoding.UTF8));
    }
}
