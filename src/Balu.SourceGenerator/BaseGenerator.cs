using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CodeAnalysis;


namespace Balu.SourceGenerator;

abstract class BaseGenerator
{
    public const string TABSTRING = "    ";


    protected IndentedTextWriter Writer { get; }

    private protected BaseGenerator()
    {
        Writer = new (new StringWriter(), TABSTRING);
    }

    public abstract void Generate(GeneratorExecutionContext context);
}
