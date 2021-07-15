using System;

namespace CMSlib.ConsoleModule
{
    public interface ITerminal
    {
        public InputRecord? ReadInput();
        public void SetupConsole();
        public string GetClipboard();

        public void QuitApp(Exception e);
    }
}