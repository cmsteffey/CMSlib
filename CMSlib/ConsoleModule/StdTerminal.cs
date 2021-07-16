using System;

namespace CMSlib.ConsoleModule
{
    public class StdTerminal : ITerminal
    {
        InputRecord? ITerminal.ReadInput()
        {
            return Console.ReadKey();
        }

        void ITerminal.SetupConsole()
        {
            
        }

        void ITerminal.Write(string toWrite)
        {
            Console.Write(toWrite);
        }

        void ITerminal.SetCursorPosition(int x, int y)
        {
            Console.SetCursorPosition(x, y);
        }

        void ITerminal.SetConsoleTitle(string title)
        {
            Console.Write(AnsiEscape.WindowTitle(title[..Math.Min(256, title.Length)]));
        }

        void ITerminal.Flush()
        {
            
        }

        string ITerminal.GetClipboard()
        {
            return string.Empty;
        }

        /// <summary>
        /// Quits the app, properly returning to the main buffer and clearing all possible cursor/format options.
        /// </summary>
        void ITerminal.QuitApp(Exception e)
        {
            Console.Write(AnsiEscape.MainScreenBuffer);
            Console.Write(AnsiEscape.SoftReset);
            Console.Write(AnsiEscape.EnableCursorBlink);
            Console.WriteLine(
                e is not null ? $"CMSlib gracefully exited with an exception:\n{e}" : $"[CMSlib] Exiting gracefully.");
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        void ITerminal.FlashWindow(FlashFlags flags, uint times, int milliDelay)
        {
            
        }
    }
}