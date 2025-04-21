using System.Collections.Immutable;
using Balu.Diagnostics;
using Balu.Symbols;

namespace Balu.Emit;

public sealed class EmitterResult(ImmutableArray<Diagnostic> diagnostics, ImmutableDictionary<Symbol, string> globalSymbolNames)
{
    public ImmutableArray<Diagnostic> Diagnostics { get; } = diagnostics;
    public ImmutableDictionary<Symbol, string> GlobalSymbolNames { get; } = globalSymbolNames;
}