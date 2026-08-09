using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;

namespace Balu.Interactive;

delegate object? LineRenderHandler(IReadOnlyList<string> lines, int lineIndex, int start, int length, TextWriter writer, object? state);

sealed class SubmissionView
{
    sealed class UpdateDisposable : IDisposable
    {
        readonly SubmissionView parent;

        public UpdateDisposable(SubmissionView parent)
        {
            this.parent = parent;
            parent.updatesInProgress++;
        }

        public void Dispose()
        {
            parent.updatesInProgress--;
            if (parent.updatesInProgress == 0) parent.Render();
        }
    }

    const int PromptWidth = 2;

    readonly LineRenderHandler lineRenderer;
    readonly IConsole console;
    int cursorTop;
    int renderedRowsCount;
    int firstVisibleLine;
    int firstVisibleColumn;
    int cursorX;
    int cursorY;
    int updatesInProgress;

    public ObservableCollection<string> SubmissionDocument { get; }
    public int CursorX
    {
        get => cursorX;
        set
        {
            if (value == cursorX) return;
            cursorX = value;
            Render();
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
            Render();
        }
    }

    public SubmissionView(ObservableCollection<string> submissionDocument, LineRenderHandler lineRenderer, IConsole? console = null)
    {
        SubmissionDocument = submissionDocument;
        this.lineRenderer = lineRenderer;
        this.console = console ?? SystemConsole.Instance;
        SubmissionDocument.CollectionChanged += OnSubmissionDocumentChanged;
        cursorTop = this.console.CursorTop;
        Render();
    }

    void OnSubmissionDocumentChanged(object? sender, NotifyCollectionChangedEventArgs e) => Render();

    void Render()
    {
        if (updatesInProgress > 0) return;

        var cursorVisible = console.CursorVisible;
        var foregroundColor = console.ForegroundColor;
        var backgroundColor = console.BackgroundColor;
        try
        {
            console.CursorVisible = false;
            for (int attempt = 0; attempt < 2; attempt++)
                if (RenderCore(foregroundColor)) break;
        }
        finally
        {
            console.ForegroundColor = foregroundColor;
            console.BackgroundColor = backgroundColor;
            console.CursorVisible = cursorVisible;
        }
    }

    bool RenderCore(ConsoleColor foregroundColor)
    {
        int bufferWidth = Math.Max(1, console.BufferWidth);
        int bufferHeight = Math.Max(1, console.BufferHeight);
        int windowWidth = Math.Max(1, Math.Min(console.WindowWidth, bufferWidth));
        int windowHeight = Math.Max(1, Math.Min(console.WindowHeight, bufferHeight));
        int renderWidth = Math.Max(0, windowWidth - 1);
        int promptWidth = Math.Min(PromptWidth, renderWidth);
        int contentWidth = Math.Max(0, renderWidth - promptWidth);

        cursorTop = Math.Clamp(cursorTop, 0, bufferHeight - 1);
        int desiredRows = Math.Min(Math.Max(Math.Min(SubmissionDocument.Count, windowHeight), renderedRowsCount), windowHeight);
        if (!EnsureRowsFitInConsoleBuffer(desiredRows, bufferHeight)) return false;
        int viewportHeight = Math.Max(1, Math.Min(windowHeight, bufferHeight - cursorTop));

        UpdateViewport(viewportHeight, contentWidth);

        int visibleRowsCount = Math.Min(SubmissionDocument.Count - firstVisibleLine, viewportHeight);
        int rowsToRender = Math.Min(Math.Max(visibleRowsCount, renderedRowsCount), viewportHeight);
        object? state = null;
        for (int row = 0; row < rowsToRender; row++)
        {
            if (!console.TrySetCursorPosition(0, cursorTop + row)) return false;
            if (row >= visibleRowsCount)
            {
                console.Out.Write(new string(' ', renderWidth));
                continue;
            }

            int lineIndex = firstVisibleLine + row;
            var prompt = lineIndex == 0 ? "» " : "· ";
            console.ForegroundColor = ConsoleColor.Green;
            console.Out.Write(prompt[..promptWidth]);
            console.ForegroundColor = foregroundColor;

            var line = SubmissionDocument[lineIndex];
            int start = Math.Min(firstVisibleColumn, line.Length);
            int length = Math.Min(contentWidth, line.Length - start);
            state = lineRenderer(SubmissionDocument, lineIndex, start, length, console.Out, state);
            console.Out.Write(new string(' ', renderWidth - promptWidth - length));
        }

        if (rowsToRender >= renderedRowsCount) renderedRowsCount = visibleRowsCount;
        return UpdateCursorPosition(promptWidth);
    }

    bool EnsureRowsFitInConsoleBuffer(int rowsCount, int bufferHeight)
    {
        while (cursorTop + rowsCount > bufferHeight)
        {
            if (!console.TrySetCursorPosition(0, bufferHeight - 1)) return false;
            console.WriteLine();
            if (cursorTop > 0) cursorTop--;
        }
        return true;
    }

    void UpdateViewport(int viewportHeight, int contentWidth)
    {
        if (cursorY < firstVisibleLine)
            firstVisibleLine = cursorY;
        else if (cursorY >= firstVisibleLine + viewportHeight)
            firstVisibleLine = cursorY - viewportHeight + 1;

        int maximumFirstVisibleLine = Math.Max(0, SubmissionDocument.Count - viewportHeight);
        firstVisibleLine = Math.Clamp(firstVisibleLine, 0, maximumFirstVisibleLine);

        if (contentWidth == 0)
        {
            firstVisibleColumn = cursorX;
            return;
        }

        if (cursorX < firstVisibleColumn)
            firstVisibleColumn = cursorX;
        else if (cursorX >= firstVisibleColumn + contentWidth)
            firstVisibleColumn = cursorX - contentWidth + 1;
    }

    bool UpdateCursorPosition(int promptWidth) =>
        console.TrySetCursorPosition(promptWidth + cursorX - firstVisibleColumn, cursorTop + cursorY - firstVisibleLine);

    public IDisposable CreateUpdateContext() => new UpdateDisposable(this);
}
