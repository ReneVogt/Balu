using System.Collections.Generic;
using System.Collections.Immutable;

namespace Balu.Binding;
sealed class BoundScope
{
    readonly Dictionary<string, VariableSymbol> variables = new();

    BoundScope? Parent { get; }
    public BoundScope(BoundScope? parent) => Parent = parent;

    public bool TryLookup(string name, out VariableSymbol variable) => variables.TryGetValue(name, out variable!) || (Parent?.TryLookup(name, out variable) ?? false);
    public bool TryDeclare(VariableSymbol variable)
    {
        if (variables.ContainsKey(variable.Name)) return false;
        variables.Add(variable.Name, variable);
        return true;
    }
    public ImmutableArray<VariableSymbol> GetDeclaredVariables() => variables.Values.ToImmutableArray();
}
