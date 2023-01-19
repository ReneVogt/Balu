
using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.Evaluation;

public class EvaluatorTests
{
    [Theory]
    [InlineData("", null)]
    [InlineData("42", 42)]
    [InlineData("-42", -42)]
    [InlineData("+--+3", 3)]
    [InlineData("+---3", -3)]
    [InlineData("~0", -1)]
    [InlineData("~1", -2)]
    [InlineData("~-1", 0)]
    [InlineData("~42", -43)]
    [InlineData("2+3", 5)]
    [InlineData("2-3", -1)]
    [InlineData("2*3", 6)]
    [InlineData("12/3", 4)]
    [InlineData("12/3+2", 6)]
    [InlineData("12/(4+2)", 2)]
    [InlineData("12*3+2", 38)]
    [InlineData("12*(3-5)", -24)]
    [InlineData("1|0", 1)]
    [InlineData("1|2", 3)]
    [InlineData("1&0", 0)]
    [InlineData("1&1", 1)]
    [InlineData("2&3", 2)]
    [InlineData("0^0", 0)]
    [InlineData("2^2", 0)]
    [InlineData("2^3", 1)]
    [InlineData("1^0", 1)]
    [InlineData("1^1", 0)]
    [InlineData("false", false)]
    [InlineData("true", true)]
    [InlineData("!false", true)]
    [InlineData("!true", false)]
    [InlineData("false && false", false)]
    [InlineData("true && false", false)]
    [InlineData("false && true", false)]
    [InlineData("true && true", true)]
    [InlineData("false || false", false)]
    [InlineData("true || false", true)]
    [InlineData("false || true", true)]
    [InlineData("true || true", true)]
    [InlineData("!true || true", true)]
    [InlineData("false || !false", true)]
    [InlineData("!(false && true)", true)]
    [InlineData("false|false", false)]
    [InlineData("false|true", true)]
    [InlineData("true|false", true)]
    [InlineData("true|true", true)]
    [InlineData("false&false", false)]
    [InlineData("false&true", false)]
    [InlineData("true&false", false)]
    [InlineData("true&true", true)]
    [InlineData("false^false", false)]
    [InlineData("false^true", true)]
    [InlineData("true^false", true)]
    [InlineData("true^true", false)]
    [InlineData("3 == 3", true)]
    [InlineData("3 == 4", false)]
    [InlineData("(true || false) == (false || true)", true)]
    [InlineData("true != false", true)]
    [InlineData("2 != 3", true)]
    [InlineData("3 != 3", false)]
    [InlineData("var a = 12 (a = a * 12)", 144)]
    [InlineData("var a = 10 for i=0 to (a = a - 1) {} a", 9)]
    [InlineData("12 < 3", false)]
    [InlineData("12 <= 3", false)]
    [InlineData("3 < 12", true)]
    [InlineData("3 <= 12", true)]
    [InlineData("12 > 3", true)]
    [InlineData("12 >= 3", true)]
    [InlineData("3 > 12", false)]
    [InlineData("3 >= 12", false)]
    [InlineData("5 < 5", false)]
    [InlineData("5 > 5", false)]
    [InlineData("5 <= 5", true)]
    [InlineData("5 >= 5", true)]
    [InlineData("\"escapedChars: \\r\\n \\v\\t\"", "escapedChars: \r\n \v\t")]
    [InlineData("\"hello \" + \"world\"", "hello world")]
    [InlineData("\"hello\" + \" \" + \"world\"", "hello world")]
    [InlineData("\"test\" == \"test\"", true)]
    [InlineData("\"test\" != \"test\"", false)]
    [InlineData("\"test\" == \"diff\"", false)]
    [InlineData("\"test\" != \"diff\"", true)]
    public void Evaluate_Expression_CorrectResults(string text, object expectedResult) => text.AssertEvaluation(value: expectedResult);

    [Theory]
    [InlineData("[!]1", "Unary operator '!' cannot be applied to type 'int'.")]
    [InlineData("[+]true", "Unary operator '+' cannot be applied to type 'bool'.")]
    [InlineData("[-]false", "Unary operator '-' cannot be applied to type 'bool'.")]
    [InlineData("[~]false", "Unary operator '~' cannot be applied to type 'bool'.")]
    public void Evaluate_UnaryOperator_Reports_TypeMismatch(string code, string? diagnostics) => code.AssertEvaluation(diagnostics);

