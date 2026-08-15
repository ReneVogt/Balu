using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Balu.SourceGenerator;

sealed class NodeModel
{
    internal static readonly DiagnosticDescriptor InvalidNodeModel = new(id: "BLS0001",
                                                                          title: "Node construction failure",
                                                                          messageFormat: "Cannot determine the constructor model for '{0}': {1}",
                                                                          category: "Balu source generation",
                                                                          DiagnosticSeverity.Error,
                                                                          isEnabledByDefault: true);

    readonly INamedTypeSymbol type;
    readonly IMethodSymbol constructor;

    internal ImmutableArray<(IParameterSymbol Parameter, IPropertySymbol Property)> Parameters { get; }

    NodeModel(INamedTypeSymbol type, IMethodSymbol constructor, ImmutableArray<(IParameterSymbol Parameter, IPropertySymbol Property)> parameters)
    {
        this.type = type;
        this.constructor = constructor;
        Parameters = parameters;
    }

    internal static NodeModel? Create(INamedTypeSymbol type, GeneratorExecutionContext context)
    {
        var constructors = type.InstanceConstructors
                               .Where(constructor => !constructor.IsImplicitlyDeclared)
                               .ToImmutableArray();
        if (constructors.Length == 0)
            constructors = type.InstanceConstructors;

        var properties = GetProperties(type);
        var candidates = constructors.Select(constructor => (Constructor: constructor, Parameters: MapParameters(constructor)))
                                     .Where(candidate => candidate.Parameters is not null)
                                     .ToImmutableArray();
        if (candidates.Length == 0)
        {
            var constructor = constructors.OrderByDescending(candidate => candidate.Parameters.Length).First();
            _ = TryMapParameters(constructor, out var reason);
            return ReportError(reason!);
        }

        var maximumParameterCount = candidates.Max(candidate => candidate.Constructor.Parameters.Length);
        var longestCandidates = candidates.Where(candidate => candidate.Constructor.Parameters.Length == maximumParameterCount).ToImmutableArray();
        if (longestCandidates.Length != 1)
            return ReportError($"{longestCandidates.Length} constructors with {maximumParameterCount} parameters match the node properties");

        var selectedCandidate = longestCandidates[0];
        return new NodeModel(type, selectedCandidate.Constructor, selectedCandidate.Parameters!.Value);

        ImmutableArray<(IParameterSymbol, IPropertySymbol)>? MapParameters(IMethodSymbol constructor) =>
            TryMapParameters(constructor, out _);

        ImmutableArray<(IParameterSymbol, IPropertySymbol)>? TryMapParameters(IMethodSymbol constructor, out string? reason)
        {
            var parameters = ImmutableArray.CreateBuilder<(IParameterSymbol, IPropertySymbol)>(constructor.Parameters.Length);
            foreach (var parameter in constructor.Parameters)
            {
                var matchingProperties = properties.Where(property =>
                                                              string.Equals(property.Name, parameter.Name, StringComparison.OrdinalIgnoreCase) &&
                                                              SymbolEqualityComparer.Default.Equals(property.Type, parameter.Type))
                                                   .ToImmutableArray();
                if (matchingProperties.Length != 1)
                {
                    reason = matchingProperties.Length == 0
                                 ? $"parameter '{parameter.Name}' has no property with the same name and type"
                                 : $"parameter '{parameter.Name}' matches multiple properties";
                    return null;
                }

                parameters.Add((parameter, matchingProperties[0]));
            }

            reason = null;
            return parameters.ToImmutable();
        }

        NodeModel? ReportError(string reason)
        {
            var location = type.Locations.FirstOrDefault(candidate => candidate.IsInSource);
            context.ReportDiagnostic(Diagnostic.Create(InvalidNodeModel, location, type.Name, reason));
            return null;
        }
    }

    internal bool ValidateAccessibility(GeneratorExecutionContext context,
                                        INamedTypeSymbol accessibleWithin,
                                        bool constructorRequired,
                                        ImmutableArray<IPropertySymbol> properties,
                                        string generatedContext)
    {
        if (constructorRequired && !context.Compilation.IsSymbolAccessibleWithin(constructor, accessibleWithin))
            return ReportError($"constructor with {constructor.Parameters.Length} parameters is not accessible from {generatedContext}");

        foreach (var property in properties)
            if (property.GetMethod is null || !context.Compilation.IsSymbolAccessibleWithin(property.GetMethod, accessibleWithin))
                return ReportError($"property '{property.Name}' has no getter accessible from {generatedContext}");

        return true;

        bool ReportError(string reason)
        {
            var location = type.Locations.FirstOrDefault(candidate => candidate.IsInSource);
            context.ReportDiagnostic(Diagnostic.Create(InvalidNodeModel, location, type.Name, reason));
            return false;
        }
    }

    static ImmutableArray<IPropertySymbol> GetProperties(INamedTypeSymbol type)
    {
        var properties = ImmutableArray.CreateBuilder<IPropertySymbol>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var current = type; current is not null; current = current.BaseType)
            foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
                if (names.Add(property.Name))
                    properties.Add(property);
        return properties.ToImmutable();
    }
}
