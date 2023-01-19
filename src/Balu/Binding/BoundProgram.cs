using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Balu.Symbols;

namespace Balu.Binding;

sealed class BoundProgram
{
    public FunctionSymbol EntryPoint { get; }
    public ImmutableArray<Symbol> Symbols { get; }
    public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; }
    public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> AllVisibleFunctions { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public BoundProgram(BoundProgram?  previous, FunctionSymbol entryPoint, ImmutableArray<Symbol> symbols, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, ImmutableArray<Diagnostic> diagnostics)
    {
        EntryPoint = entryPoint;
        Symbols = symbols;
        Functions = functions;
        Diagnostics = diagnostics;

        if (previous is null)
            AllVisibleFunctions = Functions;
        else
        {
            var builder = Functions.ToBuilder();
            var functionNames = Functions.Select(x => x.Key.Name).ToHashSet();
            foreach (var (symbol, body) in previous.AllVisibleFunctions)
                if (functionNames.Add(symbol.Name))
                    builder.TryAdd(symbol, body);
            AllVisibleFunctions = builder.ToImmutable();
        }
    }
}
