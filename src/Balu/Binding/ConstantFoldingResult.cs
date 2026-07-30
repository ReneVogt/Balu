namespace Balu.Binding;

enum ConstantFoldingError
{
    None,
    DivisionByZero,
    IntegerOverflow
}

readonly struct ConstantFoldingResult
{
    public BoundConstant? Constant { get; }
    public ConstantFoldingError Error { get; }

    public ConstantFoldingResult(BoundConstant? constant, ConstantFoldingError error = ConstantFoldingError.None)
    {
        Constant = constant;
        Error = error;
    }
}
