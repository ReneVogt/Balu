#pragma warning disable IDE0079
#pragma warning disable CA1032
namespace Balu.Syntax;

public sealed class SyntaxException : BaluException
{
    internal SyntaxException(string message) : base(message)
    {}
}