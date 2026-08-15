using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Balu.SourceGenerator;

sealed class NodeKindMapping
{
    internal static readonly DiagnosticDescriptor UnmatchedNode = new(id: "BLS0003",
                                                                       title: "Unmatched node type",
                                                                       messageFormat: "The node type '{0}' has no matching member in '{1}'.",
                                                                       category: "Balu source generation",
                                                                       DiagnosticSeverity.Error,
                                                                       isEnabledByDefault: true);
    internal static readonly DiagnosticDescriptor UnmatchedKind = new(id: "BLS0004",
                                                                       title: "Unmatched node kind",
                                                                       messageFormat: "The kind '{0}' has no matching concrete node type '{1}'.",
                                                                       category: "Balu source generation",
                                                                       DiagnosticSeverity.Error,
                                                                       isEnabledByDefault: true);
    internal static readonly DiagnosticDescriptor DuplicateNodes = new(id: "BLS0005",
                                                                        title: "Duplicate node implementations",
                                                                        messageFormat: "The kind '{0}' is implemented by multiple node types: {1}.",
                                                                        category: "Balu source generation",
                                                                        DiagnosticSeverity.Error,
                                                                        isEnabledByDefault: true);
    internal static readonly DiagnosticDescriptor MismatchedKind = new(id: "BLS0006",
                                                                        title: "Mismatched node kind",
                                                                        messageFormat: "The node type '{0}' returns kind '{1}', but its name maps to '{2}'.",
                                                                        category: "Balu source generation",
                                                                        DiagnosticSeverity.Error,
                                                                        isEnabledByDefault: true);
    internal static readonly DiagnosticDescriptor DuplicateKindValues = new(id: "BLS0007",
                                                                             title: "Duplicate node kind values",
                                                                             messageFormat: "The kinds {0} have the duplicate value '{1}'.",
                                                                             category: "Balu source generation",
                                                                             DiagnosticSeverity.Error,
                                                                             isEnabledByDefault: true);

    internal ImmutableArray<INamedTypeSymbol> Nodes { get; }
    internal ImmutableArray<(IFieldSymbol Kind, INamedTypeSymbol Node)> Mappings { get; }

    NodeKindMapping(ImmutableArray<INamedTypeSymbol> nodes,
                    ImmutableArray<(IFieldSymbol Kind, INamedTypeSymbol Node)> mappings)
    {
        Nodes = nodes;
        Mappings = mappings;
    }

    internal static NodeKindMapping Create(GeneratorExecutionContext context,
                                           ImmutableArray<INamedTypeSymbol> allTypes,
                                           INamedTypeSymbol baseNodeType,
                                           INamedTypeSymbol kindType,
                                           string nodePrefix,
                                           string nodeSuffix,
                                           Func<IFieldSymbol, bool> isDispatchKind,
                                           INamedTypeSymbol? specialNode = null)
    {
        var concreteNodes = allTypes.Where(type => !type.IsAbstract && type.IsDerivedFrom(baseNodeType)).ToImmutableArray();
        var nodes = concreteNodes.Where(type => type.IsSupportedNodeType()).ToImmutableArray();
        var dispatchNodes = nodes.Where(type => !SymbolEqualityComparer.Default.Equals(type, specialNode)).ToImmutableArray();
        var allDispatchNodes = concreteNodes.Where(type => !SymbolEqualityComparer.Default.Equals(type, specialNode)).ToImmutableArray();
        var kinds = kindType.GetMembers()
                            .OfType<IFieldSymbol>()
                            .Where(field => field.HasConstantValue && isDispatchKind(field))
                            .ToImmutableArray();
        var invalidKinds = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);

        foreach (var duplicateGroup in kinds.GroupBy(field => field.ConstantValue).Where(group => group.Count() > 1))
        {
            var duplicateKinds = duplicateGroup.ToImmutableArray();
            foreach (var kind in duplicateKinds)
                invalidKinds.Add(kind);

            var names = string.Join(", ", duplicateKinds.Select(kind => $"'{kind.ToDisplayString()}'"));
            context.ReportDiagnostic(Diagnostic.Create(DuplicateKindValues,
                                                       duplicateKinds[0].Locations.FirstOrDefault(location => location.IsInSource),
                                                       names,
                                                       duplicateGroup.Key));
        }

