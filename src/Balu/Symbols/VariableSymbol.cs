namespace Balu.Symbols;

public abstract class VariableSymbol : Symbol
{
    public TypeSymbol Type { get; }
    public bool ReadOnly { get; }

    private protected VariableSymbol(string name, bool readOnly, TypeSymbol type) : base (name)
    {
        ReadOnly = readOnly;
        Type = type;
    }

    public override string ToString() => $"{Name} ({Type.Name})";
}