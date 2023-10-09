using System.Threading;
using System.Threading.Tasks;

namespace Diplomka.Wrappers
{
    public static class TaskExtension
    {
        public static async Task DelayWithCancel(this Task task, int millisecondsDelay, CancellationToken token)
        {
            try
            {
                await Task.Delay(millisecondsDelay, token);
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}
