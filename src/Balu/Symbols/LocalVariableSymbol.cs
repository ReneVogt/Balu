using Balu.Binding;

namespace Balu.Symbols;

public class LocalVariableSymbol : VariableSymbol
{
    internal LocalVariableSymbol(string name, bool readOnly, TypeSymbol type, BoundConstant? constant)
        : base(name, readOnly, type, constant) { }
    public override SymbolKind Kind => SymbolKind.LocalVariable;
    public override string ToString() => $"{Name} ({Type.Name}) (local)";
}