    [Theory]
    [InlineData("1 [&] true", "Binary operator '&' cannot be applied to types 'int' and 'bool'.")]
    [InlineData("false [&] 2", "Binary operator '&' cannot be applied to types 'bool' and 'int'.")]
    [InlineData("1 [&&] true", "Binary operator '&&' cannot be applied to types 'int' and 'bool'.")]
    [InlineData("false [&&] 2", "Binary operator '&&' cannot be applied to types 'bool' and 'int'.")]
    [InlineData("1 [&&] 2", "Binary operator '&&' cannot be applied to types 'int' and 'int'.")]
    [InlineData("1 [|] true", "Binary operator '|' cannot be applied to types 'int' and 'bool'.")]
    [InlineData("false [|] 2", "Binary operator '|' cannot be applied to types 'bool' and 'int'.")]
    [InlineData("1 [||] true", "Binary operator '||' cannot be applied to types 'int' and 'bool'.")]
    [InlineData("false [||] 2", "Binary operator '||' cannot be applied to types 'bool' and 'int'.")]
    [InlineData("1 [||] 2", "Binary operator '||' cannot be applied to types 'int' and 'int'.")]
    [InlineData("1 [^] true", "Binary operator '^' cannot be applied to types 'int' and 'bool'.")]
    [InlineData("false [^] 2", "Binary operator '^' cannot be applied to types 'bool' and 'int'.")]
    [InlineData("1 [+] true", "Binary operator '+' cannot be applied to types 'int' and 'bool'.")]
    [InlineData("false [+] 2", "Binary operator '+' cannot be applied to types 'bool' and 'int'.")]
    [InlineData("true [+] false", "Binary operator '+' cannot be applied to types 'bool' and 'bool'.")]
    [InlineData("1 [-] true", "Binary operator '-' cannot be applied to types 'int' and 'bool'.")]
    [InlineData("false [-] 2", "Binary operator '-' cannot be applied to types 'bool' and 'int'.")]
    [InlineData("true [-] false", "Binary operator '-' cannot be applied to types 'bool' and 'bool'.")]
    [InlineData("1 [*] true", "Binary operator '*' cannot be applied to types 'int' and 'bool'.")]
    [InlineData("false [*] 2", "Binary operator '*' cannot be applied to types 'bool' and 'int'.")]
    [InlineData("true [*] false", "Binary operator '*' cannot be applied to types 'bool' and 'bool'.")]
    [InlineData("1 [/] true", "Binary operator '/' cannot be applied to types 'int' and 'bool'.")]
    [InlineData("false [/] 2", "Binary operator '/' cannot be applied to types 'bool' and 'int'.")]
    [InlineData("true [/] false", "Binary operator '/' cannot be applied to types 'bool' and 'bool'.")]
    [InlineData("1 [==] true", "Binary operator '==' cannot be applied to types 'int' and 'bool'.")]
    [InlineData("false [==] 2", "Binary operator '==' cannot be applied to types 'bool' and 'int'.")]
    [InlineData("1 [!=] true", "Binary operator '!=' cannot be applied to types 'int' and 'bool'.")]
    [InlineData("false [!=] 2", "Binary operator '!=' cannot be applied to types 'bool' and 'int'.")]
    [InlineData("1 [<] true", "Binary operator '<' cannot be applied to types 'int' and 'bool'.")]
    [InlineData("false [<] 2", "Binary operator '<' cannot be applied to types 'bool' and 'int'.")]
    [InlineData("false [<] true", "Binary operator '<' cannot be applied to types 'bool' and 'bool'.")]
    [InlineData("1 [<=] true", "Binary operator '<=' cannot be applied to types 'int' and 'bool'.")]
    [InlineData("false [<=] 2", "Binary operator '<=' cannot be applied to types 'bool' and 'int'.")]
    [InlineData("false [<=] true", "Binary operator '<=' cannot be applied to types 'bool' and 'bool'.")]
    [InlineData("1 [>] true", "Binary operator '>' cannot be applied to types 'int' and 'bool'.")]
    [InlineData("false [>] 2", "Binary operator '>' cannot be applied to types 'bool' and 'int'.")]
    [InlineData("false [>] true", "Binary operator '>' cannot be applied to types 'bool' and 'bool'.")]
    [InlineData("1 [>=] true", "Binary operator '>=' cannot be applied to types 'int' and 'bool'.")]
    [InlineData("false [>=] 2", "Binary operator '>=' cannot be applied to types 'bool' and 'int'.")]
    [InlineData("false [>=] true", "Binary operator '>=' cannot be applied to types 'bool' and 'bool'.")]
    public void Evaluate_BinaryOperator_Reports_TypeMismatch(string code, string? diagnostics) => code.AssertEvaluation(diagnostics);

