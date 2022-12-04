using System;

namespace Balu;

/// <summary>
/// Represents a named and typed bound variable.
/// </summary>
public sealed class VariableSymbol
{
    /// <summary>
    /// The name of the variable.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// The type of the variable.
    /// </summary>
    public Type Type { get; }

    internal VariableSymbol(string name, Type type) => (Name, Type) = (name, type);
}
