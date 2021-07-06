using System;
using CMSlib.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CMSlib.Tables;
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
            int actingHeight = Math.Min(Height, (Console.WindowHeight - 2) - Y);
            string actingTitle = DisplayName ?? Title;
            StringBuilder output = borderCharacter is not null ? new((Width + 2) * (actingHeight + 2) + AnsiEscape.AsciiMode.Length + AnsiEscape.SgrUnderline.Length * 2) : new();
            int inputDifferential = 2;
            int lineCount = Math.Clamp(text.Count - scrolledLines, 0, actingHeight - inputDifferential);
            int spaceCount =
                Math.Min(actingHeight - text.Count - inputDifferential + scrolledLines,
                    actingHeight - inputDifferential);
            if (lineCount <= 0 && spaceCount <= 0)
            {
                return string.Empty;
            }
            if (borderCharacter is null)
                output.Append(AnsiEscape.LineDrawingMode);
            else
                output.Append(AnsiEscape.AsciiMode);
            output.Append(borderCharacter??AnsiEscape.UpperLeftCorner);
            if (borderCharacter is null)
                output.Append(AnsiEscape.AsciiMode);
            
            if (selected)
                output.Append(AnsiEscape.SgrUnderline);
            output.Append(actingTitle.Ellipse(Width));
            
            if (selected)
                output.Append(AnsiEscape.SgrNoUnderline);
            if (borderCharacter is null)
                output.Append(AnsiEscape.LineDrawingMode);
            output.Append(borderCharacter??AnsiEscape.HorizontalLine, Width - actingTitle.Ellipse(Width).Length);
            output.Append(borderCharacter ?? AnsiEscape.UpperRightCorner);
            for (int i = 0; i < spaceCount; i++)
            {
                output.Append(borderCharacter?.ToString()??(unread && i > actingHeight - (4 + inputDifferential) ? AnsiEscape.AsciiMode + AnsiEscape.SgrRedForeGround + AnsiEscape.SgrBrightBold + "V" + AnsiEscape.SgrClear: AnsiEscape.VerticalLine.ToString()));
                output.Append(' ', Width);
                output.Append(borderCharacter?.ToString()??AnsiEscape.LineDrawingMode + AnsiEscape.VerticalLine);
            }
            int index = Math.Clamp(text.Count - (actingHeight - inputDifferential) - scrolledLines, 0, text.Count == 0 ? 0 : text.Count - 1);
            
            List<string> toPrint = text.GetRange(index, lineCount);
            
            
            for(int i = 0; i < toPrint.Count; i++)
            {
                output.Append(borderCharacter?.ToString()??(unread && i + Math.Max(spaceCount, 0) > actingHeight - (4 + inputDifferential) ? AnsiEscape.SgrRedForeGround + AnsiEscape.SgrBrightBold + "V" + AnsiEscape.SgrClear: AnsiEscape.VerticalLine.ToString()));
                if (borderCharacter is null) output.Append(AnsiEscape.AsciiMode);
                output.Append(toPrint[i]);
                bool dot = borderCharacter is null && i + Math.Max(spaceCount, 0) > Height - (2 + inputDifferential) && scrolledLines != 0;
                if (dot)
                {
                    output.Append(AnsiEscape.SgrGreenForeGround + AnsiEscape.SgrBrightBold + "." + AnsiEscape.SgrClear);
                }
                if (borderCharacter is null) output.Append(AnsiEscape.LineDrawingMode);
                output.Append(borderCharacter?.ToString()??(dot?"":AnsiEscape.VerticalLine.ToString()));
            }
            if(borderCharacter is null)
                output.Append(AnsiEscape.VerticalWithRight).Append(AnsiEscape.HorizontalLine, Width).Append(AnsiEscape.VerticalWithLeft);
            else
                output.Append(borderCharacter.Value, Width + 2);
            if(borderCharacter is null) 
                output
                    .Append(AnsiEscape.VerticalLine)
                    .Append(AnsiEscape.AsciiMode)
                    .Append(inputString)
                    .Append(' ', Width - inputString.Length)
                    .Append(AnsiEscape.LineDrawingMode)
                    .Append(AnsiEscape.VerticalLine)
                    .Append(AnsiEscape.LowerLeftCorner)
                    .Append(AnsiEscape.HorizontalLine, Width)
                    .Append(AnsiEscape.LowerRightCorner)
                    .Append(AnsiEscape.AsciiMode);
            else
                output.Append(borderCharacter).Append(inputString).Append(' ', Width - inputString.Length).Append(borderCharacter.Value, Width + 3);
            return output.ToString();
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

        
    }

   
}

