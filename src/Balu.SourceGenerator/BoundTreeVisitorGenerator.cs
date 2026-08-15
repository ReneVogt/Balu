using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Balu.SourceGenerator;

sealed class BoundTreeVisitorGenerator : BaseGenerator
{
    readonly NodeKindMapping mapping;

    internal BoundTreeVisitorGenerator(NodeKindMapping mapping)
    {
        this.mapping = mapping;
    }

    public override void Generate(GeneratorExecutionContext context)
    {
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
                    foreach (var (kind, node) in mapping.Mappings)
                    {
                        Writer.WriteLine($"case BoundNodeKind.{kind.ToIdentifier()}:");
                        Writer.Indent++;
                        Writer.WriteLine($"{$"VisitBound{kind.Name}".ToIdentifier()}(({node.ToFullyQualifiedName()})node);");
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

            foreach (var (kind, node) in mapping.Mappings)
                Writer.WriteLine($"protected virtual void {$"VisitBound{kind.Name}".ToIdentifier()}({node.ToFullyQualifiedName()} node) => VisitChildren(node);");
        }

        context.AddSource(
            "BoundTreeVisitor.g.cs",
            SourceText.From(Writer.InnerWriter.ToString(), Encoding.UTF8));
    }
}
