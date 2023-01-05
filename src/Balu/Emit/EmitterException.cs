namespace Balu.Emit;
#pragma warning disable CA1032

public sealed class EmitterException : BaluException
{
    internal EmitterException(string message)
        : base(message)
    {
    }
}

