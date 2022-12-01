using System;
using System.Linq;
using Balu;
using Balu.Binding;
using Balu.Syntax;

bool showSyntax = false, showBound = false;

while (true)
{
    try
    {
        Console.Write("> ");
        var line = Console.ReadLine();
        if (line == "#syntax")
        {
            showSyntax = !showSyntax;
            Console.WriteLine(showSyntax ? "Showing syntax tree." : "Not showing syntax tree.");
            continue;
        }
        if (line == "#bound")
        {
            showBound = !showBound;
            Console.WriteLine(showBound ? "Showing bound tree." : "Not showing boud tree.");
            continue;
        }

        if (line == "#cls")
        {
            Console.Clear();
            continue;
        }

        if (string.IsNullOrWhiteSpace(line) || line == "#exit") return;

        var parser = new Parser(line);
        var syntaxTree = parser.Parse();
        if (showSyntax)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            PrettyPrintSyntax(syntaxTree.Root);
            Console.ResetColor();
        }

        var boundTree = Binder.Bind(syntaxTree.Root);
        if (showBound)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            PrettyPrintBound(boundTree.Root);
            Console.ResetColor();
        }
        
        if (boundTree.Diagnostics.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(string.Join(Environment.NewLine, boundTree.Diagnostics));
            Console.ResetColor();
        }
        else
            Console.WriteLine(new Evaluator(syntaxTree.Root).Evaluate());

        Console.WriteLine();

        static void PrettyPrintSyntax(SyntaxNode node, string indent = "", bool last = true)
        {
            var marker = last ? "└──" : "├──";
            Console.Write(indent);
            Console.Write(marker);
            Console.WriteLine(node);

            indent += last ? "   " : "│  ";
            var children = node.Children.ToArray();
            for (int i = 0; i < children.Length - 1; i++)
                PrettyPrintSyntax(children[i], indent, false);
            if (children.Length > 0)
                PrettyPrintSyntax(children[^1], indent);
        }
        static void PrettyPrintBound(BoundExpression node, string indent = "", bool last = true)
        {
            var marker = last ? "└──" : "├──";
            Console.Write(indent);
            Console.Write(marker);
            Console.Write($"{node.Kind}({node.Type}) ");

            indent += last ? "   " : "│  ";
            switch (node)
            {
                case BoundLiteralExpression  literal:
                    Console.WriteLine(literal.Value);
                    break;
                case BoundUnaryExpression unary:
                    Console.WriteLine(unary.OperatorKind);
                    PrettyPrintBound(unary.Operand, indent);
                    break;
                case BoundBinaryExpression binary:
                    Console.WriteLine(binary.OperatorKind);
                    PrettyPrintBound(binary.Left, indent);
                    PrettyPrintBound(binary.Right, indent);
                    break;
            }
        }
    }
    catch (Exception exception)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(exception);
        Console.ResetColor();
    }
}