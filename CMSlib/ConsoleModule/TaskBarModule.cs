using System;
using System.Collections.Generic;
using System.Text;
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
            this.parent?.NextPage();
        }
        public override void PageUp()
        {
            this.parent?.PrevPage();
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
                int pages = parent.Pages.Count;
                return Math.Min((Width - pages) / pages, internalWidthPer);
            }
        }
        private readonly int internalWidthPer;
        public TaskBarModule(string title, int x, int y, int width, int height, int internalWidthPer) : base(title, x,
            y, width, height, LogLevel.None)
        {
            this.internalWidthPer = internalWidthPer;
        }

        internal override void HandleClickAsync(InputRecord record)
        {
            List<ModulePage> pages = parent.Pages;
            
            int actingInternalWidthPer = Math.Min((Width - pages.Count) / pages.Count, internalWidthPer);
            int relativeX = record.MouseEvent.MousePosition.X - X;
            if(relativeX < 0 || relativeX % (actingInternalWidthPer + 1) == actingInternalWidthPer) return;
            int target = relativeX / (actingInternalWidthPer + 1);
            if(target < 0 || target >= pages.Count) return;
            this.parent.ToPage(target);
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (parent is null) return string.Empty;

            List<ModulePage> pages = parent.Pages;
            int actingInternalWidthPer = Math.Min((Width - pages.Count) / pages.Count, internalWidthPer);

            for (int i = 0; i < Height; i++)
            {
                int lineWidth = 0;
                for (int j = 0; j < pages.Count; j++)
                {
                    
                    output.Append(parent.selected == j ? AnsiEscape.SgrNegative : null);
                    output.Append(i==0 ? (pages[j].DisplayName ?? (j + 1).ToString()).Ellipse(actingInternalWidthPer)
                        .GuaranteeLength(actingInternalWidthPer) : new string(' ', actingInternalWidthPer));
                    output.Append(AnsiEscape.SgrClear);
                    lineWidth += actingInternalWidthPer;
                    output.Append('|');
                    lineWidth++;
                }

                if (lineWidth < Width)
                    output.Append(new string(' ', Width - lineWidth));

            }

            return output.ToString();
        }
    }
}