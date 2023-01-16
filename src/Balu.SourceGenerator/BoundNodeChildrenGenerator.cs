using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Balu.SourceGenerator;

sealed class BoundNodeChildrenGenerator : BaseGenerator
{
    readonly CSharpCompilation compilation;
    readonly INamedTypeSymbol boundNodeType, immutableArrayType;

    internal BoundNodeChildrenGenerator(CSharpCompilation compilation, INamedTypeSymbol boundNodeType, INamedTypeSymbol immutableArrayType)
    {
        this.compilation = compilation;
        this.boundNodeType = boundNodeType;
        this.immutableArrayType = immutableArrayType;
    }

    public override void Generate(GeneratorExecutionContext context)
    {
        Writer.WriteLine("using System;");
        Writer.WriteLine("using System.Collections.Generic;");
        Writer.WriteLine("using System.Collections.Immutable;");
        Writer.WriteLine();
        Writer.WriteLine("namespace Balu.Binding;");

        var types = compilation.Assembly.GetAllTypes();
        var boundNodeTypes = types.Where(t => !t.IsAbstract && t.IsPartial() && t.IsDerivedFrom(boundNodeType) && SymbolEqualityComparer.Default.Equals(boundNodeType.ContainingNamespace, t.ContainingNamespace));

        foreach (var type in boundNodeTypes.TakeWhile(_ => !context.CancellationToken.IsCancellationRequested))
                WriteType(type);

        context.AddSource(
            "BoundNodeChildren.g.cs",
            SourceText.From(Writer.InnerWriter.ToString(), Encoding.UTF8));
    }
    void WriteType(INamedTypeSymbol type)
    {
        using(new CurlyIndenter(Writer, $"partial class {type.Name}"))
        {
            WriteChildrenCount(type);
            WriteGetChild(type);
        }
    }
    void WriteChildrenCount(INamedTypeSymbol type)
    {
        var properties = type.GetMembers()
                             .OfType<IPropertySymbol>()
                             .Where(property => property.Type is INamedTypeSymbol propertyType &&
                                                propertyType.IsDerivedFrom(boundNodeType))
                             .ToImmutableArray();
        var nonNullableProperties = properties.Where(property => property.NullableAnnotation != NullableAnnotation.Annotated).ToImmutableArray();
        var nullableProperties = properties.Where(property => property.NullableAnnotation == NullableAnnotation.Annotated).ToImmutableArray();
        var collections = type.GetMembers()
                              .OfType<IPropertySymbol>()
                              .Where(property => property.Type is INamedTypeSymbol propertyType && propertyType.IsGenericListOf(immutableArrayType, boundNodeType))
                              .ToImmutableArray();

        if (nullableProperties.Length == 0)
        {
            Writer.Write("public override int ChildrenCount => ");
            WriteNonNullableSum();
            Writer.WriteLine(";");
            return;
        }

        if (nonNullableProperties.Length + collections.Length == 0 && nullableProperties.Length == 1)
        {
            Writer.WriteLine($"public override int ChildrenCount => {nullableProperties[0].Name} is null ? 0 : 1;");
            return;
        }

        using(new CurlyIndenter(Writer, "public override int ChildrenCount"))
        {
            using(new CurlyIndenter(Writer, "get"))
            {
                Writer.Write("int count = ");
                WriteNonNullableSum();
                Writer.WriteLine(";");
                Writer.WriteLine(
                    string.Join(Environment.NewLine, nullableProperties.Select(property => $"if ({property.Name} is not null) count++;")));
                Writer.WriteLine("return count;");
            }
        }

        void WriteNonNullableSum()
        {
            if (nonNullableProperties.Length > 0)
            {
                Writer.Write(nonNullableProperties.Length);
                if (collections.Length > 0) Writer.Write(" + ");
            }
            else if (collections.Length == 0) Writer.Write("0");
            Writer.Write(string.Join(" + ", collections.Select(collection => $"{collection.Name}.Length")));
        }
    }
    void WriteGetChild(INamedTypeSymbol type)
    {
        const string signature = "public override Balu.Binding.BoundNode GetChild(int index)";
        const string exception = "throw new ArgumentOutOfRangeException(\"index\")";

        var properties = type.GetMembers()
                             .OfType<IPropertySymbol>()
                             .Where(property => property.Type is INamedTypeSymbol propertyType &&
                                                (propertyType.IsDerivedFrom(boundNodeType) ||
                                                 propertyType.IsGenericListOf(immutableArrayType, boundNodeType))).ToImmutableArray();

        if (properties.Length == 0)
        {
            Writer.WriteLine($"{signature} => {exception};");
            return;
        }

        if (properties.Length == 1)
        {
            var property = properties[0];
            if (((INamedTypeSymbol)property.Type).IsDerivedFrom(boundNodeType))
            {
                Writer.Write(signature + " => ");
                if (property.NullableAnnotation == NullableAnnotation.Annotated)
                    Writer.Write($"{property.Name} is not null && ");
                Writer.WriteLine($"index == 0 ? {property.Name} : {exception};");
            }
            else
                Writer.WriteLine($"{signature} => {property.Name}[index];");

            return;
        }

        using(new CurlyIndenter(Writer, signature))
        {
            int propIndex = 0;
            var leadingNonNullableProperties = properties
                                               .TakeWhile(property => ((INamedTypeSymbol)property.Type).IsDerivedFrom(boundNodeType) &&
                                                                      property.NullableAnnotation != NullableAnnotation.Annotated)
                                               .ToImmutableArray();
            if (properties.Any(property => ((INamedTypeSymbol)property.Type).IsDerivedFrom(boundNodeType) &&
                                           property.NullableAnnotation == NullableAnnotation.Annotated))
                Writer.WriteLine($"if (index < 0) {exception};");

            if (leadingNonNullableProperties.Length == 1)
            {
                Writer.WriteLine($"if (index == 0) return {leadingNonNullableProperties[0].Name};");
                propIndex++;
            }
            else if (leadingNonNullableProperties.Length > 1)
            {
                using(new CurlyIndenter(Writer, "switch(index)"))
                {
                    foreach (var property in leadingNonNullableProperties)
                        Writer.WriteLine($"case {propIndex++}: return {property.Name};");
                }
            }

            if (leadingNonNullableProperties.Length < properties.Length)
                Writer.WriteLine($"int adjustedIndex = index, propIndex = {propIndex};");
            for (int i = leadingNonNullableProperties.Length; i < properties.Length; i++)
            {
                var property = properties[i];
                var propertyType = (INamedTypeSymbol)property.Type;
                if (propertyType.IsDerivedFrom(boundNodeType))
                {
                    if (propertyType.NullableAnnotation == NullableAnnotation.Annotated)
                    {
                        Writer.WriteLine($"if ({property.Name} is null) adjustedIndex++;");
                        Writer.WriteLine($"else if (adjustedIndex == propIndex) return {property.Name};");
                    }
                    else
                        Writer.WriteLine($"if (adjustedIndex == propIndex) return {property.Name};");

                    if (i < properties.Length - 1) Writer.WriteLine("propIndex++;");
                }
                else
                {
                    Writer.WriteLine($"if (adjustedIndex - propIndex < {property.Name}.Length) return {property.Name}[adjustedIndex-propIndex];");
                    if (i < properties.Length - 1) Writer.WriteLine($"propIndex += {property.Name}.Length;");
                }
            }

            Writer.WriteLine($"{exception};");
        }
    }
}