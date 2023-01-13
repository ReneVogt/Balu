namespace Balu.Binding;

sealed class BoundConstant
{
    public object Value { get; }
    internal BoundConstant(object value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToString() ?? string.Empty;
}
