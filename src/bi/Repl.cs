using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using Balu.Visualization;

#pragma warning disable CA1303

namespace Balu.Interactive;

abstract class Repl
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    protected sealed class MetaCommandAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }
        public MetaCommandAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
    sealed class MetaCommand
    {
        public string Name { get; }
        public string Description { get; }
        public MethodInfo Method { get; }

        public MetaCommand(string name, string description, MethodInfo method)
        {
            Name = name;
            Method = method;
            Description = description;
        }
    }
    sealed class Document : ObservableCollection<string>
    {
        public Document()
            : base(new[] { string.Empty }) { }
    }

    delegate object? LineRenderHandler(IReadOnlyList<string> lines, int lineIndex, object? state);

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

        readonly LineRenderHandler lineRenderer;
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

        public SubmissionView(Document submissionDocument, LineRenderHandler lineRenderer)
        {
            SubmissionDocument = submissionDocument;
            this.lineRenderer = lineRenderer;
            SubmissionDocument.CollectionChanged += OnSubmissionDocumentChanged;
            cursorTop = Console.CursorTop;
            Render();
        }
        void OnSubmissionDocumentChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Render();
        }

        void Render()
        {
            if (updatesInProgress > 0) return;
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, cursorTop);
            object? state = null;
            for (int i = 0; i < SubmissionDocument.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(i == 0 ? "» " : "· ");
                Console.ResetColor();
                state = lineRenderer(SubmissionDocument, i, state);
            }

            int blankLines = renderedLinesCount - SubmissionDocument.Count;
            while (blankLines-- > 0) Console.WriteLine(new string(' ', Console.WindowWidth));
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

    readonly ImmutableDictionary<string, MetaCommand> metaCommands;
    readonly List<string> history = new();

    int historyIndex;
    bool editDone;

    protected Repl()
    {
        metaCommands = (from method in GetType()
                            .GetMethods(
                                BindingFlags.NonPublic |
                                BindingFlags.Public |
                                BindingFlags.Static |
                                BindingFlags.Instance |
                                BindingFlags.FlattenHierarchy)
                        let attribute = method.GetCustomAttributes<MetaCommandAttribute>().SingleOrDefault()
                        where attribute is not null
                        select new MetaCommand(attribute.Name, attribute.Description, method)).ToImmutableDictionary(c => c.Name, c => c);
    }

    protected abstract bool IsCompleteSubmission(string text);
    void EvaluateMetaCommand(string text)
    {
        var position = text.IndexOf(' ', StringComparison.InvariantCulture);
        var commandName = position < 0 ? text[1..] : text[1 .. position];
        if (position < 0) position = text.Length;

        if (!metaCommands.TryGetValue(commandName, out var command))
        {
            Console.Error.WriteColoredText($"Error: unknown command '{commandName}'.{Environment.NewLine}", ConsoleColor.Red);
            return;
        }

        var arguments = new List<string>();
        var builder = new StringBuilder();
        var inQuotes = false;

        while (position < text.Length)
        {
            var current = text[position];
            var lookAhead = position + 1 < text.Length ? text[position + 1] : '\0';
            if (current == '"')
            {
                if (!inQuotes)
                {
                    CommitArgument();
                    inQuotes = true;
                    position++;
                    continue;
                }

                if (lookAhead == '"')
                {
                    builder.Append('"');
                    position += 2;
                    continue;
                }

                CommitArgument();
                inQuotes = false;
                position++;
                continue;
            }

            if (char.IsWhiteSpace(current) && !inQuotes)
            {
                CommitArgument();
                position++;
                continue;
            }

            builder.Append(current);
            position++;
        }

        CommitArgument();

        var args = arguments.Cast<object>().ToArray();
        var parameters = command.Method.GetParameters();
        if (args.Length != parameters.Length)
        {
            Console.Error.WriteColoredText($"Error: invalid number of arguments (found {args.Length}, but expected {parameters.Length}).{Environment.NewLine}", ConsoleColor.Red);
            Console.Error.Write("Usage: ");
            Console.Error.WritePunctuation("#");
            Console.Error.WriteIdentifier(commandName);
            Console.Error.WriteSpace();
            Console.Error.WritePunctuation(string.Join(" ", parameters.Select(parameter => $"<{parameter.Name}>")));
            Console.Error.WriteLine();
        }
        else command.Method.Invoke(this, args);
    
        void CommitArgument()
        {
            var arg = builder.ToString();
            if (!string.IsNullOrWhiteSpace(arg))
                arguments.Add(arg);
            builder.Clear();
        }
    }
    protected abstract void EvaluateSubmission(string text);

    [MetaCommand("exit", "Closes Balu interpreter.")]
    protected static void EvaluateExit() => Environment.Exit(0);
    [MetaCommand("help", "Shows this help.")]
    protected void EvaluateHelp()
    {
        foreach (var command in metaCommands.Values.OrderBy(mc => mc.Name))
        {
            Console.Out.WritePunctuation("#");
            Console.Out.WriteIdentifier(command.Name);
            Console.Out.WriteSpace();
            Console.Out.WritePunctuation(string.Join(" ", command.Method.GetParameters().Select(parameter => $"<{parameter.Name}>")));
            Console.Out.WriteLine();
            Console.Out.WritePunctuation(string.Join(Environment.NewLine, Chunks(command.Description)));
            Console.Out.WriteLine();
        }

        static IEnumerable<string> Chunks(string s)
        {
            int chunkSize = Console.WindowWidth - 3;
            int position = 0;
            while (position < s.Length)
            {
                int end = position + Math.Min(chunkSize, s.Length - position);
                yield return "  " + s[position..end];
                position += chunkSize;
            }
        }
    }

    protected virtual object? RenderLine(IReadOnlyList<string> lines, int lineIndex, object? state)
    {
        Console.WriteLine(lines[lineIndex]);
        return state;
    }

    protected void AddToHistory(string text)
    {
        history.Add(text);
        historyIndex = 0;
    }

    [MetaCommand("clearHistory", "Clears the input history.")]
    protected void ClearHistory() => history.Clear();

    string EditSubmission()
    {
        editDone = false;
        var view = new SubmissionView(new (), RenderLine);

        while (!editDone)
            HandleKey(Console.ReadKey(true), view);
        view.CursorY = view.SubmissionDocument.Count - 1;
        view.CursorX = view.SubmissionDocument[view.CursorY].Length;
        Console.WriteLine();

        return string.Join(Environment.NewLine, view.SubmissionDocument);
    }
    void HandleKey(ConsoleKeyInfo keyInfo, SubmissionView view)
    {
        switch(keyInfo.Modifiers, keyInfo.Key, keyInfo.KeyChar)
        {
            case (ConsoleModifiers.Control, ConsoleKey.Enter, _):
                HandleControlEnter(view);
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
            case (0, ConsoleKey.Escape, _):
                HandleEscape(view);
                break;
            case (0, ConsoleKey.PageUp, _):
                HandlePageUp(view);
                break;
            case (0, ConsoleKey.PageDown,_):
                HandlePageDown(view);
                break;
            case (_, _, >= ' '):
                HandleTyping (view, keyInfo.KeyChar.ToString());
                break;
        }
    }

    static void HandleControlEnter(SubmissionView view) => InsertLine(view);
    void HandleEnter(SubmissionView view)
    {
        if (CheckSubmissionCompleted(view)) return;
        InsertLine(view);
    }
    static void InsertLine(SubmissionView view)
    {
        using (view.CreateUpdateContext())
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
            view.SubmissionDocument[view.CursorY] = line.Insert(view.CursorX, value: new (' ', remaining));
            view.CursorX += remaining;
        }
    }
    static void HandleEscape(SubmissionView view)
    {
        using(view.CreateUpdateContext())
        {
            view.SubmissionDocument[view.CursorY] = string.Empty;
            view.CursorX = 0;
        }
    }
    void HandlePageUp(SubmissionView view)
    {
        if (history.Count == 0) return;
        historyIndex--;
        if (historyIndex < 0) historyIndex = history.Count - 1;
        UpdateDocumentFromHistory(view);
    }
    void HandlePageDown(SubmissionView view)
    {
        if (history.Count == 0) return;
        historyIndex++;
        if (historyIndex >= history.Count) historyIndex = 0;
        UpdateDocumentFromHistory(view);
    }
    void UpdateDocumentFromHistory(SubmissionView view)
    {
        if (history.Count == 0) return;
        using(view.CreateUpdateContext())
        {
            view.SubmissionDocument.Clear();
            foreach (var line in history[historyIndex].Split(Environment.NewLine))
                view.SubmissionDocument.Add(line);
            view.CursorY = view.SubmissionDocument.Count - 1;
            view.CursorX = view.SubmissionDocument[view.CursorY].Length;
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

    [SuppressMessage("Design", "CA1031", Justification = "This is the application's main method.")]
    public void Run()
    {
        Console.ResetColor();
        while (true)
        {
            try
            {
                var text = EditSubmission();
                if (string.IsNullOrWhiteSpace(text)) continue;
                AddToHistory(text);
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
        // ReSharper disable once FunctionNeverReturns
    }
}
