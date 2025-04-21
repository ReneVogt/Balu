namespace Balu.Emit;
#pragma warning disable IDE0079
#pragma warning disable CA1032

public sealed class EmitterException : BaluException
{
    internal EmitterException(string message)
        : base(message)
    {
    }
}

