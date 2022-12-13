namespace Balu.Symbols;

/// <summary>
/// Represents a named and typed bound variable.
/// </summary>
public sealed class VariableSymbol : Symbol
{
    /// <inheritdoc />
    public override SymbolKind Kind => SymbolKind.Variable;

    /// <summary>
    /// The type of the variable.
    /// </summary>
    public TypeSymbol Type { get; }
    /// <summary>
    /// Indicates wether this variable can be changed or not.
    /// </summary>
    public bool ReadOnly { get; }

    internal VariableSymbol(string name, bool readOnly, TypeSymbol type) : base (name)
    {
        ReadOnly = readOnly;
        Type = type;
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Name} ({Type.Name})";
}