    [Fact]
    public void Evaluate_Name_Reports_UndefinedName() => "var a = [bxy]".AssertEvaluation("Undefined name 'bxy'.");

    [Fact]
    public void Evaluate_Assignment_Reports_UndefinedName() => " [abc] = 12".AssertEvaluation("Undefined variable 'abc'.");
    [Fact]
    public void Evaluate_Assignment_Reports_ReadOnly() =>
        "{ let abc = 12 [abc] = 10 }".AssertEvaluation("Variable 'abc' is readonly and cannot be assigned to.");
    [Theory]
    [InlineData("{ var abc = 12 abc [=] false }", "Cannot convert 'bool' to 'int'.")]
    [InlineData("{ var abc = true abc [=] 17 }", "Cannot convert 'int' to 'bool'.")]
    public void Evaluate_Assignment_Reports_TypeMismatch(string code, string? diagnostics) => code.AssertEvaluation(diagnostics);


    [Fact]
    public void Evaluate_Call_Reports_UndefinedFunction()
    {
        const string text = "{ [unknown]() }";
        const string diagnostics = @"
            Undefined function 'unknown'.
";
        text.AssertEvaluation(diagnostics);
    }

    [Fact]
    public void Evaluate_BlockStatement_NoInfiniteLoopIfClosedBraceMissing()
    {
        const string text = "{[)][]";
        var diagnostics = $@"
            Unexpected ClosedParenthesisToken (')'), expected IdentifierToken.
            Unexpected EndOfFileToken ('{'\0'}'), expected ClosedBraceToken.";
        text.AssertEvaluation(diagnostics);
    }
    [Fact]
    public void Evaluate_CallExpression_NoInfiniteLoopIfClosedParenthesisMissing()
    {
        const string text = "{print([[}]]";
        const string diagnostics = @"
            Unexpected ClosedBraceToken ('}'), expected IdentifierToken.
            Unexpected ClosedBraceToken ('}'), expected ClosedParenthesisToken.
";
        text.AssertEvaluation(diagnostics);
    }

    [Fact]
    public void Evaluate_GlobalStatement_ReportsInvalidExpressionStatement()
    {
        const string text = "42 * 17 function test() { [2*12] do {[17*12] }while false} do {[1+1]} while false";
        const string diagnostics = @"
            Only assignment or call expressions can be used as a statement.
            Only assignment or call expressions can be used as a statement.
            Only assignment or call expressions can be used as a statement.
";
        text.AssertEvaluation(diagnostics);
    }

    [Fact]
    public void Evaluate_VariableDeclaration_Reports_Redeclaration()
    {
        const string text = @"
                {
                    var x = 10
                    var y = 100
                    {
                        var x = 10
                    }
                    var [x] = 5
                }
";
        const string diagnostics = @"
            Symbol 'x' is already declared.
";
        text.AssertEvaluation(diagnostics);
    }
    [Fact]
    public void Evaluate_VariableDeclaration_Reports_UnknownType()
    {
        const string text = "var x : [unknown] = 10";
        const string diagnostics = @"
            Undefined type 'unknown'.
";
        text.AssertEvaluation(diagnostics);
    }
    [Fact]
    public void Evaluate_VariableDeclaration_Reports_InvalidCast()
    {
        const string text = "var x : int [=] true";
        const string diagnostics = @"
            Cannot convert 'bool' to 'int'.
";
        text.AssertEvaluation(diagnostics);
    }

