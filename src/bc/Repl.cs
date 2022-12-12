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
        sealed class UpdateDisposable : IDisposable
        {
            readonly SubmissionView parent;
            public UpdateDisposable(SubmissionView parent)
            {
                this.parent = parent;
                Console.CursorVisible = false;
                parent.updatesInProgress++;
            }
            public void Dispose()
            {
                parent.updatesInProgress--;
                if (parent.updatesInProgress != 0) return;
                parent.Render();
            }
        }
        readonly int cursorTop;
        int renderedLinesCount, cursorX, cursorY, updatesInProgress;

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
                if (cursorX > SubmissionDocument[cursorY].Length) cursorX = SubmissionDocument[cursorY].Length;
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
            if (updatesInProgress > 0) return;
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
            if (updatesInProgress > 0) return;
            Console.CursorTop = cursorTop + CursorY;
            Console.CursorLeft = 2 + CursorX;
        }
        public IDisposable CreateUpdateContext() => new UpdateDisposable(this);
    }

    bool editDone;

    protected abstract bool IsCompleteSubmission(string text);
    protected virtual void EvaluateMetaCommand(string text)
    {
        if (text == "#exit") Environment.Exit(0);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Unknown command '{text}'.");
        Console.ResetColor();
    }
    protected abstract void EvaluateSubmission(string text);

    string EditSubmission()
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
        switch((keyInfo.Modifiers, keyInfo.Key, keyInfo.KeyChar))
        {
            case (ConsoleModifiers.Control, ConsoleKey.Enter, _):
                editDone = true;
                break;
            case (0, ConsoleKey.Enter, _):
                HandleEnter(view);
                break;
            case (0, ConsoleKey.LeftArrow, _):
                HandleLeftArrow(view);
                break;
            case (0, ConsoleKey.RightArrow, _):
                HandleRightArrow(view);
                break;
            case (0, ConsoleKey.UpArrow, _):
                HandleUpArrow(view);
                break;
            case (0, ConsoleKey.DownArrow, _):
                HandleDownArrow(view);
                break;
            case (0, ConsoleKey.Backspace, _):
                HandleBackspace(view);
                break;
            case (0, ConsoleKey.Delete, _):
                HandleDelete(view);
                break;
            case (0, ConsoleKey.Home, _):
                view.CursorX = 0;
                break;
            case (0, ConsoleKey.End, _):
                view.CursorX = view.SubmissionDocument[view.CursorY].Length;
                break;
            case (0, ConsoleKey.Tab, _):
                HandleTab(view);
                break;
            case (_, _, >= ' '):
                HandleTyping (view, keyInfo.KeyChar.ToString());
                break;
        }
    }

    void HandleEnter(SubmissionView view)
    {
        if (CheckSubmissionCompleted(view)) return;
        using(view.CreateUpdateContext())
        {
            var line = view.SubmissionDocument[view.CursorY];
            var nextLine = line[view.CursorX..];
            var oldLine = line[..view.CursorX];
            view.SubmissionDocument[view.CursorY] = oldLine;
            view.SubmissionDocument.Insert(view.CursorY + 1, nextLine);
            view.CursorX = 0;
            view.CursorY++;
        }
    }
    static void HandleLeftArrow(SubmissionView view)
    {
        using(view.CreateUpdateContext())
        {
            if (view.CursorX > 0)
                view.CursorX--;
            else
            {
                if (view.CursorY == 0) return;
                view.CursorY--;
                view.CursorX = view.SubmissionDocument[view.CursorY].Length;
            }
        }
    }
    static void HandleRightArrow(SubmissionView view)
    {
        using(view.CreateUpdateContext())
        {
            if (view.CursorX < view.SubmissionDocument[view.CursorY].Length)
                view.CursorX++;
            else
            {
                if (view.CursorY == view.SubmissionDocument.Count - 1) return;
                view.CursorY++;
                view.CursorX = 0;
            }
        }
    }
    static void HandleUpArrow(SubmissionView view)
    {
        using(view.CreateUpdateContext())
        {
            if (view.CursorY > 0)
                view.CursorY--;
        }
    }
    static void HandleDownArrow(SubmissionView view)
    {
        using(view.CreateUpdateContext())
        {
            if (view.CursorY < view.SubmissionDocument.Count - 1)
                view.CursorY++;
        }
    }

    static void HandleBackspace(SubmissionView view)
    {
        using(view.CreateUpdateContext())
        {
            if (view.CursorX == 0)
            {
                if (view.CursorY == 0) return;
                view.CursorY--;
                view.CursorX = view.SubmissionDocument[view.CursorY].Length;
                view.SubmissionDocument[view.CursorY] += view.SubmissionDocument[view.CursorY + 1];
                view.SubmissionDocument.RemoveAt(view.CursorY + 1);
                return;
            }

            view.SubmissionDocument[view.CursorY] = view.SubmissionDocument[view.CursorY].Remove(view.CursorX - 1, 1);
            view.CursorX--;
        }
    }
    static void HandleDelete(SubmissionView view)
    {
        using (view.CreateUpdateContext())
        {
            if (view.CursorX < view.SubmissionDocument[view.CursorY].Length)
                view.SubmissionDocument[view.CursorY] = view.SubmissionDocument[view.CursorY].Remove(view.CursorX, 1);
            else
            {
                if (view.CursorY == view.SubmissionDocument.Count - 1) return;
                view.SubmissionDocument[view.CursorY] += view.SubmissionDocument[view.CursorY + 1];
                view.SubmissionDocument.RemoveAt(view.CursorY+1);
            }
        }
    }

    static void HandleTab(SubmissionView view)
    {
        using(view.CreateUpdateContext())
        {
            const int TabWidth = 4;
            var line = view.SubmissionDocument[view.CursorY];
            var remaining = TabWidth - view.CursorX % TabWidth;
            view.SubmissionDocument[view.CursorY] = line.Insert(view.CursorX, new string(' ', remaining));
            view.CursorX += remaining;
        }
    }

    static void HandleTyping(SubmissionView view, string input)
    {
        using(view.CreateUpdateContext())
        {
            view.SubmissionDocument[view.CursorY] = view.SubmissionDocument[view.CursorY].Insert(view.CursorX, input);
            view.CursorX += input.Length;
        }
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
        Console.ResetColor();
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
