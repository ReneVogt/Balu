namespace Balu.Binding;
#pragma warning disable CA1032

public sealed class BindingException : BaluException
{
    internal BindingException(string message)
        : base(message)
    {
    }
}

