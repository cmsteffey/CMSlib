using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CMSlib.Extensions;
using Microsoft.Extensions.Logging;

namespace CMSlib.ConsoleModule
{
    public class ProgressBarModule : BaseModule
    {
        private readonly object _statLock = new();
        private long _current;
        private long _max;

        public long Current
        {
            get => _current;
            set
            {
                lock (_statLock) _current = Math.Clamp(value, 0, _max);
            }
        }

        public long Max
        {
            get => _max;
            set
            {
                lock (_statLock) _max = value;
            }
        }

        public override void AddText(string text)
        {
        }

        public override void Clear(bool refresh = true)
        {
        }

        public override void PageDown()
        {
        }

        public override void PageUp()
        {
        }

        public override void ScrollUp(int amt)
        {
        }

        public override void ScrollTo(int line)
        {
        }

        private static readonly char[] Fillers = new char[] {' ', '▌'};
        private const string Filler = AnsiEscape.SgrBrightGreenForeGround;

        public ProgressBarModule(string title, int x, int y, int width, int height, long max) : base(title, x, y, width,
            height, LogLevel.None)
        {
            _max = max;
        }

        internal async override Task HandleClickAsync(InputRecord record, ButtonState? before)
        {
        }

        internal async override Task HandleKeyAsync(ConsoleKeyInfo info)
        {
        }

        public void QuickWriteOutput()
        {
            int internalWidth = Width - 2;
            long currPos = ToInternalFullX(_current);
            int len = (int) (currPos / 2);
            for (int i = 0; i < Height - 2; ++i)
            {
                Parent.SetCursorPosition(this.X + 1, this.Y + 1 + i);
                Parent.Write(Filler + new string('█', len) +
                             (len == internalWidth ? "" : Fillers[currPos % 2].ToString()) + AnsiEscape.SgrClear +
                             (len < internalWidth ? new string(' ', internalWidth - len - 1) : ""));
            }

            Parent.Flush();
        }

        private long ToInternalFullX(long pos)
        {
            long internalWidth = (this.Width - 2) * 2;
            double rat = pos / (double) _max;
            return (long) (rat * internalWidth);
        }

        protected override IEnumerable<string> ToOutputLines()
        {
            StringBuilder sb = new();
            string displayTitle = DisplayName ?? Title;
            int displayTitleLen = displayTitle.VisibleLength();
            int internalWidth = Width - 2;
            long currPos = ToInternalFullX(_current);
            int len = (int) (currPos / 2);
            yield return sb.Append(AnsiEscape.LineDrawingMode).Append(AnsiEscape.UpperLeftCorner)
                .Append(AnsiEscape.AsciiMode)
                .Append(displayTitleLen > internalWidth ? displayTitle.Ellipse(internalWidth) : displayTitle)
                .Append(AnsiEscape.LineDrawingMode)
                .Append(AnsiEscape.HorizontalLine, Math.Max(0, Width - 2 - displayTitleLen))
                .Append(AnsiEscape.UpperRightCorner).ToString();
            for (int i = 0; i < Height - 2; ++i)
            {
                yield return sb.Clear().Append(AnsiEscape.VerticalLine).Append(AnsiEscape.AsciiMode).Append(Filler)
                    .Append('█', len).Append(len == internalWidth ? "" : Fillers[currPos % 2].ToString())
                    .Append(AnsiEscape.SgrClear)
                    .Append(len < internalWidth ? new string(' ', internalWidth - len - 1) : "")
                    .Append(AnsiEscape.LineDrawingMode).Append(AnsiEscape.VerticalLine).ToString();
            }

            yield return sb.Clear().Append(AnsiEscape.LineDrawingMode).Append(AnsiEscape.LowerLeftCorner)
                .Append(AnsiEscape.HorizontalLine, internalWidth).Append(AnsiEscape.LowerRightCorner).ToString();
        }
    }
}