using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using CMSlib.Extensions;
namespace CMSlib.ConsoleModule
{
    public class ProgressBarModule : BaseModule
    {
	object statLock = new();
	int cachedQuickLen = -1;
	long current;
	long max;
	public long Current{get => current; set {lock(statLock) current = Math.Clamp(value, 0, max);}}
	public long Max{get => max; set {lock(statLock) max = value;}}
        public override void AddText(string text){}
        public override void Clear(bool refresh = true){}
        public override void PageDown(){}
        public override void PageUp(){}
        public override void ScrollUp(int amt){}
        public override void ScrollTo(int line){}
	private static char[] fillers = new char[]{' ','▌'};
	private const string filler = AnsiEscape.SgrBrightGreenForeGround;

        public ProgressBarModule(string title, int x, int y, int width, int height, long max): base(title, x, y, width, height, LogLevel.None){this.max = max;}
        internal async override Task HandleClickAsync(InputRecord record, ButtonState? before){}
        internal async override Task HandleKeyAsync(ConsoleKeyInfo info){}
	public void QuickWriteOutput(){
	    lock(Parent.writeLock){
	    int internalWidth = Width - 2;
	    long currPos = ToInternalFullX(current);
	    int len = (int)(currPos);
	    if(cachedQuickLen == len) return;
	    cachedQuickLen = len;
            len /= 2;
	    Parent.Write(AnsiEscape.DisableCursorVisibility);
	    for(int i = 0; i < Height - 2; ++i){
		Parent.SetCursorPosition(this.X+1, this.Y + 1 + i);
		Parent.Write((filler) + AnsiEscape.SgrBrightBlackBackGround + new string('█',len) + (len == internalWidth ? "" : fillers[currPos%2].ToString()) + (AnsiEscape.SgrClear) + AnsiEscape.SgrBrightBlackBackGround + (len < internalWidth ? new string(' ', internalWidth - len - 1) : "") + AnsiEscape.SgrClear);
	    }
	    BaseModule inputModule = Parent.InputModule;
            if (inputModule is null)
            {
                Parent.Flush();
                return;
            }

            int inputCursorY = Math.Min(Console.WindowHeight - 2, inputModule.Height + inputModule.Y - 2);
            int inputCursorX = inputModule.X + 1 + inputModule.lrCursorPos;
            if (inputCursorY >= 0 && inputCursorX >= 0)
            {
                Parent.SetCursorPosition(inputCursorX, inputCursorY);
            }
            Parent.Write(AnsiEscape.EnableCursorVisibility);
	    Parent.Flush();
	    }
	}
	
	private long ToInternalFullX(long pos){
	    long internalWidth = (this.Width - 2) * 2;
	    double rat = pos / (double)max;
	    return (long)(rat * internalWidth);
	}
        protected override IEnumerable<string> ToOutputLines(){
	    System.Text.StringBuilder sb = new();
	    string displayTitle = DisplayName ?? Title;
	    int displayTitleLen = displayTitle.VisibleLength();
	    int internalWidth = Width - 2;
	    long currPos = ToInternalFullX(current);
	    int len = (int)(currPos / 2);
	    yield return sb.Append(AnsiEscape.LineDrawingMode).Append(AnsiEscape.UpperLeftCorner).Append(AnsiEscape.AsciiMode).Append(displayTitleLen > internalWidth ? displayTitle.Ellipse(internalWidth) : displayTitle).Append(AnsiEscape.LineDrawingMode).Append(AnsiEscape.HorizontalLine, Math.Max(0, Width - 2 - displayTitleLen)).Append(AnsiEscape.UpperRightCorner).ToString();
	    for(int i = 0; i < Height - 2; ++i){
		yield return sb.Clear().Append(AnsiEscape.LineDrawingMode).Append(AnsiEscape.VerticalLine).Append(AnsiEscape.AsciiMode).Append(filler).Append(AnsiEscape.SgrBrightBlackBackGround).Append('█',len).Append(len == internalWidth ? "" : fillers[currPos%2].ToString()).Append(AnsiEscape.SgrClear).Append(AnsiEscape.SgrBrightBlackBackGround).Append(len < internalWidth ? new string(' ', internalWidth - len - 1) : "").Append(AnsiEscape.SgrClear).Append(AnsiEscape.LineDrawingMode).Append(AnsiEscape.VerticalLine).ToString();
	    }
	    yield return sb.Clear().Append(AnsiEscape.LineDrawingMode).Append(AnsiEscape.LowerLeftCorner).Append(AnsiEscape.HorizontalLine, internalWidth).Append(AnsiEscape.LowerRightCorner).ToString();

	}
    }
}