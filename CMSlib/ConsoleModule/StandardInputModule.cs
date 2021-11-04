using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMSlib.Extensions;
using Microsoft.Extensions.Logging;

namespace CMSlib.ConsoleModule
{
    public sealed class StandardInputModule : InputModule
    {
        public bool TopDown { get; init; } = false;
        private readonly List<string> _text = new();
        private readonly char? _borderCharacter;

        public StandardInputModule(string title, int x, int y, int width, int height, char? borderCharacter = null,
            LogLevel minimumLogLevel = LogLevel.Information) : base(title, x, y, width, height, minimumLogLevel)
        {
            this._borderCharacter = borderCharacter;
        }

        internal async override Task HandleClickAsync(InputRecord record, ButtonState? before)
        {
        }

        internal async override Task HandleKeyAsync(ConsoleKeyInfo info)
        {
        }

        /// <summary>
        /// Clears all lines from this module, as well as optionally refreshing.
        /// </summary>
        /// <param name="refresh">Whether to refresh after clearing out the text</param>
        public override void Clear(bool refresh = true)
        {
            lock (AddTextLock)
            {
                scrolledLines = 0;
                _text.Clear();
            }

            if (refresh) WriteOutput();
        }

        public void Clear(bool refresh, bool resetScroll)
        {
            lock (AddTextLock)
            {
                if (resetScroll)
                {
                    scrolledLines = 0;
                }

                _text.Clear();
            }

            if (refresh) WriteOutput();
        }

        internal override void Backspace(bool write = true)
        {
            if (Parent is null) return;

            lock (Parent.WriteLock)
            {
                InputString.Remove(InputString.Length - 1, 1);

                if (InputString.Length + 1 > Width - 3)
                {
                    if (!write) return;
                    WriteOutput();
                }
                else
                {
                    LrCursorPos--;
                    if (!write) return;
                    Console.Write("\b \b");
                }
            }
        }

        internal override void AddChar(char toAdd)
        {
            if (Parent is null) return;
            if (toAdd is '\u0000') return;

            lock (Parent.WriteLock)
            {
                InputString.Append(toAdd);

                if (InputString.Length > Width - 3)
                {
                    WriteOutput();
                }
                else
                {
                    Console.Write(toAdd);
                    LrCursorPos++;
                }
            }
        }

        /// <summary>
        /// Adds line(s) of text to this module. This supports \n, and \n will properly add text to the next line.
        /// </summary>
        /// <param name="text">The text to add</param>
        public override void AddText(string text)
        {
            lock (AddTextLock)
            {
                int before = this._text.Count;

                this._text.AddRange(text.Replace("\t", "        ").Replace("\r\n", "\n").Split('\n')
                    .SelectMany(x => x.PadToVisibleDivisible(Width - 2).SplitOnNonEscapeLength(Width - 2)));

                if (scrolledLines != 0)
                {
                    scrolledLines += this._text.Count - before;
                    Unread = true;
                }
            }
        }


        protected override IEnumerable<string> ToOutputLines()
        {
            return BoxRenderer.Render(Title, _borderCharacter, X, Y, Width, Height, scrolledLines, _text, Selected,
                DisplayName, true, Unread, InputString, TopDown);
        }

        public override void ScrollUp(int amt)
        {
            if (_text.Count == 0) return;

            lock (AddTextLock)
            {
                int before = scrolledLines;
                scrolledLines = Math.Clamp(scrolledLines + amt, 0, Math.Max(_text.Count - (Height - 4), 0));

                if (before != 0 && scrolledLines == 0) Unread = false;
                if (before != scrolledLines) WriteOutput();
            }
        }

        public override void ScrollTo(int line)
        {
            if (_text.Count == 0) return;

            lock (AddTextLock)
            {
                int before = scrolledLines;
                scrolledLines = Math.Clamp(line, 0, Math.Max(_text.Count - (Height - 4), 0));

                if (before != 0 && scrolledLines == 0) Unread = false;
                if (before != scrolledLines) WriteOutput();
            }
        }

        public override void PageDown()
        {
            ScrollDown(Height - 4);
        }

        public override void PageUp()
        {
            ScrollUp(Height - 4);
        }
    }
}