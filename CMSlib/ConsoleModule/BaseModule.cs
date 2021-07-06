using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using CMSlib.Tables;
using ExtensionMethods = CMSlib.Tables.ExtensionMethods;
using CMSlib.Extensions;
using System.Threading;
using System.Threading.Tasks;
namespace CMSlib.ConsoleModule
{
    public abstract class BaseModule : ILogger
    {
        public int X { get; protected init;}
        public int Y { get; protected init;}
        public int Width { get; protected init;}
        public int Height { get; protected init;}
        public string Title { get; protected init; }
        
        /// <summary>
        /// This string is shown at the top of the module. Setting it to null, or not setting it at all, uses the module title as the displayed title.
        /// </summary>
        public string DisplayName { get; set; } = null;
        
        internal  int      scrolledLines = 0;
        internal  bool     unread = false;
        internal  int      lrCursorPos = 0;
        protected readonly LogLevel minLevel;
        internal ModuleManager parent = null;
        protected readonly object AddTextLock = new();
        internal bool selected = false;

        protected BaseModule()
        {
        }

        protected BaseModule(string title, int x, int y, int width, int height, LogLevel minLevel)
        {
            this.minLevel = minLevel;
            this.Title = title;
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public abstract override string ToString();
        public abstract void AddText(string text);
        public abstract void ScrollUp(int amt);
        public void ScrollDown(int amt) => ScrollUp(-amt);
        public abstract void ScrollTo(int line);
        /// <summary>
        /// Clears all lines from this module, as well as optionally refreshing.
        /// </summary>
        /// <param name="refresh">Whether to refresh after clearing out the text</param>
        public abstract void Clear(bool refresh = true);
        public void AddText(object obj) => AddText(obj.ToString());
        
        /// <summary>
        /// Event fired when a line is entered into this module
        /// </summary>
        public event AsyncEventHandler<LineEnteredEventArgs> LineEntered;
        /// <summary>
        /// Event fired when a key is pressed while this module is focused
        /// </summary>
        public event AsyncEventHandler<KeyEnteredEventArgs> KeyEntered;
        
        internal async Task FireLineEnteredAsync(LineEnteredEventArgs args)
        {
            var handler = LineEntered;
            if (handler is not null)
            {
                await handler(this, args);
            }
        }

        internal async Task FireKeyEnteredAsync(KeyEnteredEventArgs args)
        {
            var handler = KeyEntered;
            if (handler is not null)
            {
                await handler(this, args);
            }
        }

        
        
        
        /// <summary>
        /// Logs a message to this module.
        /// </summary>
        /// <param name="logLevel">The level to log this at. If this log level is not at least the minimum, this message won't show.</param>
        /// <param name="eventId">The event id of the event being logged.</param>
        /// <param name="state">The state to log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="formatter">The formatter to format the log message.</param>
        /// <typeparam name="TState">The type of the state</typeparam>
        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!this.ShouldLog(logLevel))
                return;
            DateTime time = DateTime.Now;
            bool willWrite = (!unread && (scrolledLines != 0)) || scrolledLines == 0;
            StringBuilder output = new StringBuilder();
            string[] shortNames = {"TRC", "DBG", "INF", "WRN", "ERR", "CRT"};
            int logLevelInt = (int) logLevel;
            string shortName = logLevelInt >= 0 && logLevelInt < shortNames.Length ? shortNames[(int) logLevel] : "???";
            string colorScheme =

                logLevel switch
                {
                    LogLevel.Trace => $"{AnsiEscape.SgrCyanForeGround}",
                    LogLevel.Information => $"{AnsiEscape.SgrCyanForeGround}{AnsiEscape.SgrBrightBold}",
                    LogLevel.Debug => $"{AnsiEscape.SgrMagentaForeGround}{AnsiEscape.SgrBrightBold}",
                    LogLevel.Warning => $"{AnsiEscape.SgrYellowForeGround}{AnsiEscape.SgrBrightBold}",
                    LogLevel.Error => $"{AnsiEscape.SgrRedForeGround}{AnsiEscape.SgrBrightBold}",
                    LogLevel.Critical =>
                        $"{AnsiEscape.SgrWhiteBackGround}{AnsiEscape.SgrRedForeGround}{AnsiEscape.SgrBrightBold}{AnsiEscape.SgrNegative}",
                    _ => ""
                };
            output.Append($"{colorScheme}");
            output.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            output.Append($"|{eventId.Id.ToString().TableColumn(5, ExtensionMethods.ColumnAdjust.Right)}:{shortName}|{eventId.Name.GuaranteeLength(Width - 30)}{AnsiEscape.SgrClear}");
            lock (AddTextLock)
            {
                this.AddText(output
                    .ToString()); //.PadToVisibleDivisible(width) + $"{colorScheme}[{eventId.Id.ToString().GuaranteeLength(5)}|{eventId.Name.GuaranteeLength(18)}]{AnsiEscape.SgrClear}" );
                string formattedMessage = formatter(state, exception);
                if (formattedMessage is not null)
                    this.AddText(formatter(state, exception));
                if (exception is not null)
                {
                    this.AddText(
                        $"Exception in {exception.Source}: {exception.Message} at {exception.TargetSite?.Name ?? "unknown method"}");
                }
            }

            if (willWrite)
            {
                this.WriteOutput();
            }
            
        }
        bool ILogger.IsEnabled(LogLevel logLevel) =>
            ShouldLog(logLevel);

        private bool ShouldLog(LogLevel logLevel) =>
            logLevel >= minLevel;
        
        /// <summary>
        /// NOT IMPL'D
        /// </summary>
        /// <param name="state"></param>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Refreshes this module, showing the latest output.
        /// </summary>
        public void WriteOutput()
        {
            if (this.parent is null)
                return;
            lock (this.parent.writeLock)
            {
                Console.Write(AnsiEscape.DisableCursorVisibility);
                var outputLines = this.ToOutputLines();
                int i = Y - 1;
                if (this.X > Console.BufferWidth || this.Y > Console.BufferHeight)
                    return;
                Console.SetCursorPosition(X, Y);
                foreach (var line in outputLines)
                {
                    if (++i >= Console.WindowHeight)
                        break;
                    if (line.IsVisible())
                        Console.SetCursorPosition(X, i);
                    Console.Write(line);
                }

                BaseModule inputModule = this.parent?.InputModule;
                if (inputModule is null) return;

                int inputCursorY = Math.Min(Console.WindowHeight - 2, inputModule.Height + inputModule.Y);
                int inputCursorX = inputModule.X + 1 + inputModule.lrCursorPos;
                if (inputCursorY < 0 || inputCursorX < 0)
                    return;
                Console.SetCursorPosition(inputCursorX,
                    inputCursorY);
                Console.Write(AnsiEscape.EnableCursorVisibility);
            }
        }
        private IEnumerable<string> ToOutputLines()
        {
            return ToString().SplitOnNonEscapeLength(Width + 2);
        }
    }
}