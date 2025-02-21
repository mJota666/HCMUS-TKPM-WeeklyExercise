using System;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

namespace StudentManagementApp.Extensions
{
    public static class DispatcherQueueExtensions
    {
        public static Task EnqueueAsync(this DispatcherQueue dispatcherQueue, Func<Task> func)
        {
            var tcs = new TaskCompletionSource<bool>();

            bool enqueued = dispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    await func();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            if (!enqueued)
            {
                tcs.SetException(new InvalidOperationException("Unable to enqueue the task."));
            }

            return tcs.Task;
        }
    }
}
