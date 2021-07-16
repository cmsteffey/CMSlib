using System;

namespace CMSlib.ConsoleModule
{
    public interface ITerminal
    {
        public InputRecord? ReadInput();
        public void SetupConsole();
        public string GetClipboard();
        public void QuitApp(Exception e);
        internal void Write(string toWrite);
        internal void SetCursorPosition(int x, int y);
        internal void FlashWindow(FlashFlags flags, uint times, int milliDelay);
        internal void SetConsoleTitle(string title);
        internal void Flush();
    }
}