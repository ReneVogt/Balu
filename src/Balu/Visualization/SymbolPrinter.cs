using System;
using System.IO;
using Balu.Symbols;

namespace Balu.Visualization;

public static class SymbolPrinter
{
    public static void WriteTo(this Symbol symbol, TextWriter writer)
    {
        _ = symbol ?? throw new ArgumentNullException(nameof(symbol));
        _ = writer ?? throw new ArgumentNullException(nameof(writer));

        switch (symbol)
        {
            case FunctionSymbol function:
                writer.WriteKeyword("function");
                writer.WriteSpace();
                writer.WriteIdentifier(function.Name);
                writer.WritePunctuation("(");
                for (int i = 0; i < function.Parameters.Length; i++)
                {
                    if (i > 0) writer.WritePunctuation(", ");
                    writer.WriteIdentifier(function.Parameters[i].Name);
                    writer.WriteSpace();
                    writer.WritePunctuation(":");
                    writer.WriteSpace();
                    writer.WriteIdentifier(function.Parameters[i].Type.Name);
                }

                writer.WritePunctuation(")");
                if (function.ReturnType == TypeSymbol.Void) break;
                writer.WriteSpace();
                writer.WritePunctuation(":");
                writer.WriteSpace();
                writer.WriteIdentifier(function.ReturnType.Name);
                break;
            case GlobalVariableSymbol variable:
                writer.WriteKeyword(variable.ReadOnly ? "let" : "var");
                writer.WriteSpace();
                writer.WriteIdentifier(variable.Name);
                writer.WriteSpace();
                writer.WritePunctuation(":");
                writer.WriteSpace();
                writer.WriteIdentifier(variable.Type.Name);
                break;
            default:
                writer.WriteIdentifier(symbol.Name);
                writer.WriteSpace();
                writer.WritePunctuation(symbol.Kind.ToString());
                break;
        }
    }
}