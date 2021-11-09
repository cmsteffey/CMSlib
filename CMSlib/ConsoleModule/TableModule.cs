using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMSlib.Extensions;
using CMSlib.Tables;
using CMSlib.ConsoleModule;
using Microsoft.Extensions.Logging;
using static CMSlib.ConsoleModule.AnsiEscape;

namespace CMSlib.ConsoleModule
{
    public class TableModule : BaseModule
    {
        private (string line, object[] rowObjs)[] lineCache;
        private Table wrapped;
        private string header;
        private int selected = 0;
        private object lineCacheLock = new();
        private object selectedLock = new();

        public object[] SelectedRowObjs
        {
            get => selected < 0 || selected >= lineCache.Length ? null : lineCache[selected].rowObjs;
        }

        public int SelectedLine
        {
            get => selected;
            set
            {
                lock (selectedLock) selected = value;
            }
        }

        public TableModule(string title, int x, int y, int width, int height, Table toWrap, string header = null) :
            base(title, x, y, width, height, LogLevel.None)
        {
            wrapped = toWrap;
            this.header = (header ?? toWrap.GetHeader()).SplitOnNonEscapeLength(width - 2).First()
                .PadToVisibleDivisible(width - 2);
            lineCache = wrapped.GetOutputRows().Select((x, i) =>
            {
                if (x.VisibleLength() > Width - 2)
                    return (x.SplitOnNonEscapeLength(Width - 2).First(), wrapped[i].SectionItems);
                return (x.PadToVisibleDivisible(Width - 2), wrapped[i].SectionItems);
            }).ToArray();
        }

        public override void AddText(string text)
        {
        }

        public override void ScrollUp(int amt)
        {
            if (lineCache.Length == 0) return;
            int before = scrolledLines;
            scrolledLines = Math.Clamp(scrolledLines - amt, 0, Math.Max(0, lineCache.Length - (this.Height - 3)));
            if (before == scrolledLines) return;
            WriteOutput();
        }


        public override void ScrollTo(int line)
        {
            if (lineCache.Length == 0) return;
            int before = scrolledLines;
            scrolledLines = Math.Clamp(line, 0, Math.Max(0, lineCache.Length - (this.Height - 3)));
            if (before == scrolledLines) return;
            WriteOutput();
        }

        public override void PageUp()
        {
            ScrollUp(this.Height - 3);
        }

        public override void PageDown()
        {
            ScrollDown(this.Height - 3);
        }

        public override void Clear(bool refresh = true)
        {
            wrapped.ClearRows();
        }

        public void RefreshHeader()
        {
            this.header = (wrapped.GetHeader()).SplitOnNonEscapeLength(Width - 2).First()
                .PadToVisibleDivisible(Width - 2);
        }

        public void RefreshLineCache()
        {
            int before = scrolledLines;
            lock (lineCacheLock)
                lineCache = wrapped.GetOutputRows().Select((x, i) =>
                {
                    if (x.VisibleLength() > Width - 2)
                        return (x.SplitOnNonEscapeLength(Width - 2).First(), wrapped[i].SectionItems);
                    return (x.PadToVisibleDivisible(Width - 2), wrapped[i].SectionItems);
                }).ToArray();
            if (lineCache.Length == 0) return;
            scrolledLines = Math.Clamp(scrolledLines, 0, Math.Max(0, lineCache.Length - (this.Height - 3)));
            if (before != scrolledLines) WriteOutput();
        }


        internal override async Task HandleClickAsync(InputRecord record, ButtonState? before)
        {
            int relX = (int) record.MouseEvent.MousePosition.X - X;
            int relY = (int) record.MouseEvent.MousePosition.Y - Y;
            long row;
            if ((!before.HasValue ||
                 record.MouseEvent.ButtonState != before) &&
                relX > 0 && relX < Width - 1 &&
                relY > 1 && relY < Height - 1 &&
                (row = relY - 2 + scrolledLines) >= 0 && row < (lineCache.LongLength))
            {
                RowClickedEventArgs e = new() {RowObjs = lineCache[row].rowObjs, RowIndex = row};
                bool left = record.MouseEvent.ButtonState.HasFlag(ButtonState.Left1Pressed);
                bool right = record.MouseEvent.ButtonState.HasFlag(ButtonState.RightPressed);
                if (!(left ^ right)) return;
                if (right)
                {
                    FireRowRightClicked(e);
                    return;
                }

                FireRowClicked(e);
            }
        }

