﻿namespace Balu.Diagnostics;

public enum DiagnosticId
{
    Lexer = 0,
    UnexpectedToken,
    NumberNotValid,
    InvalidEscapeSequence,
    UnterminatedString,
    UnterminatedMultilineComment,

    Binder = 1000,
    UnaryOperatorTypeMismtach,
    BinaryOperatorTypeMismatch,
    UndefinedName,
    CannotConvert,
    CannotConvertImplicit,
    SymbolAlreadyDeclared,
    VariableIsReadOnly,
    WrongNumberOfArguments,
    ExpressionMustHaveValue,
    SymbolNoVariable,
    SymbolNoFunction,
    UndefinedType,
    UndefinedVariable,
    UndefinedFunction,
    ParameterAlreadyDeclared,
    FunctionAlreadyDeclared,
    InvalidBreakOrContinue,
    MainCannotReturnValue,
    ReturnMissingValue,
    ReturnTypeMismatch,
    NotAllPathsReturn,
    InvalidExpressionStatement,
    CannotMixMainANdGlobalStatements,
    InvalidMainSignature,
    OnlyOneFileCanHaveGlobalStatements,
    NoEntryPointDefined,
    UnreachableCode,

    Emitter = 2000,
    InvalidAssemblyReference,
    RequiredTypeNotFound,
    RequiredTypeAmbiguous,
    RequiredMethodNotFound
}