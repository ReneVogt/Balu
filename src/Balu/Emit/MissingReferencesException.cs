using System.Collections.Immutable;
using System;
using Balu.Diagnostics;

namespace Balu.Emit;
#pragma warning disable IDE0079
#pragma warning disable CA1032, CA1064

sealed class MissingReferencesException(ImmutableArray<Diagnostic> diagnostics) : Exception
{
    public ImmutableArray<Diagnostic> Diagnostics { get; } = diagnostics;
}

