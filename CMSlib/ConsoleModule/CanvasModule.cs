using CMSlib.ConsoleModule;
using System;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using CMSlib.Extensions;
using Microsoft.Extensions.Logging;

namespace CMSlib.ConsoleModule
{
    public class CanvasModule : BaseModule
    {
        string[][] cells;
        private const string emptyReset = AnsiEscape.SgrClear + " ";
        private CanvasModule()
        {
        }

        public CanvasModule(string title, int x, int y, int width, int height, string fill = emptyReset) : base(title, x, y, width, height,
            LogLevel.None)
        {
            cells = new string[height - 2][];
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = new string[width - 2];
                Array.Fill(cells[i], fill);
            }
        }

        public void SetCell(int x, int y, string value)
        {
            cells[y][x] = value ?? emptyReset;
        }

        protected override IEnumerable<string> ToOutputLines()
        {
            if (Console.WindowHeight <= this.Y) yield break;
            int innerWidth = Math.Min(this.Width - 2, +Console.WindowWidth - this.X - 2);
            string displayName = this.DisplayName ?? this.Title;
            yield return AnsiEscape.LineDrawingMode + AnsiEscape.UpperLeftCorner + AnsiEscape.AsciiMode +
                         displayName.Ellipse(innerWidth) + AnsiEscape.LineDrawingMode +
                         new string(AnsiEscape.HorizontalLine, innerWidth - displayName.VisibleLength()) +
                         AnsiEscape.UpperRightCorner;
            for (int i = 0; i < Math.Min(this.Height - 2, Y + Console.WindowHeight - 2); i++)
            {
                yield return AnsiEscape.VerticalLine + AnsiEscape.AsciiMode + string.Concat(cells[i]) +
                             AnsiEscape.LineDrawingMode + AnsiEscape.VerticalLine;
            }

            yield return AnsiEscape.LineDrawingMode + AnsiEscape.LowerLeftCorner +
                         new string(AnsiEscape.HorizontalLine, innerWidth) + AnsiEscape.LowerRightCorner;
        }

        internal override async System.Threading.Tasks.Task HandleClickAsync(InputRecord record, ButtonState? before)
        {
        }

        internal override async System.Threading.Tasks.Task HandleKeyAsync(ConsoleKeyInfo info)
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
            foreach (var line in cells)
            {
                System.Array.Fill(line, emptyReset);
            }
        }
    }
}