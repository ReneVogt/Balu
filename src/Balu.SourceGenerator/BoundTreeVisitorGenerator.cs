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
public sealed class BoundTreeVisitorGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }
    public void Execute(GeneratorExecutionContext context)
    {
        using var source = new StringWriter();
        using var writer = new IndentedTextWriter(source, "    ");

        var compilation = (CSharpCompilation)context.Compilation;
        var boundNodeType = compilation.GetTypeByMetadataName("Balu.Binding.BoundNode");
        var nodeKindType = compilation.GetTypeByMetadataName("Balu.Binding.BoundNodeKind");
        if (boundNodeType is null || nodeKindType is null)
            return;

        var kindNames = nodeKindType.MemberNames.ToImmutableArray();
        var types = compilation.Assembly.GetAllTypes();
        var boundNodeTypes = types.Where(t => !t.IsAbstract && t.IsDerivedFrom(boundNodeType));
        var kindsToVisit = kindNames.Where(kindName => boundNodeTypes.Any(nodeType => nodeType.Name == $"Bound{kindName}")).ToImmutableArray();

        writer.WriteLine("using System;");
        writer.WriteLine();
        writer.WriteLine("namespace Balu.Binding;");
        writer.WriteLine();
        using(new CurlyIndenter(writer, "abstract class BoundTreeVisitor"))
        {
            using(new CurlyIndenter(writer, "public virtual void Visit(BoundNode node)"))
            {
                using(new CurlyIndenter(writer, "switch (node.Kind)"))
                {
                    foreach (var kind in kindsToVisit)
                    {
                        writer.WriteLine($"case BoundNodeKind.{kind}:");
                        writer.Indent++;
                        writer.WriteLine($"VisitBound{kind}((Bound{kind})node);");
                        writer.WriteLine("break;");
                        writer.Indent--;
                    }

                    writer.WriteLine("default:");
                    writer.Indent++;
                    writer.WriteLine("throw new ArgumentException($\"Unexpected bound node kind '{node.Kind}'.\");");
                    writer.Indent--;
                }
            }

            writer.WriteLine();

            using(new CurlyIndenter(writer, "void VisitChildren(BoundNode node)"))
            {
                writer.WriteLine("for (int i=0; i<node.ChildrenCount; i++) Visit(node.GetChild(i));");
            }

            writer.WriteLine();

            foreach (var kind in kindsToVisit)
                writer.WriteLine($"protected virtual void VisitBound{kind}(Bound{kind} node) => VisitChildren(node);");
        }

        context.AddSource(
            "BoundTreeVisitor.g.cs",
            SourceText.From(source.ToString(), Encoding.UTF8));
    }
}