using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMSlib.Extensions;
using Microsoft.Extensions.Logging;

namespace CMSlib.ConsoleModule
{
    public class CanvasModule : BaseModule
    {
        string[][] _cells;
        private const string EmptyReset = AnsiEscape.SgrClear + " ";
        private List<Coord> _dirty = new();

        private CanvasModule()
        {
        }

        public CanvasModule(string title, int x, int y, int width, int height, string fill = EmptyReset) : base(title,
            x, y, width, height,
            LogLevel.None)
        {
            _cells = new string[height - 2][];
            for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i] = new string[width - 2];
                Array.Fill(_cells[i], fill);
            }
        }

        public void SetCell(int x, int y, string value)
        {
            if (_cells[y][x].Equals(value)) return;
            _cells[y][x] = value ?? EmptyReset;
            lock (_dirty)
                _dirty.Add(new(x, y));
        }

        protected override IEnumerable<string> ToOutputLines()
        {
            if (Console.WindowHeight <= this.Y) yield break;
            int innerWidth = Math.Min(this.Width - 2, +Console.WindowWidth - this.X - 2);
            string displayName = this.DisplayName ?? this.Title;
            yield return AnsiEscape.LineDrawingMode + AnsiEscape.UpperLeftCorner + AnsiEscape.AsciiMode +
                         (Selected
                             ? AnsiEscape.Underline(displayName.Ellipse(innerWidth))
                             : displayName.Ellipse(innerWidth)) + AnsiEscape.LineDrawingMode +
                         new string(AnsiEscape.HorizontalLine, innerWidth - displayName.VisibleLength()) +
                         AnsiEscape.UpperRightCorner;
            for (int i = 0; i < Math.Min(this.Height - 2, Y + Console.WindowHeight - 2); i++)
            {
                yield return AnsiEscape.VerticalLine + AnsiEscape.AsciiMode + string.Concat(_cells[i]) +
                             AnsiEscape.SgrClear + AnsiEscape.LineDrawingMode + AnsiEscape.VerticalLine;
            }

            yield return AnsiEscape.LineDrawingMode + AnsiEscape.LowerLeftCorner +
                         new string(AnsiEscape.HorizontalLine, innerWidth) + AnsiEscape.LowerRightCorner;
        }

        public void QuickWriteOutput()
        {
            if (this.Parent is null) return;
            lock (this.Parent.WriteLock)
            lock (_dirty)
            {
                Parent.Write(AnsiEscape.SgrClear);
                foreach (Coord c in _dirty)
                {
                    this.Parent.SetCursorPosition(c.X + 1 + this.X, c.Y + 1 + this.Y);
                    Parent.Write(_cells[c.Y][c.X]);
                }

                Parent.Write(AnsiEscape.SgrClear);
                _dirty.Clear();
            }

            Parent.Flush();
        }

        internal override async Task HandleClickAsync(InputRecord record, ButtonState? before)
        {
        }

        internal override async Task HandleKeyAsync(ConsoleKeyInfo info)
        {
        }

        public override void AddText(string text)
        {
        }

        public override void ScrollUp(int amt)
        {
        }

        public override void ScrollTo(int line)
        {
        }

        public override void PageDown()
        {
        }

        public override void PageUp()
        {
        }

        public override void Clear(bool refresh = true)
        {
            foreach (var line in _cells)
            {
                Array.Fill(line, EmptyReset);
            }

            if (refresh)
                this.WriteOutput();
        }

        public record Coord(int X, int Y);
    }
}