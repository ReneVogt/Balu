using System.Collections.Generic;
using System.Collections.Immutable;
using Balu.Syntax;

namespace Balu.Symbols;

public sealed class FunctionSymbol : Symbol
{
    public override SymbolKind Kind => SymbolKind.Function;
    public ImmutableArray<ParameterSymbol> Parameters { get; }
    public TypeSymbol ReturnType { get; }
    public FunctionDeclarationSyntax? Declaration { get; }

    internal FunctionSymbol(string name, IEnumerable<ParameterSymbol> parameters, TypeSymbol returnType, FunctionDeclarationSyntax? declaration = null) : base (name)
    {
        Parameters = parameters.ToImmutableArray();
        ReturnType = returnType;
        Declaration = declaration;
    }
}
