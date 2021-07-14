﻿using System;
using System.Text;
using System.Threading.Tasks;
using CMSlib.Extensions;
using Microsoft.Extensions.Logging;
using static CMSlib.ConsoleModule.AnsiEscape;
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

        private async Task FlipToggleAsync()
        {
            bool newState;
            lock (toggleLock)
            {
                _enabled = !_enabled;
                newState = _enabled;
            }

            var handler = ToggleFlipped;
            if (handler is not null)
                await handler(this, new ToggleFlippedEventArgs(newState));
            this.WriteOutput();
        }
        internal async override Task HandleKeyAsync(ConsoleKeyInfo info)
        {
            if(info.Key == ConsoleKey.Enter)
                await FlipToggleAsync();
                
        }
        
        internal async override Task HandleClickAsync(InputRecord record, ButtonState? before)
        {
            
            if (before.HasValue && before.Value != record.MouseEvent.ButtonState) 
                await FlipToggleAsync();
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
            builder.Append(LineDrawingMode);
            builder.Append(UpperLeftCorner);
            builder.Append(selected ? Underline(displayTitle) : displayTitle);
            builder.Append(new string(HorizontalLine, internalWidth - displayTitle.Length));
            builder.Append(UpperRightCorner);
            string displayString = enabled ? enabledText : disabledText;
            if (internalHeight > 0)
            {
                builder.Append(VerticalLine);
                builder.Append(AsciiMode);
                builder.Append(enabled
                    ? SgrGreenForeGround  +  SgrNegative +   SgrWhiteBackGround + '\u0020' + SgrClear + SgrWhiteBackGround + '\u0020'
                    : SgrWhiteBackGround + '\u0020' + SgrClear + SgrRedForeGround +  SgrNegative + SgrWhiteBackGround + '\u0020');
                builder.Append(SgrClear);
                if (internalWidth != 2)
                {
                    builder.Append(' ');
                    builder.Append(displayString[..Math.Min(internalWidth - 3, displayString.Length)].GuaranteeLength(internalWidth - 3));
                }
                builder.Append(LineDrawingMode);
                builder.Append(VerticalLine);
            }
            for (int i = 1; i < internalHeight; i++)
            {
                builder.Append(VerticalLine);
                builder.Append(AsciiMode);
                if ((internalWidth * i - 3) < displayString.Length)
                    builder.Append(
                        displayString[
                                (internalWidth * i - 3)..Math.Min(internalWidth * (i + 1) - 3, displayString.Length)]
                            .GuaranteeLength(internalWidth));
                else
                    builder.Append(new string(' ', internalWidth));
                builder.Append(LineDrawingMode);
                builder.Append(VerticalLine);
            }
            builder.Append(LowerLeftCorner);
            builder.Append(new string(HorizontalLine, internalWidth));
            builder.Append(LowerRightCorner);
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