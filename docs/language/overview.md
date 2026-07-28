# Language overview

Balu is a small, statically checked, imperative language. Source code is parsed,
bound to typed symbols, lowered to simple control flow, and emitted as managed
.NET IL.

This page is a guided tour. See the [language reference](reference.md) for the
complete syntax and semantic rules.

## A complete program

```balu
function greet(name: string) {
    println("Hello, " + name)
}

function main() {
    print("What is your name? ")
    var name = input()
    greet(name)
}
```

Functions start with `function`. Parameters always have a type. A function
without a written return type returns no value.

`main` is the entry point of this program. It must have no parameters and no
return value.

## Values and types

Balu has four types that can be used in source code:

| Type | Description | Example |
| --- | --- | --- |
| `int` | 32-bit signed integer | `42` |
| `bool` | Boolean value | `true` |
| `string` | Text | `"hello"` |
| `any` | A value stored without its more specific static type | `any(42)` |

Functions without a return type use an internal `void` type. `void` currently
cannot be written in a type clause.

Types are checked at compile time. Balu does not use truthiness, so an `if` or
loop condition must be a `bool`:

```balu
var ready = true

if ready
    println("Ready")
```

## Variables and constants

`var` declares a mutable variable. `let` declares a read-only variable:

```balu
var counter = 0
counter = counter + 1

let maximum = 10
```

The type is inferred from the initializer unless it is written explicitly:

```balu
var counter: int = 0
let title: string = "Balu"
```

Every variable declaration requires an initializer.

Declarations at the top level create global variables. Declarations in a
function or block create local variables.

## Expressions

Balu provides arithmetic, comparison, equality, Boolean, and bitwise
operators:

```balu
var arithmetic = 2 + 3 * 4
var comparison = arithmetic >= 10
var logic = comparison && true
var bits = 6 & 3
var text = "Hello, " + "world"
```

Strings only concatenate with strings. Convert another value explicitly when
needed:

```balu
var message = "The answer is " + string(42)
```

Assignments are expressions and produce a value:

```balu
var x = 1
var y = (x = 5)
```

Mutable integer variables also support prefix and postfix increment and
decrement:

```balu
++x
y--
```

## Control flow

Conditions do not require parentheses. The controlled body can be one statement
or a block.

### `if` and `else`

```balu
var score = 75

if score >= 50 {
    println("passed")
} else {
    println("failed")
}
```

### `while`

```balu
var i = 0

while i < 3 {
    println(i)
    i++
}
```

### `do while`

```balu
var i = 3

do {
    println(i)
    i--
} while i > 0
```

The body of a `do while` loop runs at least once.

### `for ... to ...`

```balu
for i = 1 to 5 {
    println(i)
}
```

The lower and upper bounds are evaluated once. The loop counts upward by one,
and the upper bound is inclusive. The current unchecked increment has an edge
case at `2147483647`; see the detailed reference before using `int` limits as
loop bounds.

Use `break` to leave the innermost loop and `continue` to begin its next
iteration.

## Functions

Function parameters require type annotations. A return type is required only
when the function returns a value:

```balu
function add(left: int, right: int): int {
    return left + right
}

function show(value: int) {
    println(value)
}
```

Functions are global. They can call functions declared later in the source,
and recursion is supported:

```balu
function fibonacci(n: int): int {
    if n <= 1
        return n

    return fibonacci(n - 1) + fibonacci(n - 2)
}
```

Balu does not currently support overloads, local functions, optional
parameters, or function values.

## Program entry points

A normal Balu program has one of two forms.

An explicit `main` function:

```balu
function main() {
    println("Hello")
}
```

Or top-level global statements, which produce an implicit entry point:

```balu
println("Hello")
```

An explicit `main` cannot be mixed with global statements. In a multi-file
program, global statements may occur in at most one file.

## Multiple source files

All source files in a compilation share one global namespace. There are no
module or import declarations:

```balu
// math.b
function double(value: int): int {
    return value * 2
}
```

```balu
// program.b
function main() {
    println(double(21))
}
```

Functions and global variables are visible across file boundaries. The order
of global variable initializers can matter; see the
[multi-file rules](reference.md#source-files-and-programs) in the reference.

## REPL and script mode

The REPL compiles each input as a script submission. Successful submissions
can see variables and functions from earlier submissions:

```text
» var value = 10
» value * 2
Result: 20
```

Unlike a normal program, a top-level value expression is valid in script mode
and becomes the submission result. Compile-time and runtime failures do not
commit the interpreter's symbols or global values. I/O that already happened
before a runtime failure cannot be rolled back.

## Built-in functions

Balu includes four built-in functions:

```text
print(value: any)
println(value: any)
input(): string
random(maximum: int): int
```

See [Built-in functions](built-in-functions.md) for their behavior and examples.

## Current limits

Balu does not currently provide:

- arrays or collections
- user-defined types
- modules, imports, or namespaces
- floating-point values
- `null`
- exceptions or exception handling
- lambdas, closures, or local functions
- function overloads or generics
- member access or .NET interop syntax
- `switch`, `foreach`, or configurable `for` steps