    [Theory]
    [InlineData("var a = 0 if true a = 1 a", 1)]
    [InlineData("var a = 0 if false a = 1 a", 0)]
    [InlineData("var a = 0 if true a = 1 else a = 2 a", 1)]
    [InlineData("var a = 0 if false a = 1 else a = 2 a", 2)]
    [InlineData("var a = 10 if a == 10 a = 5 a", 5)]
    [InlineData("var a = 10 if a != 10 a = 5 a", 10)]
    [InlineData("var a = 10 if a == 10 a = 5 else a = 20 a", 5)]
    [InlineData("var a = 10 if a != 10 a = 5 else a = 20 a", 20)]
    public void Evaluate_IfStatement_BasicallyWorks(string text, object? result) => text.AssertEvaluation(value: result);
    [Fact]
    public void Evaluate_IfStatement_Reports_WrongConditionType()
    {
        const string text = "if [(12 + 3)] {} else {}";
        const string diagnostics = @"
            Cannot convert 'int' to 'bool'.
";
        text.AssertEvaluation(diagnostics);
    }
    [Fact]
    public void Evaluate_ElseClause_Reports_UnexpectedToken()
    {
        const string text = @"
                    {
                        var x = 10
                        [else] x = 12
                    }
    ";
        const string diagnostics = @"
                Unexpected ElseKeyword ('else'), expected IdentifierToken.
    ";
        text.AssertEvaluation(diagnostics);
    }

    [Theory]
    [InlineData("var x = 0 while (x < 12) x = x + 1 x", 12)]
    [InlineData("var result = 1 var i = 0 while (i < 5) { i = i + 1 result = result * 2} result", 32)]
    [InlineData("var result = 0 var i = 0 while (i < 10) { i = i + 1 result = result + i} result", 55)]
    public void Evaluate_WhileStatement_BasicallyWorks(string text, object? result) => text.AssertEvaluation(value: result);
    [Fact]
    public void Evaluate_WhileStatement_Reports_WrongConditionType()
    {
        const string text = "{var a=0 while [(12 + 3)] a = a + 1 }";
        const string diagnostics = @"
            Cannot convert 'int' to 'bool'.
";
        text.AssertEvaluation(diagnostics);
    }

    [Theory]
    [InlineData("var x = 0 do x = x + 1 while (x < 12) x", 12)]
    [InlineData("var result = 1 var i = 0 do { i = i + 1 result = result * 2} while (i < 5) result", 32)]
    [InlineData("var result = 0 var i = 0 do { i = i + 1 result = result + i} while (i < 10) result", 55)]
    public void Evaluate_DoWhileStatement_BasicallyWorks(string text, object? result) => text.AssertEvaluation(value: result);
    [Fact]
    public void Evaluate_DoWhileStatement_Reports_WrongConditionType()
    {
        const string text = "{var a=0 do a = a + 1 while [(12 + 3)] }";
        const string diagnostics = @"
            Cannot convert 'int' to 'bool'.
";
        text.AssertEvaluation(diagnostics);
    }

    [Theory]
    [InlineData("var result = 0 for i=0 to 10 result=result+i result", 55)]
    public void Evaluate_ForStatement_BasicallyWorks(string text, object? result) => text.AssertEvaluation(value: result);
    [Fact]
    public void Evaluate_ForStatement_Reports_WrongBoundaryTypes()
    {
        const string text = "for i= [1>2] to [2>1] {}";
        const string diagnostics = @"
            Cannot convert 'bool' to 'int'.
            Cannot convert 'bool' to 'int'.
";
        text.AssertEvaluation(diagnostics);
    }

    [Fact]
    public void Evaluate_FunctionCall_LocalVariables()
    {
        @"
              var sum = 0
                
              function inner(x:int)
              {
                var b = x
                sum = 100*sum + b
              }

              inner(42) inner(17) sum".AssertEvaluation(value: 4217);
    }
    [Fact]
    public void Evaluate_FunctionCall_LocalVariablesNested()
    {
        @"
              var sum = 0 var sumx = 0
                
              function inner(x:int)
              {
                var b = x
                sum = 100*sum + b
              }
              function outer(x: int)
              {
                 var b = x
                 inner(x)
                 sumx = 100*sumx + 2*b
              }

              outer(42) outer(17) 10000*sum + sumx".AssertEvaluation(value: 42178434);
    }
    [Fact]
    public void Evaluate_FunctionCall_Recursion()
    {
        @"
              var sum = 0
                
              function inner(x:int)
              {
                var b = x
                if x > 0 inner(x-1)
                sum = sum + b
              }

              inner(5) sum".AssertEvaluation(value: 15);
    }
    [Fact]
    public void Evaluate_FunctionCall_RecursionNested()
    {
        @"
              var sum = 0 var sumb = 0
                
              function inner(x:int)
              {
                var b = x
                if x > 0 inner(x-1)
                sum = sum + b
              }
              function outer(x:int) { 
                var b = x
                inner(5)
                sumb = 100*sum + b                              
              }

              outer(42) sumb".AssertEvaluation(value: 1542);
    }

