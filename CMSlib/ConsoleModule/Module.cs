using System;
using CMSlib.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CMSlib.Tables;
using Microsoft.Extensions.Logging;

namespace CMSlib.ConsoleModule
{
    public class Module : ILogger
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
        public string Title { get; }

        internal StringBuilder inputString = new();
        private List<string> text = new();
        private readonly char? borderCharacter;
        internal readonly bool isInput;
        private ModuleManager parent;
        private object AddTextLock = new();
        private object ScrollLock = new();
        private LogLevel minLevel;
        internal int scrolledLines = 0;
        internal bool unread = false;
        internal int lrCursorPos = 0;
        private string _inputClear;
        
        private string InputClear { get {
            _inputClear ??= "\b \b".Multiply(Width);
            return _inputClear;
        } }
        /// <summary>
        /// This string is shown at the top of the module. Setting it to null, or not setting it at all, uses the module title as the displayed title.
        /// </summary>
        public string DisplayName { get; set; } = null;
        internal bool selected = false;

        /// <summary>
        /// Event fired when a line is entered into this module
        /// </summary>
        public event AsyncEventHandler<LineEnteredEventArgs> LineEntered;
        /// <summary>
        /// Event fired when a key is pressed while this module is focused
        /// </summary>
        public event AsyncEventHandler<KeyEnteredEventArgs> KeyEntered;

        internal Module()
        {
            throw new NotSupportedException("Use the parameterized constructor");
        }

        internal Module(ModuleManager parent, string title, int x, int y, int width, int height, string text,
            bool isInput, char? borderCharacter = null, LogLevel minimumLogLevel = LogLevel.Information)
        {
            (
                    this.parent,
                    this.borderCharacter,
                    this.X,
                    this.Y,
                    this.Height,
                    this.Width,
                    this.isInput,
                    this.minLevel,
                    this.Title) =
                (parent,
                    borderCharacter,
                    x,
                    y,
                    height,
                    width,
                    isInput,
                    minimumLogLevel,
                    title
                );
            this.AddText(text);
        }
        

        public delegate Task AsyncEventHandler<in T>(object sender, T eventArgs);
        /// <summary>
        /// Clears all lines from this module, as well as optionally refreshing.
        /// </summary>
        /// <param name="refresh">Whether to refresh after clearing out the text</param>
        public void Clear(bool refresh = true)
        {
            lock (AddTextLock)
            {
                scrolledLines = 0;
                text.Clear();
            }

            if(refresh)
                WriteOutput();
        }
        /// <summary>
        /// Adds line(s) of text to this module. This supports \n, and \n will properly add text to the next line.
        /// </summary>
        /// <param name="text">The text to add</param>
        public void AddText(string text)
        {
            
            lock (AddTextLock)
            {
                int before = this.text.Count;
                this.text.AddRange(text.Replace("\t", "        ").Replace("\r\n", "\n").Split('\n').SelectMany(x=>x.PadToVisibleDivisible(Width).SplitOnNonEscapeLength(Width)));
                if (scrolledLines != 0)
                {
                    scrolledLines += this.text.Count - before;
                    unread = true;
                }
                
            }
        }

        public void AddText(object obj)
        {
            AddText(obj.ToString());
        }
        /// <summary>
        /// Gets the string representation of this Module.
        /// </summary>
        /// <returns>The string representation of this module.</returns>
        public override string ToString()
        {
            //todo hEIGHT
            int actingHeight = Math.Min(Height, (Console.WindowHeight - 2) - Y);
            string actingTitle = DisplayName ?? Title;
            StringBuilder output = borderCharacter is not null ? new((Width + 2) * (actingHeight + 2) + AnsiEscape.AsciiMode.Length + AnsiEscape.SgrUnderline.Length * 2) : new();
            int inputDifferential = isInput ? 2 : 0;
            int lineCount = Math.Clamp(text.Count - scrolledLines, 0, actingHeight - inputDifferential);
            int spaceCount =
                Math.Min(actingHeight - text.Count - inputDifferential + scrolledLines,
                    actingHeight - inputDifferential);
            if (lineCount <= 0 && spaceCount <= 0)
            {
                return string.Empty;
            }
            if (borderCharacter is null)
                output.Append(AnsiEscape.LineDrawingMode);
            else
                output.Append(AnsiEscape.AsciiMode);
            output.Append(borderCharacter??AnsiEscape.UpperLeftCorner);
            if (borderCharacter is null)
                output.Append(AnsiEscape.AsciiMode);
            
            if (selected)
                output.Append(AnsiEscape.SgrUnderline);
            output.Append(actingTitle.Ellipse(Width));
            
            if (selected)
                output.Append(AnsiEscape.SgrNoUnderline);
            if (borderCharacter is null)
                output.Append(AnsiEscape.LineDrawingMode);
            output.Append(borderCharacter??AnsiEscape.HorizontalLine, Width - actingTitle.Ellipse(Width).Length);
            output.Append(borderCharacter ?? AnsiEscape.UpperRightCorner);
            for (int i = 0; i < spaceCount; i++)
            {
                output.Append(borderCharacter?.ToString()??(unread && i > actingHeight - (4 + inputDifferential) ? AnsiEscape.AsciiMode + AnsiEscape.SgrRedForeGround + AnsiEscape.SgrBrightBold + "V" + AnsiEscape.SgrClear: AnsiEscape.VerticalLine.ToString()));
                output.Append(' ', Width);
                output.Append(borderCharacter?.ToString()??AnsiEscape.LineDrawingMode + AnsiEscape.VerticalLine);
            }
            int index = Math.Clamp(text.Count - (actingHeight - inputDifferential) - scrolledLines, 0, text.Count == 0 ? 0 : text.Count - 1);
            
            List<string> toPrint = text.GetRange(index, lineCount);
            
            
            for(int i = 0; i < toPrint.Count; i++)
            {
                output.Append(borderCharacter?.ToString()??(unread && i + Math.Max(spaceCount, 0) > actingHeight - (4 + inputDifferential) ? AnsiEscape.SgrRedForeGround + AnsiEscape.SgrBrightBold + "V" + AnsiEscape.SgrClear: AnsiEscape.VerticalLine.ToString()));
                if (borderCharacter is null) output.Append(AnsiEscape.AsciiMode);
                output.Append(toPrint[i]);
                bool dot = borderCharacter is null && i + Math.Max(spaceCount, 0) > Height - (2 + inputDifferential) && scrolledLines != 0;
                if (dot)
                {
                    output.Append(AnsiEscape.SgrGreenForeGround + AnsiEscape.SgrBrightBold + "." + AnsiEscape.SgrClear);
                }
                if (borderCharacter is null) output.Append(AnsiEscape.LineDrawingMode);
                output.Append(borderCharacter?.ToString()??(dot?"":AnsiEscape.VerticalLine.ToString()));
            }
            if(borderCharacter is null)
                output.Append(isInput ? AnsiEscape.VerticalWithRight : AnsiEscape.LowerLeftCorner).Append(AnsiEscape.HorizontalLine, Width).Append(isInput ? AnsiEscape.VerticalWithLeft : AnsiEscape.LowerRightCorner);
            else
                output.Append(borderCharacter.Value, Width + 2);
            if (!isInput) return output.ToString();
            if(borderCharacter is null) 
                output
                    .Append(AnsiEscape.VerticalLine)
                    .Append(AnsiEscape.AsciiMode)
                    .Append(inputString)
                    .Append(' ', Width - inputString.Length)
                    .Append(AnsiEscape.LineDrawingMode)
                    .Append(AnsiEscape.VerticalLine)
                    .Append(AnsiEscape.LowerLeftCorner)
                    .Append(AnsiEscape.HorizontalLine, Width)
                    .Append(AnsiEscape.LowerRightCorner)
                    .Append(AnsiEscape.AsciiMode);
            else
                output.Append(borderCharacter).Append(inputString).Append(' ', Width - inputString.Length).Append(borderCharacter.Value, Width + 3);
            return output.ToString();
        }
    
