namespace Balu;

sealed class LabelSymbol
{
    public string Name { get; }
    internal LabelSymbol(string name) => Name = name;

    public override string ToString() => Name;
}
