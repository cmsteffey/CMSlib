using System;
using CMSlib.Extensions;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CMSlib.Tables;
using Microsoft.Extensions.Logging;

namespace CMSlib.ConsoleModule
{
    public class Module : ILogger
    {
        internal StringBuilder inputString = new();
        private List<string> text = new();
        private readonly char? borderCharacter;
        internal int x, y, width, height;
        private bool isInput;
        private ModuleManager parent;
        private object AddTextLock = new();
        private object ScrollLock = new();
        private LogLevel minLevel;
        private int scrolledLines = 0;
        private bool unread = false;
        private int lrCursorPos = 0;
        private string title;
        private string _inputClear;
        private string InputClear { get {
            _inputClear ??= "\b \b".Multiply(width);
            return _inputClear;
        } }
        public string DisplayName { get; set; } = null;
        internal bool selected = false;

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
                    this.x,
                    this.y,
                    this.height,
                    this.width,
                    this.isInput,
                    this.minLevel,
                    this.title) =
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
            if (!isInput) return;
            Console.TreatControlCAsInput = true;
            Console.CancelKeyPress += (_, _) => { QuitApp(); };
            if (Environment.OSVersion.Platform.ToString().ToLower().Contains("win"))
                new WinConsoleConfiguerer().SetupConsole();
            Console.Write(AnsiEscape.AlternateScreenBuffer);
            Console.Write(AnsiEscape.DisableCursorBlink);

