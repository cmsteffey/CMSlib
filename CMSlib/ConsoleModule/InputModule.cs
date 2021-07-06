using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CMSlib.ConsoleModule
{
    public abstract class InputModule : BaseModule
    {
        internal StringBuilder inputString = new();

        protected InputModule(string title, int x, int y, int width, int height, LogLevel minLevel) : base(title, x, y, width, height, minLevel)
        {
            
        }
        
        protected event AsyncEventHandler<LineEnteredEventArgs> ReadLineLineEntered;
        internal async Task FireReadLineLineEntered(LineEnteredEventArgs args)
        {
            var handler = ReadLineLineEntered;
            if (handler is not null)
            {
                await handler(this, args);
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
            CancellationTokenSource combined = CancellationTokenSource.CreateLinkedTokenSource(waitCancel.Token, cancellationToken);

            Task Waiter(object _, LineEnteredEventArgs args)
            {
                result = args;
                waitCancel.Cancel();
                return Task.CompletedTask;
            }

            try
            {
                ReadLineLineEntered += Waiter;
                await Task.Delay(-1, combined.Token);
            }
            catch (TaskCanceledException e)
            {
                
                ReadLineLineEntered -= Waiter;
                if (cancellationToken.IsCancellationRequested)
                    throw e;
            }
            return result;
        }
    }
}