    [Fact]
    public void Evaluate_FunctionCall_InsideLoop()
    {
        "function test() {} var result = true while result { test() result = false } result ".AssertEvaluation(value: false);
    }
    [Fact]
    public void Evaluate_FunctionCall_ReturnInsideLoop()
    {
        "function increase(i:int) : int { return i+1 } var result = 0 while result < 12 { result = increase(result) } result".AssertEvaluation(value: 12);
    }

    [Fact]
    public void Evaluate_Function_ReportsMissingName()
    {
        "function [(]) : int { var i = 0 return i }".AssertEvaluation(" Unexpected OpenParenthesisToken ('('), expected IdentifierToken.");
    }

    [Fact]
    public void Evaluate_Return_AllowedGloballyInScript()
    {
        "{ var i = 5 return i }".AssertEvaluation(value: 5);
    }
    [Fact]
    public void Evaluate_Return_ReportsUnexpectedExpression()
    {
        "function test() { return [[25]] }".AssertEvaluation(@"
            Cannot convert 'int' to 'void'.
            'test' does not have a return type and cannot return a value of type 'int'.");
    }
    [Fact]
    public void Evaluate_Return_ReportsMissingExpression()
    {
        @"
            function test() : int 
            { 
                [return] 
            }".AssertEvaluation("'test' needs to return a value of type 'int'.");
    }
    [Fact]
    public void Evaluate_Return_ReportsUnexpectedTokenIfEspressionIsMissing()
    {
        @"
            function test() : int 
            { 
                return [[}]]".AssertEvaluation(@"
                    Unexpected ClosedBraceToken ('}'), expected IdentifierToken.
                    'test' needs to return a value of type 'int', not '?'.
");
    }
    [Fact]
    public void Evaluate_Return_ReportsWrongExpressionType()
    {
        "function test() : int { return [[true]] }".AssertEvaluation(@"
                Cannot convert 'bool' to 'int'.
                'test' needs to return a value of type 'int', not 'bool'.");
    }
    [Fact]
    public void Evaluate_Return_ReportsNotAllPathsReturn()
    {
        "function test() : int { if false return 0 [}]".AssertEvaluation("Not all code paths of function 'test' return a value of type 'int'.");
    }
    [Fact]
    public void Evaluate_Return_ReportsNotAllPathsReturnForEmptyFunction()
    {
        "function test() : int { [}]".AssertEvaluation("Not all code paths of function 'test' return a value of type 'int'.");
    }
    [Fact]
    public void Evaluate_Return_DetectsDeadPaths()
    {
        "function test() : int { if true return 47 } test()".AssertEvaluation(value: 47);
    }

    [Fact]
    public void Evaluate_Break_BreaksCorrectLoop()
    {
        @"
            var result = 0
            for i = 1 to 10
            {
                if i > 5 break
                for j = 11 to 15
                {
                   if (j > 13) break
                   result = result + i + j
                }
            }
            result
         ".AssertEvaluation(value: 225);

    }
    [Fact]
    public void Evaluate_Continue_ContinuesCorrectLoop()
    {
        @"
            var result = 0
            for i = 1 to 10
            {
                if i/2*2 == i continue
                for j = 11 to 15
                {
                   if j == 13 || j == 14 continue
                   result = result + i + j
                }
             }
             result            
        ".AssertEvaluation(value: 265);

    }

    [Theory]
    [InlineData("{ [print] = 12 }", "Unexpected symbol kind 'Function', expected 'print' to be a variable or argument.")]
    [InlineData("{ var a = 7 [a](12) }", "Unexpected symbol kind 'GlobalVariable', expected 'a' to be a function.")]
    public void Evaluate_Reports_SymbolTypeMisatch(string text, string diagnostics) => text.AssertEvaluation(diagnostics);

    [Fact]
    public void Evaluate_ConstantFolding_DoesNotRemoveSideEffects()
    {
        @"
            var result = false
            function SetResultAndReturnTrue() : bool
            {
                result = true
                return true
            }
            var test = SetResultAndReturnTrue() && false
            result
        ".AssertEvaluation(value: true);
    }
}