using System;

namespace CMSlib.ConsoleModule
{
    public class StdConsoleHelper : IConsoleHelper
    {
        public InputRecord? ReadInput()
        {
            return Console.ReadKey();
        }

    }
}