using System;
using System.IO;

namespace Balu.Tests.TestHelper;

static class ReferenceProvider
{
    static readonly string referenceDirectory = Path.Combine(AppContext.BaseDirectory, "reference-assemblies");

    public static string[] References { get; } =
    [
        Path.Combine(referenceDirectory, "System.Runtime.dll"),
        Path.Combine(referenceDirectory, "System.Runtime.Extensions.dll"),
        Path.Combine(referenceDirectory, "System.Console.dll")
    ];
}
