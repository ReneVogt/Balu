using System;
#pragma warning disable CA1032

namespace Balu;

/// <summary>
/// The abstract base class for exceptions of the Balu compiler.
/// </summary>
public abstract class BaluException : Exception
{
    private protected BaluException(string message) : base(message) { }
}
