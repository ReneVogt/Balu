# Language reference

This document describes the language implemented by the current Balu compiler.
It is a descriptive reference, not a versioned or normative specification.
Implementation details that may require a future language-design decision are
identified as such.

## Source text

Balu source files conventionally use the `.b` extension. The command-line
compiler does not enforce the extension.

Whitespace and comments separate tokens but are otherwise ignored. Line breaks
usually have no syntactic meaning. The exception is the expression following a
`return`, which must begin on the same source line as `return`.

Statements have no semicolon terminator.

### Comments

Single-line comments begin with `//` and continue to the end of the line:

```balu
// A single-line comment
var value = 1 // A trailing comment
```

Multi-line comments begin with `/*` and end with `*/`:

```balu
/* A comment that
   spans multiple lines. */
```

Multi-line comments do not nest. An unclosed multi-line comment is a lexical
error.

## Identifiers and keywords

An identifier begins with a letter or underscore. Further characters may be
letters, digits, or underscores. The lexer uses the .NET Unicode definitions of
letters and digits.

Identifiers and keywords are case-sensitive. `value` and `Value` are different
names.

The reserved keywords are:

```text
break       continue    do          else
false       for         function    if
let         return      to          true
var         while
```

The type names `int`, `bool`, `string`, and `any` are lexed as identifiers and
interpreted as types in type and conversion contexts.

## Literals

### Integer literals

Integer literals are decimal sequences of digits:

```balu
0
42
1000000
```

They represent signed 32-bit `int` values. There are no hexadecimal, binary, or
octal forms and no digit separators. A leading sign is a separate unary
operator rather than part of the literal.

An integer outside the range accepted by the lexer produces a diagnostic.
Because a leading minus is a separate token, the positive magnitude is parsed
first. Consequently, `-2147483648` cannot currently be written directly: the
`2147483648` token is already outside the accepted literal range.

### Boolean literals

The Boolean literals are `true` and `false`.

### String literals

String literals use double quotes:

```balu
"Balu"
""
"first line\nsecond line"
```

The supported escape sequences are:

| Escape | Value |
| --- | --- |
| `\r` | Carriage return |
| `\n` | Line feed |
| `\t` | Horizontal tab |
| `\v` | Vertical tab |
| `\\` | Backslash |
| `\"` | Double quote |

A string literal cannot contain an unescaped source line break. Unsupported
escape sequences and unclosed strings produce diagnostics.

## Grammar

The following EBNF summarizes the implemented grammar. `identifier`, `number`,
and `string` represent lexical tokens.

```ebnf
compilation-unit       = { member }, end-of-file ;

member                 = function-declaration
                       | statement ;

function-declaration   = "function", identifier,
                         "(", [ parameter-list ], ")",
                         [ type-clause ], block-statement ;

parameter-list         = parameter, { ",", parameter } ;
parameter              = identifier, type-clause ;
type-clause            = ":", identifier ;

statement              = block-statement
                       | variable-declaration
                       | if-statement
                       | while-statement
                       | do-while-statement
                       | for-statement
                       | break-statement
                       | continue-statement
                       | return-statement
                       | expression-statement ;

block-statement        = "{", { statement }, "}" ;

variable-declaration   = ( "let" | "var" ), identifier,
                         [ type-clause ], "=", expression ;

if-statement           = "if", expression, statement,
                         [ "else", statement ] ;

while-statement        = "while", expression, statement ;
do-while-statement     = "do", statement, "while", expression ;

for-statement          = "for", identifier, "=", expression,
                         "to", expression, statement ;

break-statement        = "break" ;
continue-statement     = "continue" ;
return-statement       = "return", [ expression-on-the-same-line ] ;

expression-statement   = expression ;

expression             = assignment-expression
                       ;

assignment-expression  = identifier, assignment-operator, expression
                       | binary-expression ;
assignment-operator    = "=" | "+=" | "-=" | "*=" | "/="
                       | "&=" | "|=" | "^=" ;

binary-expression      = unary-expression,
                         { binary-operator, unary-expression } ;
binary-operator        = "+" | "-" | "*" | "/"
                       | "&" | "&&" | "|" | "||" | "^"
                       | "==" | "!=" | "<" | "<=" | ">" | ">=" ;

unary-expression       = unary-operator, unary-expression
                       | primary-expression ;
unary-operator         = "+" | "-" | "!" | "~" ;

primary-expression     = number | string | "true" | "false"
                       | identifier
                       | "(", expression, ")"
                       | call-expression
                       | prefix-expression
                       | postfix-expression ;

call-expression        = identifier, "(", [ argument-list ], ")" ;
argument-list          = expression, { ",", expression } ;
prefix-expression      = ( "++" | "--" ), identifier ;
postfix-expression     = identifier, ( "++" | "--" ) ;
```