        var mappings = ImmutableArray.CreateBuilder<(IFieldSymbol Kind, INamedTypeSymbol Node)>();
        var matchedNodes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var kind in kinds)
        {
            var expectedNodeName = $"{nodePrefix}{kind.Name}{nodeSuffix}";
            var allCandidates = allDispatchNodes.Where(node => node.Name == expectedNodeName).ToImmutableArray();
            var candidates = dispatchNodes.Where(node => node.Name == expectedNodeName).ToImmutableArray();
            foreach (var candidate in candidates)
                matchedNodes.Add(candidate);

            if (candidates.Length == 0)
            {
                if (allCandidates.Length > 0) continue;

                context.ReportDiagnostic(Diagnostic.Create(UnmatchedKind,
                                                           kind.Locations.FirstOrDefault(location => location.IsInSource),
                                                           kind.ToDisplayString(),
                                                           expectedNodeName));
                continue;
            }

            if (candidates.Length > 1)
            {
                context.ReportDiagnostic(Diagnostic.Create(DuplicateNodes,
                                                           kind.Locations.FirstOrDefault(location => location.IsInSource),
                                                           kind.ToDisplayString(),
                                                           string.Join(", ", candidates.Select(candidate => $"'{candidate.ToDisplayString()}'"))));
                continue;
            }

            var node = candidates[0];
            var returnedKind = TryGetReturnedKind(context.Compilation, node, kindType);
            if (returnedKind is not null && !Equals(returnedKind.Value.ConstantValue, kind.ConstantValue))
            {
                context.ReportDiagnostic(Diagnostic.Create(MismatchedKind,
                                                           GetKindLocation(node),
                                                           node.ToDisplayString(),
                                                           returnedKind.Value.Display,
                                                           kind.ToDisplayString()));
                continue;
            }

            if (!invalidKinds.Contains(kind))
                mappings.Add((kind, node));
        }

        foreach (var node in dispatchNodes.Where(node => !matchedNodes.Contains(node)))
            context.ReportDiagnostic(Diagnostic.Create(UnmatchedNode,
                                                       node.Locations.FirstOrDefault(location => location.IsInSource),
                                                       node.ToDisplayString(),
                                                       kindType.ToDisplayString()));

        return new NodeKindMapping(nodes, mappings.ToImmutable());
    }

    static (object? ConstantValue, string Display)? TryGetReturnedKind(Compilation compilation,
                                                                       INamedTypeSymbol node,
                                                                       INamedTypeSymbol kindType)
    {
        IPropertySymbol? property = null;
        for (var current = node; current is not null && property is null; current = current.BaseType)
            property = current.GetMembers("Kind").OfType<IPropertySymbol>().FirstOrDefault();
        if (property is null) return null;

        foreach (var syntaxReference in property.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not PropertyDeclarationSyntax declaration) continue;

            ExpressionSyntax? expression = declaration.ExpressionBody?.Expression;
            if (expression is null)
            {
                var getter = declaration.AccessorList?.Accessors.FirstOrDefault(accessor => accessor.Keyword.ValueText == "get");
                expression = getter?.ExpressionBody?.Expression;
                if (expression is null && getter?.Body?.Statements.Count == 1 && getter.Body.Statements[0] is ReturnStatementSyntax returnStatement)
                    expression = returnStatement.Expression;
            }

            if (expression is null) continue;
            var semanticModel = compilation.GetSemanticModel(declaration.SyntaxTree);
            var constant = semanticModel.GetConstantValue(expression);
            if (!constant.HasValue) continue;

            var field = kindType.GetMembers()
                                .OfType<IFieldSymbol>()
                                .FirstOrDefault(candidate => candidate.HasConstantValue && Equals(candidate.ConstantValue, constant.Value));
            return (constant.Value, field?.ToDisplayString() ?? expression.ToString());
        }

        return null;
    }

    static Location? GetKindLocation(INamedTypeSymbol node) =>
        node.GetMembers("Kind")
            .OfType<IPropertySymbol>()
            .SelectMany(property => property.Locations)
            .FirstOrDefault(location => location.IsInSource) ??
        node.Locations.FirstOrDefault(location => location.IsInSource);
}
