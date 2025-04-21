using System.Collections.Generic;
using Balu.Symbols;
using Mono.Cecil.Cil;

namespace Balu.Emit;

sealed class LocalVariableScope(LocalVariableScope? parent, int startIndex, bool isBlock = false)
{
    public LocalVariableScope? Parent { get; } = parent;
    public bool IsBlock { get; } = isBlock;
    public Dictionary<LocalVariableSymbol, VariableDefinition> Locals { get; } = [];
    public List<LocalVariableScope> Scopes { get; } = [];
    public int StartIndex { get; } = startIndex;
    public int EndIndex { get; set; }
    public VariableDefinition this[LocalVariableSymbol variable] => Locals.TryGetValue(variable, out var definition)
                                                                        ? definition
                                                                        : Parent?[variable] ??
                                                                          throw new KeyNotFoundException(
                                                                              $"Local variable '{variable.Name}' not found in scopes.");
}