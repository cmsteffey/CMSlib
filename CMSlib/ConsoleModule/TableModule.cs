using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMSlib.Tables;
using Microsoft.Extensions.Logging;

namespace CMSlib.ConsoleModule
{
    public class ListModule : BaseModule
    {
        private string[] lineCache;
        private Table wrapped;
        private string header;
        public ListModule(string title, int x, int y, int width, int height, Table toWrap, string header = null, 
            LogLevel logLevel = LogLevel.Information) : base(title, x, y, width, height, logLevel)
        {
            wrapped = toWrap;
            this.header = header ?? toWrap.GetHeader();
            lineCache = toWrap.GetOutputRows().ToArray();
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

        internal override async Task HandleClickAsync(InputRecord record, ButtonState? before)
        {
            
        }

        internal override async Task HandleKeyAsync(ConsoleKeyInfo info)
        {
            
        }

        protected override IEnumerable<string> ToOutputLines()
        {
            yield break;
        }
    }
}