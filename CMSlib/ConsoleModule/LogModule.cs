using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMSlib.Extensions;
using Microsoft.Extensions.Logging;

namespace CMSlib.ConsoleModule
{
    public class LogModule : BaseModule
    {
        protected readonly List<string> Text = new();
        protected readonly char? BorderCharacter;
        public bool TopDown { get; init; } = false;


        public LogModule(string title, int x, int y, int width, int height,
            char? borderCharacter = null, LogLevel minimumLogLevel = LogLevel.Information) : base(title, x, y, width,
            height, minimumLogLevel)
        {
            BorderCharacter = borderCharacter;
        }

        public override void Clear(bool refresh = true)
        {
            lock (AddTextLock)
            {
                scrolledLines = 0;
                Text.Clear();
            }

            if (refresh)
                WriteOutput();
        }

        /// <summary>
        /// Adds line(s) of text to this module. This supports \n, and \n will properly add text to the next line.
        /// </summary>
        /// <param name="text">The text to add</param>
        public override void AddText(string text)
        {
            lock (AddTextLock)
            {
                int before = this.Text.Count;
                Text.AddRange(text.Replace("\t", "        ").Replace("\r\n", "\n").Split('\n')
                    .SelectMany(x => x.PadToVisibleDivisible(Width - 2).SplitOnNonEscapeLength(Width - 2)));
                if (scrolledLines != 0)
                {
                    scrolledLines += this.Text.Count - before;
                    Unread = true;
                }
            }
        }

        /// <summary>
        /// Gets the string representation of this Module.
        /// </summary>
        /// <returns>The string representation of this module.</returns>
        protected override IEnumerable<string> ToOutputLines()
        {
            return BoxRenderer.Render(Title, BorderCharacter, X, Y, Width, Height, scrolledLines, Text, Selected,
                DisplayName,
                false, Unread, null, TopDown);
        }

        public override void ScrollUp(int amt)
        {
            if (Text.Count == 0) return;
            lock (AddTextLock)
            {
                int before = scrolledLines;
                scrolledLines = Math.Clamp(scrolledLines + amt, 0, Math.Max(Text.Count - (Height - 2), 0));
                if (before != 0 && scrolledLines == 0)
                    Unread = false;
                if (before != scrolledLines) WriteOutput();
            }
        }

        public override void ScrollTo(int line)
        {
            if (Text.Count == 0) return;
            lock (AddTextLock)
            {
                int before = scrolledLines;
                scrolledLines = Math.Clamp(line, 0, Math.Max(Text.Count - (Height - 2), 0));
                if (before != 0 && scrolledLines == 0)
                    Unread = false;
                if (before != scrolledLines) WriteOutput();
            }
        }

        internal async override Task HandleClickAsync(InputRecord record, ButtonState? before)
        {
        }

        internal async override Task HandleKeyAsync(ConsoleKeyInfo info)
        {
        }

        public override void PageDown()
        {
            ScrollDown(Height - 2);
        }

        public override void PageUp()
        {
            ScrollUp(Height - 2);
        }
    }
}