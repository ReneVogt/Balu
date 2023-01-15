using System;
using System.CodeDom.Compiler;

sealed class CurlyIndenter : IDisposable
{
    readonly IndentedTextWriter indentedTextWriter;
    readonly bool semicolon;

    public CurlyIndenter(IndentedTextWriter indentedTextWriter, string openingLine = "", bool semicolon = false)
    {
        this.indentedTextWriter = indentedTextWriter;
        if (!string.IsNullOrWhiteSpace(openingLine))
            indentedTextWriter.WriteLine(openingLine);
        indentedTextWriter.WriteLine("{");
        indentedTextWriter.Indent++;
        this.semicolon = semicolon;
    }

    public void Dispose()
    {
        indentedTextWriter.Indent--;
        indentedTextWriter.WriteLine(semicolon ? "};" : "}");
    }
}
