# Diagnostics

Balu reports lexical, syntactic, semantic, control-flow, and emission problems
as diagnostics. Each diagnostic has an ID, severity, message, and, where
possible, a source location.

## Diagnostic IDs

IDs use the format `BLdddd` and are grouped by compiler phase:

| Range | Area |
| --- | --- |
| `BL0001`-`BL0999` | Lexer and parser |
| `BL1001`-`BL1999` | Binding and lowering |
| `BL2001`-`BL2999` | Assembly and debug-symbol emission |

The numeric ID is the useful external identifier. The corresponding .NET enum
member names are implementation details and may be corrected or renamed as the
project evolves.

## Severity

Most diagnostics are errors. An error prevents assembly emission or execution
of a REPL submission.

The current warnings are:

| ID | Meaning |
| --- | --- |
| `BL1009` | A declaration hides a symbol from an outer scope |
| `BL1030` | Code is unreachable |

Warnings do not prevent emission or execution. `bc` exits successfully when a
compilation contains warnings but no errors. There is currently no
warnings-as-errors option.

## Display format

A source diagnostic is displayed with a one-based line and column:

```text
program.b(3,5): error BL1005: Undefined name 'value'.
    value
```

Warnings use `warning` instead of `error`. For a diagnostic with a non-empty
source span, the compiler also prints the affected source text and highlights
it when writing to an interactive console. Colors are disabled when output is
redirected.

Diagnostics without a source location use this format:

```text
[BL2002]: The required type 'System.Object' could not be resolved among the referenced assemblies.
```

For example, missing runtime types and path collisions are not necessarily tied
to one source span.

## Lexer and parser diagnostics

| ID | Condition |
| --- | --- |
| `BL0001` | An unexpected character or token was found, or an expected token is missing |
| `BL0002` | A number is not a valid signed 32-bit integer |
| `BL0003` | A string contains an unsupported escape sequence |
| `BL0004` | A string literal is not terminated |
| `BL0005` | A multi-line comment is not terminated |

The parser attempts to recover after errors by creating missing tokens and
preserving skipped source text. One malformed construct can therefore produce
more than one diagnostic.

## Binding and lowering diagnostics

| ID | Severity | Condition |
| --- | --- | --- |
| `BL1001` | Error | A unary operator does not support its operand type |
| `BL1002` | Error | A binary operator does not support its operand types |
| `BL1003` | Error | Prefix increment or decrement does not support the variable type |
| `BL1004` | Error | Postfix increment or decrement does not support the variable type |
| `BL1005` | Error | A name is not defined in the current scope |
| `BL1006` | Error | No conversion exists between two types |
| `BL1007` | Error | A conversion exists but must be written explicitly |
| `BL1008` | Error | A symbol is already declared in the same scope |
| `BL1009` | Warning | A declaration hides a symbol in an outer scope |
| `BL1010` | Error | An assignment targets a read-only `let` variable |
| `BL1011` | Error | A function has the wrong number of arguments, or an argument has the wrong type |
| `BL1012` | Error | An expression that must produce a value has no value |
| `BL1013` | Error | A symbol used as a variable has another symbol kind |
| `BL1014` | Error | A symbol called as a function has another symbol kind |
| `BL1015` | Error | A type name is not defined |
| `BL1016` | Error | A required variable is not defined |
| `BL1017` | Error | A required function is not defined |
| `BL1018` | Error | A parameter name is duplicated in a function declaration |
| `BL1019` | Error | A function with the same name is already declared |
| `BL1020` | Error | `break` or `continue` is used outside a loop |
| `BL1021` | Error | A value-bearing `return` is used in global statements of a normal program |
| `BL1022` | Error | A value-returning function uses `return` without a value |
| `BL1023` | Error | A returned value has the wrong type, or a value is returned from a no-value function |
| `BL1024` | Error | Not all reachable paths of a value-returning function return a value |
| `BL1025` | Error | An unsupported expression is used as a statement |
| `BL1026` | Error | Global statements are mixed with an explicit `main` function |
| `BL1027` | Error | `main` has parameters or a return type |
| `BL1028` | Error | Without an explicit `main`, global statements occur in more than one source file |
| `BL1029` | Error | A normal program has neither `main` nor global statements |
| `BL1030` | Warning | Control-flow analysis found unreachable code |

`BL1011` currently covers both argument-count and argument-type mismatches. A
future compiler version may assign a separate ID to argument-type errors.

The binder intends to report `BL1020` for `break` or `continue` outside a loop.
It currently records the diagnostic and then can terminate unexpectedly while
accessing the missing loop context. This is a known compiler issue.

## Emitter diagnostics

| ID | Condition |
| --- | --- |
| `BL2001` | A reference has an invalid assembly image or encounters a handled I/O load failure |
| `BL2002` | A required .NET runtime type cannot be found in the references |
| `BL2003` | A required .NET runtime type is ambiguous across references |
| `BL2004` | A required .NET runtime method cannot be found |
| `BL2005` | A source document has no name and cannot be represented in debug symbols |
| `BL2006` | One document name identifies different source texts in debug symbols |
| `BL2007` | An assembly, PDB, or source path conflicts with another emit path |

Emitter diagnostics are produced only when emitting an assembly or PDB. They do
not appear merely by parsing or binding a `Compilation`.

## Tools and exit behavior

The command-line compiler writes diagnostics to standard error. Its exit code
is:

| Exit code | Meaning |
| ---: | --- |
| `0` | Help was displayed, or compilation succeeded, possibly with warnings |
| `1` | Compilation completed with error diagnostics |
| `2` | The command-line invocation is invalid |
| `3` | The compiler tool could not complete |

The `bc` quiet option suppresses diagnostics as well as informational output;
the exit code is then the only error indication. See the
[compiler command-line reference](../tools/compiler.md) for details.

Invocation and tool failures are reported without a `BLxxxx` diagnostic ID or
source location. This lets callers distinguish them from source and emitter
diagnostics.

The REPL displays diagnostics and does not commit a submission that has errors.
Warnings are displayed but do not prevent execution or persistence of the
submission.
