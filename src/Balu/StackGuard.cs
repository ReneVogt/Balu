using System;
using System.Runtime.CompilerServices;

namespace Balu;

static class StackGuard
{
    const int MaxUncheckedRecursionDepth = 20;
    internal const int MaxRecursionDepth = 256;

    public static void EnsureSufficientExecutionStack(int recursionDepth)
    {
        if (recursionDepth > MaxRecursionDepth)
            throw new InsufficientExecutionStackException();

        if (recursionDepth > MaxUncheckedRecursionDepth)
            RuntimeHelpers.EnsureSufficientExecutionStack();
    }
}
