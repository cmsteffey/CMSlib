using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CMSlib.ConsoleModule
{
    public class WinTerminal : ITerminal
    {
        private uint _prevIn;
        private uint _prevOut;
        private uint _prevInCp;
        private uint _prevOutCp;
        private StreamWriter _writer;

        InputRecord? ITerminal.ReadInput()
        {
            IntPtr inputHandle = GetStdHandle(-10);

            int hResult = WaitForSingleObject(inputHandle, -1);
            IntPtr recordPtr = Marshal.AllocHGlobal(Marshal.SizeOf<InputRecord>());

            if (hResult == int.MaxValue) return null;
            try
            {
                if (!ReadConsoleInput(inputHandle, recordPtr, 1, out uint numberOfEventsRead)) return null;
                if (numberOfEventsRead == 0u) return null;
                return Marshal.PtrToStructure(recordPtr, typeof(InputRecord)) as InputRecord?;
            }
            catch
            {
                return null;
            }
            finally
            {
                Marshal.FreeHGlobal(recordPtr);
            }
        }

        void ITerminal.SetCursorPosition(int x, int y)
        {
            _writer.Write(AnsiEscape.SetCursorPosition(x, y));
        }

        void ITerminal.SetConsoleTitle(string title)
        {
            _writer.Write(AnsiEscape.WindowTitle(title[..Math.Min(256, title.Length)]));
        }

        void ITerminal.Write(string toWrite)
        {
            _writer.Write(toWrite);
        }

        void ITerminal.Flush()
        {
            _writer.Flush();
        }

        void ITerminal.SetupConsole()
        {
            _writer = new(Console.OpenStandardOutput());
            _writer.AutoFlush = false;
            _prevInCp = GetConsoleCP();
            _prevOutCp = GetConsoleOutputCP();
            SetConsoleOutputCP(65001);
            SetConsoleCP(65001);
            IntPtr outputHandle = GetStdHandle(-11); //CONSOLE OUTPUT
            IntPtr inputHandle = GetStdHandle(-10); //CONSOLE INPUT
            GetConsoleMode(outputHandle, out uint outMode);
            GetConsoleMode(inputHandle, out uint inMode);
            _prevOut = outMode;
            _prevIn = inMode;
            outMode |= 4; // ENABLE VIRTUAL TERMINAL OUTPUT
            outMode = (uint) (outMode & ~0x0002); //DISABLE WRAP AT EOL

            SetConsoleMode(outputHandle, outMode);
            inMode = (uint) (inMode & ~0x0040); //DISABLE QUICK_EDIT MODE
            inMode = (uint) (inMode & ~0x0002); //DISABLE LINE INPUT
            inMode = (uint) (inMode & ~0x0001); //DISABLE PROCESSED INPUT
            inMode |= 0x0010; //MOUSE INPUT
            inMode |= 0x0080; //EXTENDED_FLAGS
            SetConsoleMode(inputHandle, inMode);
        }

        string ITerminal.GetClipboard()
        {
            if (OpenClipboard(GetConsoleWindow()))
            {
                IntPtr dataHandle = GetClipboardData(1);
                if (dataHandle != IntPtr.Zero)
                {
                    IntPtr contentHandle = GlobalLock(dataHandle);
                    int size = GlobalSize(contentHandle);
                    byte[] bytes = new byte[size];
                    Marshal.Copy(contentHandle, bytes, 0, size);
                    GlobalUnlock(dataHandle);
                    CloseClipboard();
                    return Encoding.Default.GetString(bytes);
                }

                CloseClipboard();
                return "you don't have text on your clipboard :(";
            }

            return String.Empty;
        }

        /// <summary>
        /// Quits the app, properly returning to the main buffer and clearing all possible cursor/format options.
        /// </summary>
        void ITerminal.QuitApp(Exception e)
        {
            _writer?.Write(AnsiEscape.MainScreenBuffer);
            _writer?.Write(AnsiEscape.SoftReset);
            _writer?.Write(AnsiEscape.EnableCursorBlink);
            _writer?.Flush();
            SetConsoleCP(_prevInCp);
            SetConsoleOutputCP(_prevOutCp);
            SetConsoleMode(GetStdHandle(-10), _prevIn);
            SetConsoleMode(GetStdHandle(-11), _prevOut);
            _writer?.WriteLine(
                e is not null ? $"CMSlib gracefully exited with an exception:\n{e}" : $"[CMSlib] Exiting gracefully.");
            _writer?.Dispose();
            Environment.Exit(0);
        }

        void ITerminal.FlashWindow(FlashFlags flags, uint times, int milliDelay)
        {
            FlashInfo info = new();
            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
            info.dwFlags = flags;
            info.hWnd = GetConsoleWindow();
            info.uCount = times;
            info.dwTimeOut = milliDelay;

            FlashWindowEx(ref info);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int FlashWindowEx(ref FlashInfo info);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadConsoleInput(
            IntPtr hConsoleInput,
            IntPtr recordBuffer,
            uint nLength,
            out uint lpNumberOfEventsRead
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool OpenClipboard(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseClipboard();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetClipboardData(uint format);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalLock(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalUnlock(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GlobalSize(IntPtr hmem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int SetConsoleOutputCP(uint cp);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int SetConsoleCP(uint cp);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetConsoleOutputCP();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetConsoleCP();
    }
}