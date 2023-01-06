using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Balu.Symbols;

static class BuiltInFunctions
{
    public static FunctionSymbol Print { get; } =
        new FunctionSymbol("print", ImmutableArray.Create(new ParameterSymbol("text", TypeSymbol.Any, 0)), TypeSymbol.Void);
    public static FunctionSymbol Input { get; } =
        new FunctionSymbol("input", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.String);
    public static FunctionSymbol Random { get; } =
        new FunctionSymbol("random", ImmutableArray.Create(new ParameterSymbol("maximum", TypeSymbol.Integer, 0)), TypeSymbol.Integer);

    public static IEnumerable<FunctionSymbol> GetBuiltInFunctions() => typeof(BuiltInFunctions)
                                                                       .GetProperties(BindingFlags.Public | BindingFlags.Static)
                                                                       .Where(property => property.PropertyType == typeof(FunctionSymbol))
                                                                       .Select(property => (FunctionSymbol)property.GetValue(0)!);
}