        internal override async Task HandleKeyAsync(ConsoleKeyInfo info)
        {
            int internalHeight = Math.Min(Console.WindowHeight - Y, Height) - 4;

            bool CorrectScroll()
            {
                int before = scrolledLines;
                if (selected < scrolledLines)
                    ScrollTo(selected);
                else if (selected >= scrolledLines + internalHeight)
                {
                    ScrollTo(selected - internalHeight);
                }

                return scrolledLines != before;
            }

            if (selected < 0 || lineCache.Length == 0) return;
            RowClickedEventArgs e;
            switch (info.Key)
            {
                case ConsoleKey.UpArrow:
                    lock (selectedLock)
                        selected = (selected - 1).Modulus(lineCache.Length);
                    if (!CorrectScroll())
                        WriteOutput();
                    break;

                case ConsoleKey.DownArrow:
                    lock (selectedLock)
                        selected = (selected + 1).Modulus(lineCache.Length);
                    if (!CorrectScroll())
                        WriteOutput();
                    break;
                case ConsoleKey.Enter when info.Modifiers.HasFlag(ConsoleModifiers.Control):
                    e = new() {RowObjs = lineCache[selected].rowObjs, RowIndex = selected};
                    FireRowRightClicked(e);
                    break;
                case ConsoleKey.Enter:
                    e = new() {RowObjs = lineCache[selected].rowObjs, RowIndex = selected};
                    FireRowClicked(e);
                    break;
            }
        }

        public event EventHandler<RowClickedEventArgs> RowClicked;
        public event EventHandler<RowClickedEventArgs> RowRightClicked;

        private void FireRowClicked(RowClickedEventArgs e)
        {
            var handler = RowClicked;
            if (handler is not null)
                handler(this, e);
        }

        private void FireRowRightClicked(RowClickedEventArgs e)
        {
            var handler = RowRightClicked;
            if (handler is not null)
                handler(this, e);
        }

        public class RowClickedEventArgs : System.EventArgs
        {
            public object[] RowObjs { get; internal init; }
            public long RowIndex { get; internal init; }

            internal RowClickedEventArgs()
            {
            }
        }

        protected override IEnumerable<string> ToOutputLines()
        {
            int internalHeight = Math.Min(Height - 3, Console.WindowHeight - Y - 3);
            int internalWidth = Width - 2;
            if (internalHeight < 0) yield break;
            string displayTitle = DisplayName ?? this.Title;
            int visLen = displayTitle.VisibleLength();
            if (visLen > internalWidth)
            {
                displayTitle = displayTitle.Ellipse(internalWidth);
                visLen = displayTitle.VisibleLength();
            }

            int relSelected = selected - scrolledLines;
            yield return LineDrawingMode + UpperLeftCorner + AsciiMode +
                         (base.selected ? SgrUnderline + SgrBlinking : "") + displayTitle +
                         (base.selected ? SgrNoUnderline + SgrNoBlinking : "") + LineDrawingMode +
                         new string(HorizontalLine, internalWidth - visLen) + UpperRightCorner;
            yield return VerticalLine + AsciiMode + header + LineDrawingMode + VerticalLine;
            int lineCount = Math.Min(internalHeight, lineCache.Length);
            int spaceCount = internalHeight - lineCount;
            for (int i = 0; i < lineCount; i++)
            {
                string start = i != relSelected
                    ? VerticalLine + AsciiMode
                    : AsciiMode + SgrBlackForeGround + SgrBrightYellowBackGround + ">" + AnsiEscape.SgrClear;
                yield return start + lineCache[scrolledLines + i].line + LineDrawingMode +
                             VerticalLine;
            }

            string spaceLine = null;
            for (int i = 0; i < spaceCount; i++)
            {
                spaceLine ??= VerticalLine + new string('\x20', internalWidth) + VerticalLine;
                yield return spaceLine;
            }

            yield return LowerLeftCorner + new string(HorizontalLine, internalWidth) + LowerRightCorner;
        }
    }
}