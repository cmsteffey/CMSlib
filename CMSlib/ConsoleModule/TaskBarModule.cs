using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CMSlib.Extensions;
using Microsoft.Extensions.Logging;

namespace CMSlib.ConsoleModule
{
    public class TaskBarModule : BaseModule
    {
        public override void AddText(string text)
        {
            
        }

        public override void Clear(bool refresh = true)
        {
            
        }

        public override void PageDown()
        {
            this.Parent?.NextPage();
        }
        public override void PageUp()
        {
            this.Parent?.PrevPage();
        }

        public override void ScrollUp(int amt)
        {
            switch (amt)
            {
                case > 0:
                    PageUp();
                    break;
                case < 0:
                    PageDown();
                    break;
            }
        }

        public override void ScrollTo(int line)
        {
            
        }

        private int TabWidth
        {
            get
            {
                int pages = Parent.Pages.Count;
                return Math.Min((Width - pages) / pages, internalWidthPer);
            }
        }
        private readonly int internalWidthPer;
        public TaskBarModule(string title, int x, int y, int width, int height, int internalWidthPer) : base(title, x,
            y, width, height, LogLevel.None)
        {
            this.internalWidthPer = internalWidthPer;
        }

        internal async override Task HandleClickAsync(InputRecord record, ButtonState? before)
        {
            if (!before.HasValue || before.Value == record.MouseEvent.ButtonState) return;
            
            List<ModulePage> pages = Parent.Pages;
            
            int actingInternalWidthPer = Math.Min((Width - pages.Count) / pages.Count, internalWidthPer);
            int relativeX = record.MouseEvent.MousePosition.X - X;
            if(relativeX < 0 || relativeX % (actingInternalWidthPer + 1) == actingInternalWidthPer) return;
            int target = relativeX / (actingInternalWidthPer + 1);
            if(target < 0 || target >= pages.Count) return;
            this.Parent.ToPage(target);
        }

        internal async override Task HandleKeyAsync(ConsoleKeyInfo info)
        {
            switch(info.Key){
		case ConsoleKey.RightArrow:
		    PageDown()
		    break;
		case ConsoleKey.LeftArrow:
		    PageUp();
		    break;
	    }
        }

        protected override IEnumerable<string> ToOutputLines()
        {
            StringBuilder output = new StringBuilder();
            if (Parent is null) yield break;
            if (Y >= Console.WindowHeight) yield break;
            List<ModulePage> pages = Parent.Pages;
            int actingInternalWidthPer = Math.Min((Width - pages.Count) / pages.Count, internalWidthPer);
            
            for (int i = 0; i < Math.Min(Height, Console.WindowHeight - Y); i++)
            {
                output.Clear().Append(AnsiEscape.AsciiMode);
                int lineWidth = 0;
                for (int j = 0; j < pages.Count; j++)
                {
                    
                    output.Append(Parent.selected == j ? (this.selected ? AnsiEscape.SgrBlackForeGround + AnsiEscape.SgrWhiteBackGround : AnsiEscape.SgrBlackForeGround + AnsiEscape.SgrBlackBackGround + AnsiEscape.SgrBrightBold + AnsiEscape.SgrNegative) : null);
                    output.Append(i==0 ? (pages[j].DisplayName ?? (j + 1).ToString()).Ellipse(actingInternalWidthPer)
                        .GuaranteeLength(actingInternalWidthPer) : new string(' ', actingInternalWidthPer));
                    output.Append(AnsiEscape.SgrClear);
                    lineWidth += actingInternalWidthPer;
                    output.Append(AnsiEscape.LineDrawingMode).Append(AnsiEscape.VerticalLine).Append(AnsiEscape.AsciiMode);
                    lineWidth++;
                }

                if (lineWidth < Width)
                    output.Append(' ', Width - lineWidth);
                yield return output.ToString();
            }
        }
    }
}