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
    /// <summary>
    /// Indicates wether this variable can be changed or not.
    /// </summary>
    public bool ReadOnly { get; }

    internal VariableSymbol(string name, bool readOnly, Type type) => (Name, ReadOnly, Type) = (name, readOnly, type);
}
