namespace Balu.Symbols;

/// <summary>
/// Abstract base class for named and typed bound variable.
/// </summary>
public abstract class VariableSymbol : Symbol
{
    /// <summary>
    /// The type of the variable.
    /// </summary>
    public TypeSymbol Type { get; }
    /// <summary>
    /// Indicates wether this variable can be changed or not.
    /// </summary>
    public bool ReadOnly { get; }

    private protected VariableSymbol(string name, bool readOnly, TypeSymbol type) : base (name)
    {
        ReadOnly = readOnly;
        Type = type;
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Name} ({Type.Name})";
}