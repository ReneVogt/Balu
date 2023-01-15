using System;
using System.CodeDom.Compiler;

sealed class CurlyIndenter : IDisposable
{
    private readonly IndentedTextWriter indentedTextWriter;

    public CurlyIndenter(IndentedTextWriter indentedTextWriter, string openingLine = "")
    {
        this.indentedTextWriter = indentedTextWriter;
        if (!string.IsNullOrWhiteSpace(openingLine))
            indentedTextWriter.WriteLine(openingLine);
        indentedTextWriter.WriteLine("{");
        indentedTextWriter.Indent++;
    }

    public void Dispose()
    {
        indentedTextWriter.Indent--;
        indentedTextWriter.WriteLine("}");
    }
}
