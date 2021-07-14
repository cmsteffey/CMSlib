using System;
using System.Collections.Generic;
using System.Text;
using CMSlib.Extensions;
using Microsoft.Extensions.Logging;

namespace CMSlib.ConsoleModule
{
    public class ToggleModule : BaseModule
    {
        private object toggleLock = new();
        private bool _enabled;
        private string enabledText;
        private string disabledText;
        public bool Enabled
        {
            get
            {
                lock (toggleLock) return _enabled;
            }
            set
            {
                lock (toggleLock) _enabled = value;
            }
        }

        public ToggleModule(string title, int x, int y, int width, int height, bool defaultEnabled, string enabledText = "On", string disabledText = "Off") : base(title, x, y, width, height,
            LogLevel.None)
        {
            _enabled = defaultEnabled;
            this.enabledText = enabledText;
            this.disabledText = disabledText;
        }
        public override void AddText(string text)
        {
            
        }

        internal override void HandleClickAsync(InputRecord record, ButtonState? before)
        {
            
            if (!before.HasValue || before.Value == record.MouseEvent.ButtonState) return;
            bool newState;
            lock (toggleLock)
            {
                _enabled = !_enabled;
                newState = _enabled;
            }

            ToggleFlipped?.Invoke(this, new(newState));
            this.WriteOutput();
        }

        public override void ScrollTo(int line)
        {
            
        }

        public override void ScrollUp(int amt)
        {
            
        }

        public override void PageDown()
        {
            
        }

        public override void Clear(bool refresh = true)
        {
            
        }

        public override void PageUp()
        {
            
        }

        public event AsyncEventHandler<ToggleFlippedEventArgs> ToggleFlipped;

        public override string ToString()
        {
            bool enabled = Enabled;
            int internalWidth = Math.Min(Width - 2, Console.WindowWidth - X - 2);
            int internalHeight = Math.Min(Height - 2, Console.WindowHeight - Y - 2);
            if (internalWidth < 2)
                return string.Empty;
            string displayTitle = Title.Ellipse(internalWidth);
            StringBuilder builder = new();
            builder.Append(AnsiEscape.LineDrawingMode);
            builder.Append(AnsiEscape.UpperLeftCorner);
            builder.Append(displayTitle);
            builder.Append(new string(AnsiEscape.HorizontalLine, internalWidth - displayTitle.Length));
            builder.Append(AnsiEscape.UpperRightCorner);
            string displayString = enabled ? enabledText : disabledText;
            if (internalHeight > 0)
            {
                builder.Append(AnsiEscape.VerticalLine);
                builder.Append(AnsiEscape.AsciiMode);
                builder.Append(enabled
                    ? AnsiEscape.SgrGreenForeGround + AnsiEscape.SgrBrightBold + AnsiEscape.SgrNegative + " " + AnsiEscape.SgrClear + AnsiEscape.SgrWhiteBackGround + " "
                    : AnsiEscape.SgrWhiteBackGround + " " + AnsiEscape.SgrRedBackGround  + " ");
                builder.Append(AnsiEscape.SgrClear);
                if (internalWidth != 2)
                {
                    builder.Append(' ');
                    builder.Append(displayString[..Math.Min(internalWidth - 3, displayString.Length)].GuaranteeLength(internalWidth - 3));
                }
                builder.Append(AnsiEscape.LineDrawingMode);
                builder.Append(AnsiEscape.VerticalLine);
            }
            for (int i = 1; i < internalHeight; i++)
            {
                builder.Append(AnsiEscape.VerticalLine);
                builder.Append(AnsiEscape.AsciiMode);
                if ((internalWidth * i - 3) < displayString.Length)
                    builder.Append(
                        displayString[
                                (internalWidth * i - 3)..Math.Min(internalWidth * (i + 1) - 3, displayString.Length)]
                            .GuaranteeLength(internalWidth));
                else
                    builder.Append(new string(' ', internalWidth));
                builder.Append(AnsiEscape.LineDrawingMode);
                builder.Append(AnsiEscape.VerticalLine);
            }
            builder.Append(AnsiEscape.LowerLeftCorner);
            builder.Append(new string(AnsiEscape.HorizontalLine, internalWidth));
            builder.Append(AnsiEscape.LowerRightCorner);
            return builder.ToString();
        }
    }

    public class ToggleFlippedEventArgs : EventArgs
    {
        public bool ToggleState { get; }
        internal ToggleFlippedEventArgs(){}

        internal ToggleFlippedEventArgs(bool toggle)
        {
            ToggleState = toggle;
        }
    }
}