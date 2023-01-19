using System.Collections.Immutable;
using Balu.Symbols;

namespace Balu.Emit;

sealed class EmitterResult
{
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public ImmutableDictionary<GlobalVariableSymbol, string> GlobalFieldNames { get; }
    public EmitterResult(ImmutableArray<Diagnostic> diagnostics, ImmutableDictionary<GlobalVariableSymbol, string> globalFieldNames)
    {
        Diagnostics = diagnostics;
        GlobalFieldNames = globalFieldNames;
    }
}