using System;

namespace Balu;

/// <summary>
/// The abstract base class for exceptions of the Balu compiler.
/// </summary>
public abstract class BaluException : Exception
{
    /// <summary>
    /// Creates a new instance of an exception derived from <see cref="BaluException"/>.
    /// </summary>
    /// <param name="message">The exception message.</param>
    protected BaluException(string message) : base(message) { }
}
