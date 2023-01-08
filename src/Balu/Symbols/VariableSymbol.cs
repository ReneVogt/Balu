using Balu.Binding;

namespace Balu.Symbols;

public abstract class VariableSymbol : Symbol
{
    public TypeSymbol Type { get; }
    public bool ReadOnly { get; }
    internal BoundConstant? Constant { get; }

    private protected VariableSymbol(string name, bool readOnly, TypeSymbol type, BoundConstant? constant) : base (name)
    {
        ReadOnly = readOnly;
        Type = type;
        Constant = ReadOnly ? constant : null;
    }

    public override string ToString() => $"{Name} ({Type.Name})";
}