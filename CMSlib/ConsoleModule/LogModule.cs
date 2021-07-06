using System;
using System.Collections.Generic;
using System.Text;
using CMSlib.Extensions;
using Microsoft.Extensions.Logging;
using System.Linq;
namespace CMSlib.ConsoleModule
{
    public sealed class LogModule : BaseModule
    {
        private  readonly List<string> text = new();
        private  readonly char? borderCharacter;

        

        public LogModule(string title, int x, int y, int width, int height,
            char? borderCharacter = null, LogLevel minimumLogLevel = LogLevel.Information) : base(title, x, y, width, height, minimumLogLevel)
        {
            this.borderCharacter = borderCharacter;
        }
        
        public override void Clear(bool refresh = true)
        {
            lock (AddTextLock)
            {
                scrolledLines = 0;
                text.Clear();
            }
            if(refresh)
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
                int before = this.text.Count;
                this.text.AddRange(text.Replace("\t", "        ").Replace("\r\n", "\n").Split('\n').SelectMany(x=>x.PadToVisibleDivisible(Width).SplitOnNonEscapeLength(Width)));
                if (scrolledLines != 0)
                {
                    scrolledLines += this.text.Count - before;
                    unread = true;
                }
                
            }
        }
        
        /// <summary>
        /// Gets the string representation of this Module.
        /// </summary>
        /// <returns>The string representation of this module.</returns>
        public override string ToString()
        {
            return BoxRenderer.Render(Title, borderCharacter, X, Y, Width, Height, scrolledLines, text, selected, DisplayName,
                false, unread, null);
        }

        public override void ScrollUp(int amt)
        {
            if (this.text.Count == 0) return;
            lock (AddTextLock)
            {
                int before = scrolledLines;
                scrolledLines = Math.Clamp(scrolledLines + amt, 0, this.text.Count - 1);
                if (before != scrolledLines) WriteOutput();
            }
        }

        public override void ScrollTo(int line)
        {
            if (this.text.Count == 0) return;
            lock (AddTextLock)
            {
                int before = scrolledLines;
                scrolledLines = Math.Clamp(line, 0, this.text.Count - 1);
                if (before != scrolledLines) WriteOutput();
            }
        }

        public override void PageDown()
        {
            ScrollDown(Height);
        }

        public override void PageUp()
        {
            ScrollUp(Height);
        }
    }

    
    
}