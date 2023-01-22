using System.Collections.Immutable;
using Balu.Diagnostics;
using Balu.Symbols;

namespace Balu.Emit;

public sealed class EmitterResult
{
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public ImmutableDictionary<Symbol, string> GlobalSymbolNames { get; }
    public EmitterResult(ImmutableArray<Diagnostic> diagnostics, ImmutableDictionary<Symbol, string> globalSymbolNames)
    {
        Diagnostics = diagnostics;
        GlobalSymbolNames = globalSymbolNames;
    }
}