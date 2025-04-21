namespace Balu.Lowering;
#pragma warning disable IDE0079
#pragma warning disable CA1032

public sealed class ControlFlowException : BaluException
{
    internal ControlFlowException(string message)
        : base(message)
    {
    }
}

