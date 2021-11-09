using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CMSlib.CollectionTypes;
using Microsoft.Extensions.Logging;

namespace CMSlib.ConsoleModule
{
    public abstract class InputModule : BaseModule
    {
        internal StringBuilder inputString = new();
        private FifoReverseBuffer<string> prevInput = new(50);
        private int historyPointer = -1;
        internal bool usingHistory = false;

        protected InputModule(string title, int x, int y, int width, int height, LogLevel minimumLogLevel) : base(title,
            x, y, width, height, minimumLogLevel)
        {
        }

        internal void AddToHistory(string line)
        {
            prevInput.Add(line);
            if (usingHistory)
                historyPointer++;
            else
                historyPointer = -1;
        }

        internal void ScrollHistory(int amt)
        {
            usingHistory = true;
            int before = historyPointer;
            historyPointer = Math.Clamp(historyPointer + amt, 0, prevInput.Count == 0 ? 0 : prevInput.Count - 1);
            if (before == historyPointer || historyPointer < 0 || historyPointer >= prevInput.Count) return;
            inputString.Append(prevInput[historyPointer]);
            lrCursorPos = Math.Min(prevInput[historyPointer].Length, Width - 3);
            this.WriteOutput();
        }


        internal abstract void AddChar(char toAdd);
        internal abstract void Backspace(bool write = true);

        /// <summary>
        /// Event fired when a line is entered into this module
        /// </summary>
        public event EventHandler<LineEnteredEventArgs> LineEntered;


        internal void FireLineEntered(LineEnteredEventArgs args)
        {
            var handler = LineEntered;
            if (handler is not null)
            {
                handler(this, args);
            }
        }


        protected event EventHandler<LineEnteredEventArgs> ReadLineLineEntered;

        internal void FireReadLineLineEntered(LineEnteredEventArgs args)
        {
            var handler = ReadLineLineEntered;
            if (handler is not null)
            {
                handler(this, args);
            }
        }

        /// <summary>
        /// Reads and returns the next line entered into this module. DO NOT call this method inside a LineEntered event handler.
        /// </summary>
        /// <returns>LineEnteredEventArgs which hold the line and a reference to this module</returns>
        /// <exception cref="TaskCanceledException">Thrown when the cancellation token provided is cancelled</exception>
        public async Task<LineEnteredEventArgs> ReadLineAsync(CancellationToken cancellationToken = default)
        {
            LineEnteredEventArgs result = null;
            CancellationTokenSource waitCancel = new();
            CancellationTokenSource combined =
                CancellationTokenSource.CreateLinkedTokenSource(waitCancel.Token, cancellationToken);

            void Waiter(object _, LineEnteredEventArgs args)
            {
                result = args;
                waitCancel.Cancel();
            }


            ReadLineLineEntered += Waiter;
            combined.Token.WaitHandle.WaitOne();
            ReadLineLineEntered -= Waiter;

            return result;
        }
    }
}