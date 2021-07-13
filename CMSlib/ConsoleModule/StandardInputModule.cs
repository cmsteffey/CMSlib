using System;
using CMSlib.Extensions;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace CMSlib.ConsoleModule
{
    public sealed class StandardInputModule : InputModule
    {
        
        private  readonly List<string> text = new();
        private  readonly char? borderCharacter;

        

        public StandardInputModule(string title, int x, int y, int width, int height,
            char? borderCharacter = null, LogLevel minimumLogLevel = LogLevel.Information) : base(title, x, y, width, height, minimumLogLevel)
        {
            this.borderCharacter = borderCharacter;
        }

        internal override void HandleClickAsync(InputRecord record)
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
                text.Clear();
            }

            if(refresh)
                WriteOutput();
        }

        internal override void Backspace(bool write = true)
        {
            if (parent is null) return;
            lock (this.parent.writeLock)
            {
                this.inputString.Remove(inputString.Length - 1, 1);
                
                
                if (inputString.Length + 1 > Width - 3)
                {
                    if (!write) return;
                    this.WriteOutput();
                }
                else
                {
                    lrCursorPos--;
                    if (!write) return;
                    Console.Write("\b \b");
                }
                
            }
        }
        
        internal override void AddChar(char toAdd)
        {
            if (parent is null) return;
            if (toAdd is '\u0000') return;
            lock (this.parent.writeLock)
            {
                this.inputString.Append(toAdd);
                if (inputString.Length > Width - 3)
                {
                    this.WriteOutput();
                }
                else
                {
                    Console.Write(toAdd);
                    lrCursorPos++;
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
                int before = this.text.Count;
                this.text.AddRange(text.Replace("\t", "        ").Replace("\r\n", "\n").Split('\n').SelectMany(x=>x.PadToVisibleDivisible(Width - 2).SplitOnNonEscapeLength(Width - 2)));
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
                true, unread, inputString);
        }

        public override void ScrollUp(int amt)
        {
            if (this.text.Count == 0) return;
            lock (AddTextLock)
            {
                int before = scrolledLines;
                scrolledLines = Math.Clamp(scrolledLines + amt, 0, this.text.Count - 1);
                if (before != 0 && scrolledLines == 0)
                    unread = false;
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
                if (before != 0 && scrolledLines == 0)
                    unread = false;
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

