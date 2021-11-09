using System;
using System.IO;

namespace CMSlib.ConsoleModule
{
    public class StdTerminal : ITerminal
    {
        StreamWriter _writer = null;

        InputRecord? ITerminal.ReadInput()
        {
            if (Console.IsInputRedirected) throw new NoInputException(this);
            return (InputRecord) Console.ReadKey(true);
        }

        void ITerminal.SetupConsole()
        {
            _writer = new StreamWriter(Console.OpenStandardOutput());
        }

        void ITerminal.Write(string toWrite)
        {
            _writer.Write(toWrite);
        }

        void ITerminal.SetCursorPosition(int x, int y)
        {
            Console.SetCursorPosition(x, y);
        }

        void ITerminal.SetConsoleTitle(string title)
        {
            _writer.Write(AnsiEscape.WindowTitle(title[..Math.Min(256, title.Length)]));
        }

        void ITerminal.Flush()
        {
            _writer.Flush();
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
            _writer?.Write(AnsiEscape.MainScreenBuffer);
            _writer?.Write(AnsiEscape.SoftReset);
            _writer?.Write(AnsiEscape.EnableCursorBlink);
            _writer?.WriteLine(
                e is not null ? $"CMSlib gracefully exited with an exception:\n{e}" : $"[CMSlib] Exiting gracefully.");
            _writer.Close();
            _writer.Dispose();
            System.Environment.Exit(0);
        }

        void ITerminal.FlashWindow(FlashFlags flags, uint times, int milliDelay)
        {
        }
    }
}