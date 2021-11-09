using System;
using System.Threading;
using System.Threading.Tasks;
using static CMSlib.ConsoleModule.AnsiEscape;
using CMSlib.Extensions;
using System.Linq;

namespace CMSlib.ConsoleModule
{
    public class ToastNotificationModule : BaseModule
    {
        System.Collections.Generic.List<ToastNotification> notifications = new();
        ToastNotification current;
        object currentLock = new();
        System.Timers.Timer toastShower;
        TimeSpan toastLifetime;
        string emptyString;

        public override void AddText(string text)
        {
        }

        public override void Clear(bool refresh = true)
        {
        }

        public override void PageDown()
        {
        }

        public override void PageUp()
        {
        }

        public override void ScrollUp(int amt)
        {
        }

        public override void ScrollTo(int line)
        {
        }


        public ToastNotificationModule(string title, int x, int y, int width, int height, TimeSpan toastLifetime,
            TimeSpan? checkSends = null) : base(title, x,
            y, width, height, Microsoft.Extensions.Logging.LogLevel.None)
        {
            checkSends ??= TimeSpan.FromSeconds(1);
            this.toastLifetime = toastLifetime;
            toastShower = new(checkSends.Value.TotalMilliseconds);
            toastShower.Elapsed += CheckToasts;
            toastShower.AutoReset = true;
            toastShower.Start();
        }

        public bool CheckToasts()
        {
            return CheckToasts(toastShower, System.DateTime.Now);
        }

        private bool CheckToasts(object sender, System.DateTime signalTime)
        {
            if (current is not null && signalTime - current.SendAt < toastLifetime) return false;

            ToastNotification toShow = notifications.FirstOrDefault(x => x.SendAt < signalTime);

            if (toShow is null && current is null) return false;

            _ = System.Threading.Tasks.Task.Run(() =>
            {
                if (toShow is not null) notifications.Remove(toShow);

                lock (currentLock) current = toShow;
                WriteOutput();

                if (toShow is not null) System.Threading.Thread.Sleep((int) toastLifetime.TotalMilliseconds);

                lock (currentLock) current = notifications.FirstOrDefault(x => x.SendAt < DateTime.Now);
                WriteOutput();
            });
            return true;
        }

        private void CheckToasts(object sender, System.Timers.ElapsedEventArgs e)
        {
            CheckToasts(sender, e.SignalTime);
        }

        internal async override Task HandleClickAsync(InputRecord record, ButtonState? before)
        {
        }

        internal async override Task HandleKeyAsync(ConsoleKeyInfo info)
        {
        }

        public void SendNotification(string title, string text, DateTime? sendAt = null)
        {
            sendAt ??= DateTime.Now;
            notifications.Add(new(title, text, sendAt.Value));
        }

        protected override System.Collections.Generic.IEnumerable<string> ToOutputLines()
        {
            int innerWidth = Math.Min(Console.WindowWidth - X, Width) - 2;
            int innerHeight = Math.Min(Console.WindowHeight - Y, Height) - 2;

            if (innerHeight < 0) yield break;

            if (current is not ToastNotification validCurrent)
            {
                emptyString ??= new string(' ', base.Width);
                for (int i = 0; i < this.Height; ++i)
                    yield return emptyString;
                yield break;
            }

            System.Text.StringBuilder output = new();
            yield return output.Append(LineDrawingMode).Append(UpperLeftCorner).Append(HorizontalLine, innerWidth)
                .Append(UpperRightCorner).ToString();
            int titleVisLen = validCurrent.Title.VisibleLength();
            yield return output.Clear().Append(VerticalLine).Append(AsciiMode).Append(SgrUnderline)
                .Append(titleVisLen < innerWidth ? validCurrent.Title : validCurrent.Title.Ellipse(innerWidth))
                .Append(' ', Math.Max(innerWidth - titleVisLen, 0)).Append(SgrNoUnderline).Append(LineDrawingMode)
                .Append(VerticalLine).ToString();
            string[] split = validCurrent.Text.PadToVisibleDivisible(innerWidth).SplitOnNonEscapeLength(innerWidth)
                .ToArray();
            int lineCount = Math.Min(innerHeight - 1, split.Length);
            for (int i = 0; i < lineCount; ++i)
            {
                yield return VerticalLine + AsciiMode + SgrForeGroundColor(246) + split[i] + SgrClear +
                             LineDrawingMode + VerticalLine;
            }

            string emptyLine = VerticalLine + new string(' ', innerWidth) + VerticalLine;
            for (int i = 0; i < (innerHeight - 1) - lineCount; ++i)
            {
                yield return emptyLine;
            }

            yield return output.Clear().Append(LowerLeftCorner).Append(HorizontalLine, innerWidth)
                .Append(LowerRightCorner).Append(AsciiMode).ToString();
        }

        internal record ToastNotification(string Title, string Text, DateTime SendAt);
    }
}