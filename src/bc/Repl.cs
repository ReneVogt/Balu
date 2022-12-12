using System.Text;
using System;

namespace Balu;

abstract class Repl
{
    protected StringBuilder TextBuilder { get; } = new();


    protected abstract bool IsCompleteSubmission(string text);
    protected virtual void EvaluateMetaCommand(string text)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Unknown command '{text}'.");
        Console.ResetColor();
    }
    protected abstract void EvaluateSubmission(string text);

    public void Run()
    {
        while (true)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(TextBuilder.Length == 0 ? "» " : "· ");
                Console.ResetColor();
                var line = Console.ReadLine();
                if (TextBuilder.Length == 0)
                {
                    if (string.IsNullOrWhiteSpace(line)) return;
                    if (line.StartsWith('#'))
                    {
                        EvaluateMetaCommand(line);
                        continue;
                    }
                }

                TextBuilder.AppendLine(line);
                var text = TextBuilder.ToString();
                if (!IsCompleteSubmission(text)) continue;

                EvaluateSubmission(text);
                TextBuilder.Clear();
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exception);
                Console.ResetColor();
                TextBuilder.Clear();
            }
        }

    }
}
