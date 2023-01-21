using System.Collections.Immutable;
using System;
using Balu.Diagnostics;

namespace Balu.Emit;
#pragma warning disable CA1032, CA1064

sealed class MissingReferencesException : Exception
{
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public MissingReferencesException(ImmutableArray<Diagnostic> diagnostics) => Diagnostics = diagnostics;
}