The compact `binary-expression` production does not encode precedence.
Precedence and associativity are defined by the table in
[Precedence and associativity](#precedence-and-associativity).

Function and call argument lists do not allow a trailing comma. Function
declarations are only recognized at the compilation-unit level; functions
cannot be declared inside blocks or other functions.

## Types

### Source types

| Type | Meaning | CLR representation |
| --- | --- | --- |
| `int` | Signed 32-bit integer | `System.Int32` |
| `bool` | Boolean value | `System.Boolean` |
| `string` | String value | `System.String` |
| `any` | A boxed or reference value without its more specific static type | `System.Object` |

`void` represents the absence of a return value. It is assigned internally to
functions without a return-type clause, but `void` is not currently recognized
as a source type. Write `function run() { ... }`, not
`function run(): void { ... }`.

There is no `null` literal.

### Type inference

A variable declaration without a type clause takes the static type of its
initializer:

```balu
var count = 1          // int
let message = "hello" // string
```

Function parameters always require a type. Function return types are never
inferred: omitting the return type declares a function that returns no value.

### Conversions

Identity conversions exist from every type to itself.

The following conversions are implicit:

| From | To |
| --- | --- |
| `int` | `any` |
| `bool` | `any` |
| `string` | `any` |

The following conversions are explicit:

| From | To |
| --- | --- |
| `any` | `int`, `bool`, or `string` |
| `int` | `string` |
| `bool` | `string` |
| `string` | `int` or `bool` |

An explicit conversion uses call-like syntax:

```balu
var text = string(42)
var number = int("42")
var flag = bool("true")
var value: any = 42
var restored = int(value)
```

Conversions use the corresponding .NET conversion behavior. A conversion can
therefore fail at runtime, for example when converting `"not a number"` to
`int`. Balu does not currently provide exception handling.

A one-argument call whose name is a known type is treated as a conversion
before normal function lookup. This behavior may be refined in a future
language design.

There are no implicit conversions to `bool`; conditions must already have type
`bool`.

## Declarations and names

### Variables

Every variable declaration has an initializer:

```balu
var mutable = 1
let readOnly = 2
var explicitType: int = 3
```

`var` variables can be assigned after declaration. `let` variables cannot.

Parameters are mutable and can be assigned inside their function.

The initializer is bound before the new variable is declared. If an outer
variable has the same name, the initializer can therefore refer to that outer
variable:

```balu
var value = 1
{
    var value = value + 1
    println(value)
}
```

### Name spaces

Variables, parameters, and functions share a symbol namespace within each
scope. Balu does not support function overloads. Two symbols with the same name
cannot be declared in the same scope.

A declaration may hide a symbol in an outer scope. This is allowed but produces
a warning. Built-in functions can also be hidden.

Type lookup is separate from ordinary symbol lookup.

### Scopes

An explicit `{ ... }` block creates a lexical scope. A `for` statement creates
a scope containing its loop variable. A function has scopes for its parameters
and body.

In the current implementation, an unbraced body does not create an additional
lexical scope. Consequently, a variable declared as the single body of an
`if` or loop can enter the surrounding scope. This is an implementation
behavior under review rather than a recommended coding style; use braces when
scope boundaries matter.

## Expressions

### Primary expressions

Primary expressions are:

- integer, Boolean, and string literals
- variable and parameter names
- parenthesized expressions
- function calls
- prefix increment and decrement of a direct variable name
- postfix increment and decrement of a direct variable name

Calls have the form:

```balu
name(argument1, argument2)
```

Only a direct function name can be called. There are no member calls, callable
values, or call chains.

### Unary operators

| Operator | Operand | Result | Meaning |
| --- | --- | --- | --- |
| `+` | `int` | `int` | Identity |
| `-` | `int` | `int` | Arithmetic negation |
| `~` | `int` | `int` | Bitwise complement |
| `!` | `bool` | `bool` | Logical negation |

### Binary operators

Arithmetic and string operators:

| Operator | Operands | Result |
| --- | --- | --- |
| `+` | `int`, `int` | `int` |
| `-` | `int`, `int` | `int` |
| `*` | `int`, `int` | `int` |
| `/` | `int`, `int` | `int` |
| `+` | `string`, `string` | `string` |

Boolean and bitwise operators:

| Operator | Operands | Result | Short-circuiting |
| --- | --- | --- | --- |
| `&` | `int`, `int` | `int` | No |
| `|` | `int`, `int` | `int` | No |
| `^` | `int`, `int` | `int` | No |
| `&` | `bool`, `bool` | `bool` | No |
| `|` | `bool`, `bool` | `bool` | No |
| `^` | `bool`, `bool` | `bool` | No |
| `&&` | `bool`, `bool` | `bool` | Yes |
| `||` | `bool`, `bool` | `bool` | Yes |

Equality and relational operators:

| Operator | Operands | Result |
| --- | --- | --- |
| `==`, `!=` | two `int` values | `bool` |
| `==`, `!=` | two `bool` values | `bool` |
| `==`, `!=` | two `string` values | `bool` |
| `==`, `!=` | two `any` values | `bool` |
| `<`, `<=`, `>`, `>=` | two `int` values | `bool` |

Both operands must have the same supported static type. String equality is
value equality. `any` equality uses .NET object equality.

### Precedence and associativity

The precedence levels, from highest to lowest, are:

| Precedence | Operators | Associativity |
| ---: | --- | --- |
| primary | calls, parentheses, `++x`, `--x`, `x++`, `x--` | n/a |
| 100 | unary `+`, `-`, `!`, `~` | right |
| 11 | `*`, `/` | left |
| 10 | `+`, `-` | left |
| 5 | `==`, `!=`, `<`, `<=`, `>`, `>=` | left |
| 2 | `&`, `&&` | left |
| 1 | `|`, `||`, `^` | left |
| assignment | `=`, `+=`, `-=`, `*=`, `/=`, `&=`, `|=`, `^=` | right |

Several operators intentionally share a precedence level that differs from
common C-family precedence tables. Use parentheses when mixing equality and
relational operators or different Boolean operator forms.

### Assignment

The left side of an assignment must be a direct mutable variable or parameter
name. Assignment is an expression and evaluates to the assigned value:

```balu
var a = 0
var b = 0
a = b = 5
```

The compound assignment operators are:

```text
+=  -=  *=  /=  &=  |=  ^=
```

Supported combinations are:

| Type | Compound assignments |
| --- | --- |
| `int` | `+=`, `-=`, `*=`, `/=`, `&=`, `|=`, `^=` |
| `string` | `+=` |
| `bool` | `&=`, `|=`, `^=` |

In the current implementation, Boolean `&=` and `|=` are internally bound as
logical operations. Their exact relationship to the non-short-circuiting `&`
and `|` operators should not yet be treated as a stable language guarantee.

### Increment and decrement

Prefix and postfix increment and decrement apply only to direct mutable `int`
variables or parameters:

```balu
var value = 1
var old = value++ // old is 1, value is 2
var next = ++value // value and next are 3
```

Prefix form evaluates to the new value. Postfix form evaluates to the old
value. The operators cannot be applied to a `let`, call, or parenthesized
expression.

## Statements

### Blocks

A block contains zero or more statements and introduces a lexical scope:

```balu
{
    var value = 1
    println(value)
}
```

### Expression statements

In a normal program or function, only assignments, calls, increments, and
decrements can be used as statements:

```balu
value = 10
println(value)
value++
```

A value expression such as `1 + 2` is permitted as a top-level script
submission, where it becomes the script result.

### `if`

```balu
if condition
    statement
else
    statement
```

The condition must be `bool`. An `else` binds to the nearest preceding `if`
that does not already have an `else`. `else if` works by placing another `if`
statement after `else`.

### `while`

```balu
while condition {
    statements
}
```

The `bool` condition is checked before every iteration. `continue` returns to
the condition check.

### `do while`

```balu
do {
    statements
} while condition
```

The body executes before the `bool` condition is checked and therefore runs at
least once. There is no trailing semicolon.

### `for ... to ...`

```balu
for i = lowerBound to upperBound {
    statements
}
```

Both bounds must have type `int`. They are each evaluated once before the loop.
The loop variable begins at the lower bound, increases by one, and continues
while it is less than or equal to the stored upper bound.

The loop variable is mutable and scoped to the `for` statement. `continue`
performs the increment before checking the next iteration.

There is no descending form or configurable step.

The increment currently uses unchecked 32-bit arithmetic. If the inclusive
upper bound is `2147483647`, incrementing after that iteration wraps the loop
variable to `-2147483648`, so the loop does not terminate normally. This is a
known implementation limitation.

### `break` and `continue`

`break` exits the innermost enclosing loop. `continue` begins the next
iteration of the innermost enclosing loop. Both are invalid outside a loop.

Known compiler issue: the binder currently records `BL1020` and then attempts
to access a missing loop context, which can terminate compilation instead of
returning the diagnostic normally.

### `return`

```balu
return
return expression
```

A function without a return type accepts only `return` without a value. A
function with a return type requires a compatible return value, and all
reachable paths must return.

At the top level of a normal program, `return` without a value exits the
implicit entry point. A top-level return value is allowed only in script mode.

The expression must begin on the same physical source line as `return`:

```balu
return value // returns value
```

```balu
return
value        // a separate statement
```

This is the only generally line-sensitive statement rule in the current
grammar.

## Functions

A function declaration has a name, zero or more typed parameters, an optional
return type, and a block body:

```balu
function name(first: int, second: string): bool {
    return second == string(first)
}
```

Functions are declared globally. Their names are available before bodies are
bound, so functions can call later declarations, call themselves recursively,
and participate in mutual recursion.

Arguments are evaluated from left to right. Their count must exactly match the
parameter count, and each argument must be implicitly convertible to its
parameter type.

Parameters are passed by value and are mutable within the function.

Balu does not support overloads, default arguments, named arguments, variadic
parameters, local functions, or function values.

## Source files and programs

### Shared global namespace

All syntax trees passed to one compilation share a global namespace. There are
no source-level modules, imports, namespaces, exports, or file-local symbols.

Functions can refer to functions and global variables in other files.

Global variable declarations are bound in syntax-tree and source order. A
global initializer can refer to functions and to global variables already
declared, but not to a global variable declared later. The order in which
source files are supplied can therefore affect global initializers. This is a
current implementation characteristic that may be constrained by a future
language specification.

### Entry point

A normal compilation needs exactly one effective entry point.

An explicit entry point is a function named `main` with no parameters and no
return type:

```balu
function main() {
    println("Hello")
}
```

If there is no explicit `main`, global statements create an implicit entry
point:

```balu
println("Hello")
```

The following rules apply:

- an explicit `main` cannot be mixed with global statements
- global statements may occur in at most one source file
- a compilation with neither form reports that no entry point is defined
- command-line arguments are not passed to `main`
- `main` cannot return an exit code

The module name accepted by `bc` is the emitted .NET assembly name. It is not a
Balu module declaration.

## Script mode

Script mode is used by `Compilation.CreateScript` and the REPL. It creates a
synthetic entry point that returns `any`.

Compared with a normal program:

- a top-level value expression is allowed and becomes the submission result
- a top-level `return expression` is allowed
- visible symbols from a successful previous submission form the parent scope
  of `Compilation.CreateScript`
- `Balu.Interpretation.Interpreter` copies global variable values into the next
  emitted submission
- failed compilation or execution does not commit the interpreter's compilation
  or global-variable state

Redeclaring a name in a later submission hides the previous symbol. Functions
that were bound in an earlier submission retain their references to the older
symbols.

The REPL persists successful source submissions and executes them again on the
next process start. This persistence behavior belongs to `bi`, not to the core
language.

State rollback cannot undo external effects that occurred before a runtime
failure. Console output remains visible and input already read remains
consumed.

## Runtime behavior

Balu currently emits managed .NET IL. Observable runtime behavior includes:

- integer addition, subtraction, multiplication, negation, and increment use
  normal unchecked 32-bit IL operations
- integer division truncates toward zero
- nonconstant division by zero fails at runtime
- dividing `-2147483648` by `-1` overflows and fails at runtime
- `&&` and `||` evaluate the right operand only when required
- ordinary binary operands and call arguments are evaluated left to right
- string concatenation uses .NET string concatenation
- string and `any` equality use .NET equality behavior
- explicit conversions use .NET conversion methods and can fail at runtime

These statements describe the current backend. A future normative language
specification may define equivalent behavior independently of .NET.

Constant folding currently attempts to evaluate a constant expression such as
`1 / 0` while binding. The resulting exception is not converted into a normal
diagnostic. This is a known compiler issue rather than defined language
behavior.

## Unsupported constructs

The grammar has no syntax for:

- arrays, indexing, or collection literals
- user-defined types or members
- modules or imports
- floating-point or character literals
- `null`
- exceptions
- lambdas or closures
- function overloads or generics
- member access
- `switch`, `foreach`, or a ternary expression
- string interpolation or raw strings
