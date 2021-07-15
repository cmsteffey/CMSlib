using System;

namespace CMSlib.ConsoleModule
{
    public class StdTerminal : ITerminal
    {
        public InputRecord? ReadInput()
        {
            return Console.ReadKey();
        }

        public void SetupConsole()
        {
            
        }

        public string GetClipboard()
        {
            return string.Empty;
        }

        /// <summary>
        /// Quits the app, properly returning to the main buffer and clearing all possible cursor/format options.
        /// </summary>
        public void QuitApp(Exception e)
        {
            Console.Write(AnsiEscape.MainScreenBuffer);
            Console.Write(AnsiEscape.SoftReset);
            Console.Write(AnsiEscape.EnableCursorBlink);
            Console.WriteLine(
                e is not null ? $"CMSlib gracefully exited with an exception:\n{e}" : $"[CMSlib] Exiting gracefully.");
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}