        private IEnumerable<string> ToOutputLines()
        {
            return ToString().SplitOnNonEscapeLength(Width + 2);
        }
        /// <summary>
        /// Refreshes this module, showing the latest output.
        /// </summary>
        public void WriteOutput()
        {
            lock (this.parent.writeLock)
            {
                Console.Write(AnsiEscape.DisableCursorVisibility);
                var outputLines = this.ToOutputLines();
                int i = Y - 1;
                if(this.X > Console.BufferWidth || this.Y > Console.BufferHeight)
                    return;
                Console.SetCursorPosition(X, Y);
                foreach (var line in outputLines)
                {
                    if(++i >= Console.WindowHeight)
                        break;
                    if (line.IsVisible())
                        Console.SetCursorPosition(X, i);
                    Console.Write(line);
                }

                Module inputModule = this.parent.InputModule;
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

        internal Module ToInputModule()
        {
            return isInput ? this : null;
        }
        
        public bool IsEnabled(LogLevel logLevel) =>
            logLevel >= minLevel;
        /// <summary>
        /// Logs a message to this module.
        /// </summary>
        /// <param name="logLevel">The level to log this at. If this log level is not at least the minimum, this message won't show.</param>
        /// <param name="eventId">The event id of the event being logged.</param>
        /// <param name="state">The state to log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="formatter">The formatter to format the log message.</param>
        /// <typeparam name="TState">The type of the state</typeparam>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
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
        

        internal void ScrollUp(int amt)
        {
            if (this.text.Count == 0) return;
            lock (AddTextLock)
            {
                int before = scrolledLines;
                scrolledLines = Math.Clamp(scrolledLines + amt, 0, this.text.Count - 1);
                if (before != scrolledLines) WriteOutput();
            }
        }

        internal void ScrollDown(int amt)
        {
            if (this.text.Count == 0) return;
            lock(AddTextLock){
                int before = scrolledLines;
                scrolledLines = Math.Clamp(scrolledLines - amt, 0, this.text.Count - 1);
                if (scrolledLines == 0) unread = false;
                if(before != scrolledLines) WriteOutput();
            }
        }
        internal void ScrollTo(int line)
        {
            if (this.text.Count == 0) return;
            lock (AddTextLock)
            {
                int before = scrolledLines;
                scrolledLines = Math.Clamp(line, 0, this.text.Count - 1);
                if (before != scrolledLines) WriteOutput();
            }
        }

        internal void FireLineEntered(LineEnteredEventArgs args)
        {
            var handler = LineEntered;
            if (handler is not null)
            {
                handler(this, args);
            }
        }
        internal void FireKeyEntered(KeyEnteredEventArgs args)
        {
            var handler = KeyEntered;
            if (handler is not null)
            {
                handler(this, args);
            }
        }
    }

    /// <summary>
    /// Configures the Windows console to enable Ansi. Uses native WINAPI functions, don't call this without ensuring you're on a windows OS first.
    /// </summary>
    public class WinConsoleConfiguerer
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
        
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        
        public void SetupConsole()
        {
            IntPtr outputHandle = GetStdHandle(-11);
            IntPtr inputHandle = GetStdHandle(-10);
            GetConsoleMode(outputHandle, out uint outmode);
            GetConsoleMode(inputHandle, out uint inMode);
            outmode |= 4;
            SetConsoleMode(outputHandle, outmode);
            inMode = (uint)(inMode & ~64);
            SetConsoleMode(inputHandle, inMode);
        }
    }
}

