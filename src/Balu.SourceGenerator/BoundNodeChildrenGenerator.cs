using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Balu.SourceGenerator;

sealed class BoundNodeChildrenGenerator : BaseGenerator
{
    readonly ImmutableArray<INamedTypeSymbol> nodes;
    readonly INamedTypeSymbol boundNodeType, immutableArrayType;

    internal BoundNodeChildrenGenerator(ImmutableArray<INamedTypeSymbol> nodes, INamedTypeSymbol boundNodeType, INamedTypeSymbol immutableArrayType)
    {
        this.nodes = nodes;
        this.boundNodeType = boundNodeType;
        this.immutableArrayType = immutableArrayType;
    }

    public override void Generate(GeneratorExecutionContext context)
    {
        Writer.WriteLine("using System;");
        Writer.WriteLine("using System.Collections.Generic;");
        Writer.WriteLine("using System.Collections.Immutable;");
        Writer.WriteLine();
        foreach (var namespaceGroup in nodes.Where(type => type.IsPartial())
                                            .GroupBy(type => type.ContainingNamespace.GetFullName())
                                            .TakeWhile(_ => !context.CancellationToken.IsCancellationRequested))
        {
            if (namespaceGroup.Key.Length > 0)
            {
                Writer.WriteLine($"namespace {namespaceGroup.Key}");
                using (new CurlyIndenter(Writer))
                    foreach (var type in namespaceGroup)
                        WriteType(type, context);
            }
            else
            {
                foreach (var type in namespaceGroup)
                    WriteType(type, context);
            }
        }

        context.AddSource(
            "BoundNodeChildren.g.cs",
            SourceText.From(Writer.InnerWriter.ToString(), Encoding.UTF8));
    }
    void WriteType(INamedTypeSymbol type, GeneratorExecutionContext context)
    {
        var model = NodeModel.Create(type, context);
        if (model is null) return;

        var properties = model.Parameters
                              .Select(parameter => parameter.Property)
                              .Where(property => property.Type is INamedTypeSymbol propertyType &&
                                                 (propertyType.IsDerivedFrom(boundNodeType) ||
                                                  propertyType.IsGenericListOf(immutableArrayType, boundNodeType)))
                              .ToImmutableArray();
        if (!model.ValidateAccessibility(context, type, constructorRequired: false, properties, "the generated bound node members")) return;

        using(new CurlyIndenter(Writer, $"partial class {type.ToIdentifier()}"))
        {
            WriteChildrenCount(properties);
            WriteGetChild(properties);
        }
    }
    void WriteChildrenCount(ImmutableArray<IPropertySymbol> properties)
    {
        var nodeProperties = properties.Where(property => ((INamedTypeSymbol)property.Type).IsDerivedFrom(boundNodeType)).ToImmutableArray();
        var nonNullableProperties = nodeProperties.Where(property => property.NullableAnnotation != NullableAnnotation.Annotated).ToImmutableArray();
        var nullableProperties = nodeProperties.Where(property => property.NullableAnnotation == NullableAnnotation.Annotated).ToImmutableArray();
        var collections = properties.Where(property => ((INamedTypeSymbol)property.Type).IsGenericListOf(immutableArrayType, boundNodeType)).ToImmutableArray();

        if (nullableProperties.Length == 0)
        {
            Writer.Write("public override int ChildrenCount => ");
            WriteNonNullableSum();
            Writer.WriteLine(";");
            return;
        }

        if (nonNullableProperties.Length + collections.Length == 0 && nullableProperties.Length == 1)
        {
            Writer.WriteLine($"public override int ChildrenCount => {nullableProperties[0].ToIdentifier()} is null ? 0 : 1;");
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
                    string.Join(Environment.NewLine, nullableProperties.Select(property => $"if ({property.ToIdentifier()} is not null) count++;")));
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
            Writer.Write(string.Join(" + ", collections.Select(collection => $"{collection.ToIdentifier()}.Length")));
        }
    }
    void WriteGetChild(ImmutableArray<IPropertySymbol> properties)
    {
        const string signature = "public override Balu.Binding.BoundNode GetChild(int index)";
        const string exception = "throw new ArgumentOutOfRangeException(\"index\")";

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
                    Writer.Write($"{property.ToIdentifier()} is not null && ");
                Writer.WriteLine($"index == 0 ? {property.ToIdentifier()} : {exception};");
            }
            else
                Writer.WriteLine($"{signature} => {property.ToIdentifier()}[index];");

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
                Writer.WriteLine($"if (index == 0) return {leadingNonNullableProperties[0].ToIdentifier()};");
                propIndex++;
            }
            else if (leadingNonNullableProperties.Length > 1)
            {
                using(new CurlyIndenter(Writer, "switch(index)"))
                {
                    foreach (var property in leadingNonNullableProperties)
                        Writer.WriteLine($"case {propIndex++}: return {property.ToIdentifier()};");
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
                        Writer.WriteLine($"if ({property.ToIdentifier()} is null) adjustedIndex++;");
                        Writer.WriteLine($"else if (adjustedIndex == propIndex) return {property.ToIdentifier()};");
                    }
                    else
                        Writer.WriteLine($"if (adjustedIndex == propIndex) return {property.ToIdentifier()};");

                    if (i < properties.Length - 1) Writer.WriteLine("propIndex++;");
                }
                else
                {
                    Writer.WriteLine($"if (adjustedIndex - propIndex < {property.ToIdentifier()}.Length) return {property.ToIdentifier()}[adjustedIndex-propIndex];");
                    if (i < properties.Length - 1) Writer.WriteLine($"propIndex += {property.ToIdentifier()}.Length;");
                }
            }

            Writer.WriteLine($"{exception};");
        }
    }
}
