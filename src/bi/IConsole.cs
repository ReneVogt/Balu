using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Balu.Interactive;

interface IConsole
{
    int BufferWidth { get; }
    int BufferHeight { get; }
    int WindowWidth { get; }
    int WindowHeight { get; }
    int CursorLeft { get; }
    int CursorTop { get; }
    bool CursorVisible { get; set; }
    ConsoleColor ForegroundColor { get; set; }
    ConsoleColor BackgroundColor { get; set; }
    TextWriter Out { get; }

    bool TrySetCursorPosition(int left, int top);
    void WriteLine();
}

sealed class SystemConsole : IConsole
{
    public static SystemConsole Instance { get; } = new();

    SystemConsole() { }

    public int BufferWidth => Console.BufferWidth;
    public int BufferHeight => Console.BufferHeight;
    public int WindowWidth => Console.WindowWidth;
    public int WindowHeight => Console.WindowHeight;
    public int CursorLeft => Console.CursorLeft;
    public int CursorTop => Console.CursorTop;
    [SuppressMessage("Interoperability", "CA1416", Justification = "The REPL requires an interactive console that supports cursor positioning.")]
    public bool CursorVisible { get => Console.CursorVisible; set => Console.CursorVisible = value; }
    public ConsoleColor ForegroundColor { get => Console.ForegroundColor; set => Console.ForegroundColor = value; }
    public ConsoleColor BackgroundColor { get => Console.BackgroundColor; set => Console.BackgroundColor = value; }
    public TextWriter Out => Console.Out;

    public bool TrySetCursorPosition(int left, int top)
    {
        try
        {
            Console.SetCursorPosition(left, top);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }
    public void WriteLine() => Console.WriteLine();
}
