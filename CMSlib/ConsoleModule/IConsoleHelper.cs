using System;

namespace CMSlib.ConsoleModule
{
    public interface IConsoleHelper
    {
        public InputRecord? ReadInput();
        public void SetupConsole();
        public string GetClipboard();

        public void QuitApp(Exception e);
    }
}