            _ = Task.Run(async () =>
            {
                bool closed = false;
                bool highlightMode = false;
                while (true)
                {
                    var key = Console.ReadKey(true);
                    var keyHandler = this.KeyEntered;
                    if (keyHandler is not null)
                        await keyHandler(this, new(key));
                    if (key.Modifiers.HasFlag(ConsoleModifiers.Alt))
                        continue;

                    if (key.Key == ConsoleKey.Escape)
                    {
                        closed = true;
                        lock (this.parent.writeLock)
                        {
                            Console.CursorVisible = false;
                            this.inputString.Clear();
                            this.inputString.Append(new string('%', width));
                            lrCursorPos = width;
                            this.parent.RefreshModule(parent.dictKeys[0]);
                        }

                        continue;
                    }

                    if (closed)
                    {
                        closed = false;

                        lock (this.parent.writeLock)
                        {
                            this.inputString.Clear();
                            lrCursorPos = 0;
                            Console.Write(InputClear);
                        }
                    }
                    lock(this.parent.writeLock)
                        Console.Write(AnsiEscape.EnableCursorVisibility);

                    switch (key.Key)
                    {
                        case ConsoleKey.RightArrow:
                            break;
                        case ConsoleKey.LeftArrow:
                            break;
                        case ConsoleKey.PageUp:
                            this.parent.SelectedModule?.ScrollUp((height - (isInput ? 2 : 0)));
                            break;
                        case ConsoleKey.PageDown:
                            this.parent.SelectedModule?.ScrollDown((height - (isInput ? 2 : 0)));
                            break;
                        case ConsoleKey.Tab:
                            this.parent.SelectNext();

                            break;
                        case ConsoleKey.C when key.Modifiers.HasFlag(ConsoleModifiers.Control):
                            QuitApp();
                            break;
                        case ConsoleKey.Enter:

                            string line;
                            AsyncEventHandler<LineEnteredEventArgs> handler;
                            lock (this.parent.writeLock)
                            {
                                handler = LineEntered;
                                line = inputString.ToString();
                                inputString.Clear();
                                lrCursorPos = 0;
                                scrolledLines = 0;
                                unread = false;
                            }
                            
                            if (handler != null)
                            {
                                var e = new LineEnteredEventArgs(line);
                                await handler(this, e);
                            }
                            this.WriteOutput();


                            continue;
                        case ConsoleKey.Backspace when inputString.Length == 0:
                            continue;
                        case ConsoleKey.Backspace when key.Modifiers.HasFlag(ConsoleModifiers.Control):
                            bool? isPrevSpace = inputString[^1] == ' ';
                            int i;
                            for (i = inputString.Length - 2; i >= 0; i--)
                            {

                                if (isPrevSpace.Value && inputString[i] != ' ')
                                    break;
                                if (inputString[i] == ' ' && !isPrevSpace.Value)
                                    isPrevSpace = true;
                            }

                            //TODO fix this
                            break;
                        case ConsoleKey.Backspace:
                            lock (this.parent.writeLock)
                            {
                                inputString.Remove(inputString.Length - 1, 1);
                                lrCursorPos--;
                                Console.Write("\b \b");
                            }

                            continue;
                        default:
                            if (key.KeyChar == '\u0000') continue;
                            if (inputString.Length < width)
                            {
                                lock (this.parent.writeLock)
                                {
                                    inputString.Append(key.KeyChar);
                                    Console.Write(key.KeyChar);
                                    lrCursorPos++;
                                }
                            }
                            

                            break;
                    }
                }
            });
        }

        internal event AsyncEventHandler<LineEnteredEventArgs> LineEntered;
        internal event AsyncEventHandler<KeyEnteredEventArgs> KeyEntered;

        public delegate Task AsyncEventHandler<in T>(object sender, T eventArgs);

        public void Clear(bool refresh = true)
        {
            lock(AddTextLock)
                text.Clear();
            if(refresh)
                WriteOutput();
        }

        public void AddText(string text)
        {
            
            lock (AddTextLock)
            {
                int before = this.text.Count;
                this.text.AddRange(text.Split('\n').SelectMany(x=>x.PadToVisibleDivisible(width).SplitOnNonEscapeLength(width)));
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

        public override string ToString()
        {
            //todo hEIGHT
            int actingHeight = Math.Min(height, (Console.WindowHeight - 2) - y);
            string actingTitle = DisplayName ?? title;
            StringBuilder output = borderCharacter is not null ? new((width + 2) * (actingHeight + 2) + AnsiEscape.AsciiMode.Length + AnsiEscape.SgrUnderline.Length * 2) : new();
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
            output.Append(actingTitle.ToUpper().Ellipse(width));
            
            if (selected)
                output.Append(AnsiEscape.SgrNoUnderline);
            if (borderCharacter is null)
                output.Append(AnsiEscape.LineDrawingMode);
            output.Append(borderCharacter??AnsiEscape.HorizontalLine, width - actingTitle.Ellipse(width).Length);
            output.Append(borderCharacter ?? AnsiEscape.UpperRightCorner);
            for (int i = 0; i < spaceCount; i++)
            {
                output.Append(borderCharacter?.ToString()??(unread && i > actingHeight - (4 + inputDifferential) ? AnsiEscape.AsciiMode + AnsiEscape.SgrRedForeGround + AnsiEscape.SgrBrightBold + "V" + AnsiEscape.SgrClear: AnsiEscape.VerticalLine.ToString()));
                output.Append(' ', width);
                output.Append(borderCharacter?.ToString()??AnsiEscape.LineDrawingMode + AnsiEscape.VerticalLine);
            }
            int index = Math.Clamp(text.Count - (actingHeight - inputDifferential) - scrolledLines, 0, text.Count == 0 ? 0 : text.Count - 1);
            
            List<string> toPrint = text.GetRange(index, lineCount);
            
            
            for(int i = 0; i < toPrint.Count; i++)
            {
                output.Append(borderCharacter?.ToString()??(unread && i + Math.Max(spaceCount, 0) > actingHeight - (4 + inputDifferential) ? AnsiEscape.SgrRedForeGround + AnsiEscape.SgrBrightBold + "V" + AnsiEscape.SgrClear: AnsiEscape.VerticalLine.ToString()));
                if (borderCharacter is null) output.Append(AnsiEscape.AsciiMode);
                output.Append(toPrint[i]);
                bool dot = borderCharacter is null && i + Math.Max(spaceCount, 0) > height - (2 + inputDifferential) && scrolledLines != 0;
                if (dot)
                {
                    output.Append(AnsiEscape.SgrGreenForeGround + AnsiEscape.SgrBrightBold + "." + AnsiEscape.SgrClear);
                }
                if (borderCharacter is null) output.Append(AnsiEscape.LineDrawingMode);
                output.Append(borderCharacter?.ToString()??(dot?"":AnsiEscape.VerticalLine.ToString()));
            }
            if(borderCharacter is null)
                output.Append(isInput ? AnsiEscape.VerticalWithRight : AnsiEscape.LowerLeftCorner).Append(AnsiEscape.HorizontalLine, width).Append(isInput ? AnsiEscape.VerticalWithLeft : AnsiEscape.LowerRightCorner);
            else
                output.Append(borderCharacter.Value, width + 2);
            if (!isInput) return output.ToString();
            if(borderCharacter is null) 
                output
                    .Append(AnsiEscape.VerticalLine)
                    .Append(AnsiEscape.AsciiMode)
                    .Append(inputString)
                    .Append(' ', width - inputString.Length)
                    .Append(AnsiEscape.LineDrawingMode)
                    .Append(AnsiEscape.VerticalLine)
                    .Append(AnsiEscape.LowerLeftCorner)
                    .Append(AnsiEscape.HorizontalLine, width)
                    .Append(AnsiEscape.LowerRightCorner)
                    .Append(AnsiEscape.AsciiMode);
            else
                output.Append(borderCharacter).Append(inputString).Append(' ', width - inputString.Length).Append(borderCharacter.Value, width + 3);
            return output.ToString();
        }

        public IEnumerable<string> ToOutputLines()
        {
            return ToString().SplitOnNonEscapeLength(width + 2);
        }

        public void WriteOutput()
        {
            lock (this.parent.writeLock)
            {
                Console.Write(AnsiEscape.DisableCursorVisibility);
                var outputLines = this.ToOutputLines();
                int i = y - 1;
                if(this.x > Console.BufferWidth || this.y > Console.BufferHeight)
                    return;
                Console.SetCursorPosition(x, y);
                foreach (var line in outputLines)
                {
                    if(++i >= Console.WindowHeight)
                        break;
                    if (line.IsVisible())
                        Console.SetCursorPosition(x, i);
                    Console.Write(line);
                }

                Console.Write(AnsiEscape.EnableCursorVisibility);
                int inputCursorY = Math.Min(Console.WindowHeight - 2, this.parent.InputModule.height + this.parent.InputModule.y);
                int inputCursorX = this.parent.InputModule.x + 1 + this.parent.InputModule.lrCursorPos;
                if (inputCursorY < 0 || inputCursorX < 0)
                    return;
                Console.SetCursorPosition(inputCursorX,
                    inputCursorY);
            }
        }
        
        public bool IsEnabled(LogLevel logLevel) =>
            logLevel >= minLevel;

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
            output.Append($"|{eventId.Id.ToString().TableColumn(5, ExtensionMethods.ColumnAdjust.Right)}:{shortName}|{eventId.Name.GuaranteeLength(width - 30)}{AnsiEscape.SgrClear}");
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

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public static void QuitApp()
        {
            Console.Write(AnsiEscape.MainScreenBuffer);
            Console.Write(AnsiEscape.SoftReset);
            Console.Write(AnsiEscape.EnableCursorBlink);
            Environment.Exit(0);
        }

        private void ScrollUp(int amt)
        {
            if (this.text.Count == 0) return;
            int before = scrolledLines;
            scrolledLines = Math.Clamp(scrolledLines + amt, 0, this.text.Count - 1);
            if(before != scrolledLines) WriteOutput();
        }

        private void ScrollDown(int amt)
        {
            if (this.text.Count == 0) return;
            int before = scrolledLines;
            scrolledLines = Math.Clamp(scrolledLines - amt, 0, this.text.Count - 1);
            if (scrolledLines == 0) unread = false;
            if(before != scrolledLines) WriteOutput();
        }
    }

    
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

