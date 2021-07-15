using System;
using System.Runtime.InteropServices;

namespace CMSlib.ConsoleModule
{
    public class WinTerminal : ITerminal
    {
        private uint prevIn;
        private uint prevOut;
        
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
            Console.SetCursorPosition(x, y);
        }
        void ITerminal.SetupConsole()
        {
            IntPtr outputHandle = GetStdHandle(-11); //CONSOLE OUTPUT
            IntPtr inputHandle = GetStdHandle(-10); //CONSOLE INPUT
            GetConsoleMode(outputHandle, out uint outMode);
            GetConsoleMode(inputHandle, out uint inMode);
            prevOut = outMode;
            prevIn = inMode;
            outMode |= 4; // ENABLE VIRTUAL TERMINAL OUTPUT
            outMode = (uint) (outMode & ~0x0002);
            SetConsoleMode(outputHandle, outMode);
            inMode = (uint) (inMode & ~0x0040); //DISABLE QUICK_EDIT MODE
            inMode = (uint) (inMode & ~0x0002); //DISABLE LINE INPUT
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
                    return System.Text.Encoding.Default.GetString(bytes);
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
            Console.Write(AnsiEscape.MainScreenBuffer);
            Console.Write(AnsiEscape.SoftReset);
            Console.Write(AnsiEscape.EnableCursorBlink);
            SetConsoleMode(GetStdHandle(-10), prevIn);
            SetConsoleMode(GetStdHandle(-11), prevOut);
            Console.WriteLine(
                e is not null ? $"CMSlib gracefully exited with an exception:\n{e}" : $"[CMSlib] Exiting gracefully.");
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        void ITerminal.Write(string toWrite)
        {
            Console.Write(toWrite);
        }
        
        
        
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadConsoleInput(
            IntPtr  hConsoleInput,
            IntPtr recordBuffer,
            uint  nLength,
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
        
        
    }
}