using System.Text;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Balu;

abstract class Repl
{
    sealed class Document : ObservableCollection<string>
    {
        public Document() : base(new []{string.Empty}){}
    }
    sealed class SubmissionView
    {
        readonly int cursorTop;
        int renderedLinesCount, cursorX, cursorY;

        public Document SubmissionDocument { get; }
        public int CursorX
        {
            get => cursorX;
            set
            {
                if (value == cursorX) return;
                cursorX = value;
                UpdateCursorPosition();
            }
        }
        public int CursorY
        {
            get => cursorY;
            set
            {
                if (value == cursorY) return;
                cursorY = value;
                UpdateCursorPosition();
            }
        }

        public SubmissionView(Document submissionDocument)
        {
            SubmissionDocument = submissionDocument;
            SubmissionDocument.CollectionChanged += OnSubmissionDocumentChanged;
            cursorTop = Console.CursorTop;
            Render();
        }
        void OnSubmissionDocumentChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Render();
        }

        void Render()
        {
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, cursorTop);
            for (int i = 0; i < SubmissionDocument.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(i == 0 ? "» " : "· ");
                Console.ResetColor();
                Console.WriteLine(SubmissionDocument[i].PadRight(Console.WindowWidth-2));
            }

            int blankLines = renderedLinesCount - SubmissionDocument.Count;
            while(blankLines-- > 0) Console.WriteLine(new string(' ', Console.WindowWidth));
            renderedLinesCount = SubmissionDocument.Count;
            Console.CursorVisible = true;
            UpdateCursorPosition();
        }

        void UpdateCursorPosition()
        {
            Console.CursorTop = cursorTop + CursorY;
            Console.CursorLeft = 2 + CursorX;
        }
    }

    bool editDone;

    protected abstract bool IsCompleteSubmission(string text);
    protected virtual void EvaluateMetaCommand(string text)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Unknown command '{text}'.");
        Console.ResetColor();
    }
    protected abstract void EvaluateSubmission(string text);

    string? EditSubmission()
    {
        editDone = false;
        var view = new SubmissionView(new ());

        while (!editDone)
            HandleKey(Console.ReadKey(true), view);
        Console.WriteLine();
        return string.Join(Environment.NewLine, view.SubmissionDocument);
    }
    void HandleKey(ConsoleKeyInfo keyInfo, SubmissionView view)
    {
        if (keyInfo.Modifiers == default)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    HandleEnter(view);
                    break;
                case ConsoleKey.LeftArrow:
                    HandleLeftArrow(view);
                    break;
                case ConsoleKey.RightArrow:
                    HandleRightArrow(view);
                    break;
                case ConsoleKey.UpArrow:
                    HandleUpArrow(view);
                    break;
                case ConsoleKey.DownArrow:
                    HandleDownArrow(view);
                    break;
                case ConsoleKey.Escape:
                    HandleEscape(view);
                    break;
            }
        }

        if (keyInfo.Modifiers == ConsoleModifiers.Control)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    editDone = true;
                    break;
            }
        }

        if (keyInfo.KeyChar >= ' ')
            HandleTyping(view, keyInfo.KeyChar.ToString());
    }

    void HandleEnter(SubmissionView view)
    {
        if (CheckSubmissionCompleted(view)) return;
        var line = view.SubmissionDocument[view.CursorY];
        var nextLine = line[view.CursorX..];
        var oldLine = line[..view.CursorX];
        view.SubmissionDocument[view.CursorY] = oldLine;
        view.SubmissionDocument.Insert(view.CursorY+1, nextLine);
        view.CursorX = 0;
        view.CursorY++;
    }

    static void HandleLeftArrow(SubmissionView view)
    {
        if (view.CursorX > 0) view.CursorX--;
    }

    static void HandleRightArrow(SubmissionView view)
    {
        if (view.CursorX < view.SubmissionDocument[view.CursorY].Length - 1)
            view.CursorX++;
    }

    static void HandleUpArrow(SubmissionView view)
    {
        if (view.CursorY > 0)
            view.CursorY--;
    }

    static void HandleDownArrow(SubmissionView view)
    {
        if (view.CursorY < view.SubmissionDocument.Count - 1)
            view.CursorY++;
    }

    static void HandleEscape(SubmissionView view) { }

    static void HandleTyping(SubmissionView view, string input)
    {
        view.SubmissionDocument[view.CursorY] = view.SubmissionDocument[view.CursorY].Insert(view.CursorX, input);
        view.CursorX += input.Length;
    }
    bool CheckSubmissionCompleted(SubmissionView view)
    {
        var text = string.Join(Environment.NewLine, view.SubmissionDocument);
        if (text.StartsWith('#') || IsCompleteSubmission(text))
            editDone = true;
        return editDone;
    }

    public void Run()
    {
        while (true)
        {
            try
            {
                var text = EditSubmission();
                if (string.IsNullOrWhiteSpace(text)) return;
                if (text.StartsWith('#'))
                    EvaluateMetaCommand(text);
                else
                    EvaluateSubmission(text);
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exception);
                Console.ResetColor();
            }
        }

    }
}
