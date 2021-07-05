using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CMSlib.ConsoleModule
{
    public interface IModule : ILogger
    {
        int X { get; }
        int Y { get; }
        int Width { get; }
        int Height { get; }
        string Title { get; }
        string InputClear { get; }

        /// <summary>
        /// This string is shown at the top of the module. Setting it to null, or not setting it at all, uses the module title as the displayed title.
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// Event fired when a line is entered into this module
        /// </summary>
        event Module.AsyncEventHandler<LineEnteredEventArgs> LineEntered;

        /// <summary>
        /// Event fired when a key is pressed while this module is focused
        /// </summary>
        event Module.AsyncEventHandler<KeyEnteredEventArgs> KeyEntered;

        event Module.AsyncEventHandler<LineEnteredEventArgs> ReadLineLineEntered;

        /// <summary>
        /// Reads and returns the next line entered into this module. DO NOT call this method inside a LineEntered event handler.
        /// </summary>
        /// <returns>LineEnteredEventArgs which hold the line and a reference to this module</returns>
        Task<LineEnteredEventArgs> ReadLineAsync();

        /// <summary>
        /// Clears all lines from this module, as well as optionally refreshing.
        /// </summary>
        /// <param name="refresh">Whether to refresh after clearing out the text</param>
        void Clear(bool refresh = true);

        /// <summary>
        /// Adds line(s) of text to this module. This supports \n, and \n will properly add text to the next line.
        /// </summary>
        /// <param name="text">The text to add</param>
        void AddText(string text);

        void AddText(object obj);

        /// <summary>
        /// Gets the string representation of this Module.
        /// </summary>
        /// <returns>The string representation of this module.</returns>
        string ToString();

        IEnumerable<string> ToOutputLines();

        /// <summary>
        /// Refreshes this module, showing the latest output.
        /// </summary>
        void WriteOutput();

        Module ToInputModule();
        bool IsEnabled(LogLevel logLevel);

        /// <summary>
        /// NOT IMPL'D
        /// </summary>
        /// <param name="state"></param>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IDisposable BeginScope<TState>(TState state);

        void ScrollUp(int amt);
        void ScrollDown(int amt);
        void ScrollTo(int line);
        Task FireLineEnteredAsync(LineEnteredEventArgs args);
        Task FireKeyEnteredAsync(KeyEnteredEventArgs args);
        Task FireReadLineLineEntered(LineEnteredEventArgs args);
    }
}