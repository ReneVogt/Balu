# Built-in functions

Balu programs can call four built-in functions without declaring or importing
them:

| Function | Return type | Purpose |
| --- | --- | --- |
| `print(value: any)` | no value | Write a value without a line break |
| `println(value: any)` | no value | Write a value followed by a line break |
| `input()` | `string` | Read one line from standard input |
| `random(maximum: int)` | `int` | Generate a pseudo-random integer below `maximum` |

The built-ins are part of the compiler runtime mapping, not functions written
in Balu source code.

## `print`

```text
print(value: any)
```

`print` writes `value` to standard output without appending a line break. It is
implemented using `.NET`'s `Console.Write(object)`.

```balu
print("Hello, ")
print("world")
```

Output:

```text
Hello, world
```

`int`, `bool`, and `string` values are implicitly convertible to `any`, so no
explicit conversion is needed when printing them:

```balu
print(42)
print(true)
print("text")
```

## `println`

```text
println(value: any)
```

`println` writes `value` to standard output and appends the platform's line
terminator. It is implemented using `.NET`'s `Console.WriteLine(object)`.

```balu
println("first line")
println("second line")
```

Output:

```text
first line
second line
```

There is no zero-argument overload. To write an empty line, pass an empty
string:

```balu
println("")
```

## `input`

```text
input(): string
```

`input` reads one line from standard input. The returned string does not
contain the line terminator.

```balu
print("Name: ")
var name = input()
println("Hello, " + name)
```

At the end of the input stream, the underlying `.NET` read returns `null`. The
compiler converts that result to a string, producing an empty string.

In the REPL, Balu program input and the REPL editor use the same console. Call
`input()` only when the submitted program is expected to pause and read another
line.

## `random`

```text
random(maximum: int): int
```

For a positive `maximum`, `random` returns a pseudo-random integer in the range
from zero, inclusive, to `maximum`, exclusive:

```balu
var die = random(6) + 1
println(die)
```

The generated program owns one `.NET` `Random` instance and calls
`Random.Next(maximum)` for each invocation.

The current runtime behavior follows `.NET`:

- a positive maximum produces `0 <= result < maximum`
- a maximum of zero produces zero
- a negative maximum throws a runtime exception

Balu does not currently provide a way to set the random seed.

## Name lookup and shadowing

Built-in functions live in the outer global scope. A user declaration can hide
a built-in, but the compiler reports a warning:

```balu
function print(value: int) {
}
```

After that declaration, calls in its scope resolve according to the normal
symbol lookup rules. Avoid reusing built-in names unless shadowing is
intentional.

Built-ins cannot be overloaded. For example, `println()` is an argument-count
error rather than a call to a separate zero-argument overload.
