using System.Collections.Generic;
using Balu.Symbols;
using Mono.Cecil.Cil;

namespace Balu.Emit;

sealed class LocalVariableScope
{
    public LocalVariableScope? Parent { get; }
    public bool IsBlock { get; }
    public Dictionary<LocalVariableSymbol, VariableDefinition> Locals { get; } = new();
    public List<LocalVariableScope> Scopes { get; } = new();
    public int StartIndex { get; }
    public int EndIndex { get; set; }
    public VariableDefinition this[LocalVariableSymbol variable] => Locals.TryGetValue(variable, out var definition)
                                                                        ? definition
                                                                        : Parent?[variable] ??
                                                                          throw new KeyNotFoundException(
                                                                              $"Local variable '{variable.Name}' not found in scopes.");
    public LocalVariableScope(LocalVariableScope? parent, int startIndex, bool isBlock = false)
    {
        Parent = parent;
        StartIndex = startIndex;
        IsBlock = isBlock;
    }
}