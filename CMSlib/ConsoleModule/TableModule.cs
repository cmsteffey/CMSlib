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
        public TableModule(string title, int x, int y, int width, int height, Table toWrap, string header = null, 
            LogLevel logLevel = LogLevel.Information) : base(title, x, y, width, height, logLevel)
        {
            wrapped = toWrap;
            this.header = (header ?? toWrap.GetHeader()).SplitOnNonEscapeLength(width - 2).First().PadToVisibleDivisible(width - 2);
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
            if (before != scrolledLines) WriteOutput();
        }

        public override void ScrollTo(int line)
        {
            if (lineCache.Length == 0) return;
            int before = scrolledLines;
            scrolledLines = Math.Clamp(line, 0, Math.Max(0, lineCache.Length - (this.Height - 3)));
            if (before != scrolledLines) WriteOutput();
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
            this.header = (wrapped.GetHeader()).SplitOnNonEscapeLength(Width - 2).First().PadToVisibleDivisible(Width - 2);
        }

        public void RefreshLineCache()
        {
            lineCache = wrapped.GetOutputRows().Select((x, i) =>
            {
                if (x.VisibleLength() > Width - 2)
                    return (x.SplitOnNonEscapeLength(Width - 2).First(), wrapped[i].SectionItems);
                return (x.PadToVisibleDivisible(Width - 2), wrapped[i].SectionItems);
            }).ToArray();
        }

        internal override async Task HandleClickAsync(InputRecord record, ButtonState? before)
        {
	    int relX = (int)record.MouseEvent.MousePosition.X - X;
	    int relY = (int)record.MouseEvent.MousePosition.Y - Y;
            long row;
	    if ((!before.HasValue ||
	        record.MouseEvent.ButtonState != before) &&
		relX > 0 && relX < Width - 1 &&
		relY > 1 && relY < Height - 1 &&
		(row = relY - 2 + scrolledLines) >= 0 && row < (lineCache.LongLength)){
			await FireRowClickedAsync(new(){RowObjs = lineCache[row].rowObjs, RowIndex = row});
	    }	    
        }

        internal override async Task HandleKeyAsync(ConsoleKeyInfo info)
        {
            
        }
	public event AsyncEventHandler<RowClickedEventArgs> RowClicked;
	private async Task FireRowClickedAsync(RowClickedEventArgs e){
	    var handler = RowClicked;
	    if(handler is not null)
		await handler(this, e);
	}
	public class RowClickedEventArgs : System.EventArgs{
	    public object[] RowObjs {get; internal init;}
	    public long RowIndex {get; internal init;}
	    internal RowClickedEventArgs(){}
	}
        protected override IEnumerable<string> ToOutputLines()
        {
            int internalHeight = Height - 3;
            int internalWidth = Width - 2;
            if(internalHeight < 2) yield break;
            string displayTitle = DisplayName ?? this.Title;
            int visLen = displayTitle.VisibleLength();
            if (visLen > internalWidth)
            {
                displayTitle = displayTitle.Ellipse(internalWidth);
                visLen = displayTitle.VisibleLength();
            }

            yield return LineDrawingMode + UpperLeftCorner + AsciiMode +
                         (this.selected ? SgrUnderline + SgrBlinking : "") + displayTitle +
                         (this.selected ? SgrNoUnderline + SgrNoBlinking : "") + LineDrawingMode +
                         new string(HorizontalLine, internalWidth - visLen) + UpperRightCorner;
            yield return VerticalLine + AsciiMode + header + LineDrawingMode + VerticalLine;
            int lineCount = Math.Min(internalHeight, lineCache.Length);
            int spaceCount = internalHeight - lineCount;
            for (int i = 0; i < lineCount; i++)
            {
                yield return VerticalLine + AsciiMode + lineCache[scrolledLines + i].line + LineDrawingMode +
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