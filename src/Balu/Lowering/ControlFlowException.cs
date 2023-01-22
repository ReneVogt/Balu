namespace Balu.Lowering;
#pragma warning disable CA1032

public sealed class ControlFlowException : BaluException
{
    internal ControlFlowException(string message)
        : base(message)
    {
    }
}

