using System;
using Balu;

while (true)
{
    Console.Write("> ");
    var line = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(line)) return;
    var lexer = new Lexer(line);
    var oldColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine(string.Join(Environment.NewLine, lexer.GetTokens()));
    Console.ForegroundColor = oldColor;
    Console.WriteLine();
}