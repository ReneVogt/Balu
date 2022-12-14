using System.Collections.Generic;
using System.Collections.Immutable;

namespace Balu.Symbols;

/// <summary>
/// Represents a function in Balu.
/// </summary>
public sealed class FunctionSymbol : Symbol
{
    /// <inheritdoc />
    public override SymbolKind Kind => SymbolKind.Function;

    /// <summary>
    /// The function's parameters.
    /// </summary>
    public ImmutableArray<ParameterSymbol> Parameters { get; }
    /// <summary>
    /// The function's return type.
    /// </summary>
    public TypeSymbol ReturnType { get; }

    internal FunctionSymbol(string name, IEnumerable<ParameterSymbol> parameters, TypeSymbol returnType) : base (name)
    {
        Parameters = parameters.ToImmutableArray();
        ReturnType = returnType;
    }
}
