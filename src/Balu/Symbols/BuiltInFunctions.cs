using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Balu.Symbols;

static class BuiltInFunctions
{
    public static FunctionSymbol Print { get; } =
        new FunctionSymbol("print", ImmutableArray.Create(new ParameterSymbol("value", TypeSymbol.Any, 0)), TypeSymbol.Void);
    public static FunctionSymbol PrintLine { get; } =
        new FunctionSymbol("println", ImmutableArray.Create(new ParameterSymbol("value", TypeSymbol.Any, 0)), TypeSymbol.Void);
    public static FunctionSymbol Input { get; } =
        new FunctionSymbol("input", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.String);
    public static FunctionSymbol Random { get; } =
        new FunctionSymbol("random", ImmutableArray.Create(new ParameterSymbol("maximum", TypeSymbol.Integer, 0)), TypeSymbol.Integer);

    static ImmutableArray<FunctionSymbol>? builtInFunctions;
    public static IEnumerable<FunctionSymbol> GetBuiltInFunctions()
    {
        if (builtInFunctions.HasValue) return builtInFunctions.Value;

        var builder = ImmutableArray.CreateBuilder<FunctionSymbol>();
        builder.AddRange(typeof(BuiltInFunctions)
                         .GetProperties(BindingFlags.Public | BindingFlags.Static)
                         .Where(property => property.PropertyType == typeof(FunctionSymbol))
                         .Select(property => (FunctionSymbol)property.GetValue(0)!));
        builtInFunctions = builder.ToImmutable();
        return builder;
    }
}
