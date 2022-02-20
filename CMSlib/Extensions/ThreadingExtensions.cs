using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CMSlib.Extensions;

public static class ThreadingExtensions
{
    public static TaskAwaiter GetAwaiter(this CancellationToken token)
    {
        TaskCompletionSource tcs = new();
        token.Register(tcs.SetResult);
        return tcs.Task.GetAwaiter();
    }
}