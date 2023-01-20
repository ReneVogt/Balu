namespace Balu.Tests.TestHelper;

static class ReferenceProvider
{
    // HACK: find a way to get this for tests
    public static string[] References { get; } =
    {
        @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.13\ref\net6.0\System.Runtime.dll",
        @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.13\ref\net6.0\System.Runtime.Extensions.dll",
        @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.13\ref\net6.0\System.Console.dll"
    };
}
