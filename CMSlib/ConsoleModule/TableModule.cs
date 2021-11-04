using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMSlib.Extensions;
using CMSlib.Tables;
using Microsoft.Extensions.Logging;
using static CMSlib.ConsoleModule.AnsiEscape;

namespace CMSlib.ConsoleModule
{
    public class TableModule : BaseModule
    {
        private (string line, object[] rowObjs)[] _lineCache;
        private readonly Table _wrapped;
        private string _header;
        private int _selected;
        private readonly object _lineCacheLock = new();
        private readonly object _selectedLock = new();

        public object[] SelectedRowObjs
        {
            get => _selected < 0 || _selected >= _lineCache.Length ? null : _lineCache[_selected].rowObjs;
        }

        public int SelectedLine
        {
            get => _selected;
            set
            {
                lock (_selectedLock) _selected = value;
            }
        }

        public TableModule(string title, int x, int y, int width, int height, Table toWrap, string header = null) :
            base(title, x, y, width, height, LogLevel.None)
        {
            _wrapped = toWrap;
            _header = (header ?? toWrap.GetHeader()).SplitOnNonEscapeLength(width - 2).First()
                .PadToVisibleDivisible(width - 2);
            _lineCache = _wrapped.GetOutputRows().Select((x, i) =>
            {
                if (x.VisibleLength() > Width - 2)
                    return (x.SplitOnNonEscapeLength(Width - 2).First(), _wrapped[i].SectionItems);
                return (x.PadToVisibleDivisible(Width - 2), _wrapped[i].SectionItems);
            }).ToArray();
        }

        public override void AddText(string text)
        {
        }

        public override void ScrollUp(int amt)
        {
            if (_lineCache.Length == 0) return;
            int before = scrolledLines;
            scrolledLines = Math.Clamp(scrolledLines - amt, 0, Math.Max(0, _lineCache.Length - (this.Height - 3)));
            if (before == scrolledLines) return;
            WriteOutput();
        }


        public override void ScrollTo(int line)
        {
            if (_lineCache.Length == 0) return;
            int before = scrolledLines;
            scrolledLines = Math.Clamp(line, 0, Math.Max(0, _lineCache.Length - (this.Height - 3)));
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
            _wrapped.ClearRows();
        }

        public void RefreshHeader()
        {
            this._header = (_wrapped.GetHeader()).SplitOnNonEscapeLength(Width - 2).First()
                .PadToVisibleDivisible(Width - 2);
        }

        public void RefreshLineCache()
        {
            int before = scrolledLines;
            lock (_lineCacheLock)
                _lineCache = _wrapped.GetOutputRows().Select((x, i) =>
                {
                    if (x.VisibleLength() > Width - 2)
                        return (x.SplitOnNonEscapeLength(Width - 2).First(), _wrapped[i].SectionItems);
                    return (x.PadToVisibleDivisible(Width - 2), _wrapped[i].SectionItems);
                }).ToArray();
            if (_lineCache.Length == 0) return;
            scrolledLines = Math.Clamp(scrolledLines, 0, Math.Max(0, _lineCache.Length - (this.Height - 3)));
            if (before != scrolledLines) WriteOutput();
        }


        internal override async Task HandleClickAsync(InputRecord record, ButtonState? before)
        {
            int relX = record.MouseEvent.MousePosition.X - X;
            int relY = record.MouseEvent.MousePosition.Y - Y;
            long row;
            if ((!before.HasValue ||
                 record.MouseEvent.ButtonState != before) &&
                relX > 0 && relX < Width - 1 &&
                relY > 1 && relY < Height - 1 &&
                (row = relY - 2 + scrolledLines) >= 0 && row < (_lineCache.LongLength))
            {
                RowClickedEventArgs e = new() {RowObjs = _lineCache[row].rowObjs, RowIndex = row};
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
                if (_selected < scrolledLines)
                    ScrollTo(_selected);
                else if (_selected >= scrolledLines + internalHeight)
                {
                    ScrollTo(_selected - internalHeight);
                }

                return scrolledLines != before;
            }

            if (_selected < 0 || _lineCache.Length == 0) return;
            RowClickedEventArgs e;
            switch (info.Key)
            {
                case ConsoleKey.UpArrow:
                    lock (_selectedLock)
                        _selected = (_selected - 1).Modulus(_lineCache.Length);
                    if (!CorrectScroll())
                        WriteOutput();
                    break;

                case ConsoleKey.DownArrow:
                    lock (_selectedLock)
                        _selected = (_selected + 1).Modulus(_lineCache.Length);
                    if (!CorrectScroll())
                        WriteOutput();
                    break;
                case ConsoleKey.Enter when info.Modifiers.HasFlag(ConsoleModifiers.Control):
                    e = new() {RowObjs = _lineCache[_selected].rowObjs, RowIndex = _selected};
                    FireRowRightClicked(e);
                    break;
                case ConsoleKey.Enter:
                    e = new() {RowObjs = _lineCache[_selected].rowObjs, RowIndex = _selected};
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

        public class RowClickedEventArgs : EventArgs
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
            if (internalHeight < 2) yield break;
            string displayTitle = DisplayName ?? this.Title;
            int visLen = displayTitle.VisibleLength();
            if (visLen > internalWidth)
            {
                displayTitle = displayTitle.Ellipse(internalWidth);
                visLen = displayTitle.VisibleLength();
            }

            int relSelected = _selected - scrolledLines;
            yield return LineDrawingMode + UpperLeftCorner + AsciiMode +
                         (Selected ? SgrUnderline + SgrBlinking : "") + displayTitle +
                         (Selected ? SgrNoUnderline + SgrNoBlinking : "") + LineDrawingMode +
                         new string(HorizontalLine, internalWidth - visLen) + UpperRightCorner;
            yield return VerticalLine + AsciiMode + _header + LineDrawingMode + VerticalLine;
            int lineCount = Math.Min(internalHeight, _lineCache.Length);
            int spaceCount = internalHeight - lineCount;
            for (int i = 0; i < lineCount; i++)
            {
                string start = i != relSelected
                    ? VerticalLine + AsciiMode
                    : AsciiMode + SgrBlackForeGround + SgrBrightYellowBackGround + ">" + SgrClear;
                yield return start + _lineCache[scrolledLines + i].line + LineDrawingMode +
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