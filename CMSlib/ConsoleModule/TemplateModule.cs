using System;
using System.Threading.Tasks;

namespace CMSlib.ConsoleModule
{
    /*MAKE PUBLIC, CHANGE CLASS NAME*/ internal class TemplateModule : BaseModule
    {
        public override void AddText(string text){}
        public override void Clear(bool refresh = true){}
        public override void PageDown(){}
        public override void PageUp(){}
        public override void ScrollUp(int amt){}
        public override void ScrollTo(int line){}
	/*CHANGE CLASS NAME*/
        public TemplateModule(string title, int x, int y, int width, int height): base(title, x, y, width, height, LogLevel.None){}
        internal async override Task HandleClickAsync(InputRecord record, ButtonState? before){}
        internal async override Task HandleKeyAsync(ConsoleKeyInfo info){}
        protected override IEnumerable<string> ToOutputLines(){yield break;}
    }
}