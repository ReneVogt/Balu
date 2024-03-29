﻿using System.Collections.Immutable;
using Balu.Diagnostics;
using Balu.Symbols;

namespace Balu.Binding;

sealed class BoundProgram
{
    public FunctionSymbol EntryPoint { get; }
    public ImmutableArray<Symbol> Symbols { get; }
    public ImmutableArray<Symbol> VisibleSymbols { get; }
    public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public BoundProgram(FunctionSymbol entryPoint, ImmutableArray<Symbol> symbols, ImmutableArray<Symbol> visibleSymbols, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, ImmutableArray<Diagnostic> diagnostics)
    {
        EntryPoint = entryPoint;
        Symbols = symbols;
        VisibleSymbols = visibleSymbols;
        Functions = functions;
        Diagnostics = diagnostics;
    }
}
