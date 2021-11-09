using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CMSlib.Extensions;
using Microsoft.VisualBasic;
using static CMSlib.ConsoleModule.AnsiEscape;

namespace CMSlib.ConsoleModule
{
    public class ButtonModule : BaseModule
    {
        private string emptyLine;
        private string buttonText;

        public ButtonModule(string title, int x, int y, int width, int height, string buttonText = "") : base(title, x,
            y, width, height,
            LogLevel.None)
        {
            this.buttonText = buttonText;
            emptyLine = string.Concat(LineDrawingMode, VerticalLine, AsciiMode, new string(' ', width - 2),
                LineDrawingMode, VerticalLine);
        }

        public override void AddText(string text)
        {
        }

        public override void ScrollUp(int amt)
        {
        }

        public override void ScrollTo(int line)
        {
        }

        public override void PageUp()
        {
        }

        public override void PageDown()
        {
        }

        public override void Clear(bool refresh = true)
        {
            if (refresh)
                this.WriteOutput();
        }

        internal async override Task HandleClickAsync(InputRecord record, ButtonState? before)
        {
            if (before.HasValue && before.Value != record.MouseEvent.ButtonState)
                HandleClick();
        }

        private void HandleClick()
        {
            var eventHandler = Clicked;
            if (eventHandler is not null)
                eventHandler(this, new ClickEventArgs());
        }

        public event EventHandler<ClickEventArgs> Clicked;

        internal async override Task HandleKeyAsync(ConsoleKeyInfo info)
        {
            if (info.Key is ConsoleKey.Enter)
                HandleClick();
        }

        protected override IEnumerable<string> ToOutputLines()
        {
            int internalWidth = Math.Min(Width - 2, Console.WindowWidth - X - 2);
            int internalHeight = Math.Min(Height - 2, Console.WindowHeight - Y - 2);
            if (internalWidth < 0)
                yield break;
            string displayTitle = (DisplayName ?? Title).Ellipse(internalWidth);
            StringBuilder builder = new();
            yield return builder.Append(LineDrawingMode)
                .Append(UpperLeftCorner)
                .Append(selected
                    ? AsciiMode + Underline(displayTitle) + LineDrawingMode
                    : AsciiMode + displayTitle + LineDrawingMode)
                .Append(HorizontalLine, internalWidth - displayTitle.Length)
                .Append(UpperRightCorner).ToString();
            if (buttonText.VisibleLength() <= internalWidth)
                yield return builder.Clear().Append(VerticalLine).Append(AsciiMode)
                    .Append(buttonText.PadToVisibleDivisible(internalWidth)).Append(LineDrawingMode)
                    .Append(VerticalLine).ToString();
            else
            {
                var splitInner = buttonText.SplitOnNonEscapeLength(internalWidth).ToArray();
                int count = splitInner.Length;
                for (int i = 0; i < internalHeight; i++)
                {
                    if (i < splitInner.Length)
                        yield return builder.Clear().Append(VerticalLine).Append(AsciiMode).Append(splitInner[i])
                            .Append(new string(' ', internalWidth - splitInner[i].VisibleLength()))
                            .Append(LineDrawingMode).Append(VerticalLine).ToString();
                    else
                        yield return emptyLine;
                }
            }

            yield return builder.Clear().Append(LowerLeftCorner).Append(HorizontalLine, internalWidth)
                .Append(LowerRightCorner).ToString();
        }
    }

    public class ClickEventArgs : EventArgs
    {
    }
}