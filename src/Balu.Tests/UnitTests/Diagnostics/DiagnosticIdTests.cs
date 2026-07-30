using System;
using System.Collections.Generic;
using System.Linq;
using Balu.Diagnostics;
using Xunit;

namespace Balu.Tests.UnitTests.Diagnostics;

public class DiagnosticIdTests
{
    private static readonly (DiagnosticId Id, int Value)[] ExpectedValues =
    {
        (DiagnosticId.Lexer, 0),
        (DiagnosticId.UnexpectedToken, 1),
        (DiagnosticId.NumberNotValid, 2),
        (DiagnosticId.InvalidEscapeSequence, 3),
        (DiagnosticId.UnterminatedString, 4),
        (DiagnosticId.UnterminatedMultilineComment, 5),

        (DiagnosticId.Binder, 1000),
        (DiagnosticId.UnaryOperatorTypeMismtach, 1001),
        (DiagnosticId.BinaryOperatorTypeMismatch, 1002),
        (DiagnosticId.PrefixExpressionTypeMismatch, 1003),
        (DiagnosticId.PostfixExpressionTypeMismatch, 1004),
        (DiagnosticId.UndefinedName, 1005),
        (DiagnosticId.CannotConvert, 1006),
        (DiagnosticId.CannotConvertImplicit, 1007),
        (DiagnosticId.SymbolAlreadyDeclared, 1008),
        (DiagnosticId.SymbolHidesSymbol, 1009),
        (DiagnosticId.VariableIsReadOnly, 1010),
        (DiagnosticId.WrongNumberOfArguments, 1011),
        (DiagnosticId.WrongArgumentType, 1012),
        (DiagnosticId.ExpressionMustHaveValue, 1013),
        (DiagnosticId.SymbolNoVariable, 1014),
        (DiagnosticId.SymbolNoFunction, 1015),
        (DiagnosticId.UndefinedType, 1016),
        (DiagnosticId.UndefinedVariable, 1017),
        (DiagnosticId.UndefinedFunction, 1018),
        (DiagnosticId.ParameterAlreadyDeclared, 1019),
        (DiagnosticId.FunctionAlreadyDeclared, 1020),
        (DiagnosticId.InvalidBreakOrContinue, 1021),
        (DiagnosticId.MainCannotReturnValue, 1022),
        (DiagnosticId.ReturnMissingValue, 1023),
        (DiagnosticId.ReturnTypeMismatch, 1024),
        (DiagnosticId.NotAllPathsReturn, 1025),
        (DiagnosticId.InvalidExpressionStatement, 1026),
        (DiagnosticId.CannotMixMainANdGlobalStatements, 1027),
        (DiagnosticId.InvalidMainSignature, 1028),
        (DiagnosticId.OnlyOneFileCanHaveGlobalStatements, 1029),
        (DiagnosticId.NoEntryPointDefined, 1030),
        (DiagnosticId.UnreachableCode, 1031),
        (DiagnosticId.ConstantDivisionByZero, 1032),

        (DiagnosticId.Emitter, 2000),
        (DiagnosticId.InvalidAssemblyReference, 2001),
        (DiagnosticId.RequiredTypeNotFound, 2002),
        (DiagnosticId.RequiredTypeAmbiguous, 2003),
        (DiagnosticId.RequiredMethodNotFound, 2004),
        (DiagnosticId.SourceDocumentNameMissing, 2005),
        (DiagnosticId.SourceDocumentNameCollision, 2006),
        (DiagnosticId.EmitPathCollision, 2007)
    };

    [Theory]
    [MemberData(nameof(GetExpectedValues))]
    public void DiagnosticId_HasExpectedValue(DiagnosticId id, int expectedValue) => Assert.Equal(expectedValue, (int)id);

    [Fact]
    public void ExpectedValues_ContainsEveryDiagnosticId()
    {
        Assert.Equal(Enum.GetValues<DiagnosticId>(), ExpectedValues.Select(entry => entry.Id));
    }

    [Fact]
    public void DiagnosticId_ValuesAreUnique()
    {
        var values = Enum.GetValues<DiagnosticId>().Select(id => (int)id).ToArray();
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    public static IEnumerable<object[]> GetExpectedValues() =>
        ExpectedValues.Select(entry => new object[] { entry.Id, entry.Value });
}
