using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Balu.Symbols;

namespace Balu.Binding;
sealed class BoundScope
{
    Dictionary<string, VariableSymbol>? variables;
    Dictionary<string, FunctionSymbol>? functions;

    public BoundScope? Parent { get; }
    public BoundScope(BoundScope? parent)
    {
        Parent = parent;
        if (Parent is null)
            functions = BuiltInFunctions.GetBuiltInFunctions().ToDictionary(function => function.Name, function => function);
    }

    public bool TryLookupVariable(string name, out VariableSymbol variable)
    {
        variable = null!;
        return (variables?.TryGetValue(name, out variable!) ?? false) || (Parent?.TryLookupVariable(name, out variable) ?? false);
    }
    public bool TryDeclareVariable(VariableSymbol variable) => TryDeclare(variable, ref variables);
    public ImmutableArray<VariableSymbol> GetDeclaredVariables() => variables?.Values.ToImmutableArray() ?? ImmutableArray<VariableSymbol>.Empty;

    public bool TryLookupFunction(string name, out FunctionSymbol function)
    {
        function= null!;
        return (functions?.TryGetValue(name, out function!) ?? false) || (Parent?.TryLookupFunction(name, out function) ?? false);
    }

    public bool TryDeclareFunction(FunctionSymbol function) => TryDeclare(function, ref functions);
    public ImmutableArray<FunctionSymbol> GetDeclaredFunctions() => functions?.Values.ToImmutableArray() ?? ImmutableArray<FunctionSymbol>.Empty;
    
    static bool TryDeclare<T>(T symbol, ref Dictionary<string, T>? store) where T : Symbol
    {
        store ??= new();
        if (store.ContainsKey(symbol.Name)) return false;
        store.Add(symbol.Name, symbol);
        return true;
    }
}
