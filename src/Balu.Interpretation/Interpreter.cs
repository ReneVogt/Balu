using System.Collections.Immutable;
using Balu.Diagnostics;
using Balu.Symbols;

namespace Balu.Interpretation;

public sealed class Interpreter
{
    public object? Result { get; }
    public ImmutableArray<Symbol> VisibleSymbols { get; private set; } = ImmutableArray<Symbol>.Empty;
    public ImmutableArray<Symbol> AllSymbols { get; private set; } = ImmutableArray<Symbol>.Empty;
    public ImmutableDictionary<GlobalVariableSymbol, object> GlobalVariables { get; private set; } = ImmutableDictionary<GlobalVariableSymbol, object>.Empty;

    public ImmutableArray<Diagnostic> Interprete(Compilation compilation)
    {
        return ImmutableArray<Diagnostic>.Empty;
    }
}