using System.Collections.Generic;
using System.Collections.Immutable;
using Balu.Symbols;

namespace Balu.Binding;
sealed class BoundScope
{
    readonly Dictionary<string, VariableSymbol> variables = new();
    readonly Dictionary<string, FunctionSymbol> functions = new();

    public BoundScope? Parent { get; }
    public BoundScope(BoundScope? parent)
    {
        Parent = parent;
        if (Parent is null)
            foreach (var function in BuiltInFunctions.GetBuiltInFunctions())
                functions.Add(function.Name, function);
    }

    public bool TryLookupVariable(string name, out VariableSymbol variable) => variables.TryGetValue(name, out variable!) || (Parent?.TryLookupVariable(name, out variable) ?? false);
    public bool TryDeclareVariable(VariableSymbol variable) => TryDeclare(variable, variables);
    public ImmutableArray<VariableSymbol> GetDeclaredVariables() => variables.Values.ToImmutableArray();

    public bool TryLookupFunction(string name, out FunctionSymbol function) => functions.TryGetValue(name, out function!) || (Parent?.TryLookupFunction(name, out function) ?? false);
    public bool TryDeclareFunction(FunctionSymbol function) => TryDeclare(function, functions);
    public ImmutableArray<FunctionSymbol> GetDeclaredFunctions() => functions.Values.ToImmutableArray();


    static bool TryDeclare<T>(T symbol, Dictionary<string, T> store) where T : Symbol
    {
        if (store.ContainsKey(symbol.Name)) return false;
        store.Add(symbol.Name, symbol);
        return true;
    }
}
